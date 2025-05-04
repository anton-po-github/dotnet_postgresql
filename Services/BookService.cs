using MongoDB.Bson;
using MongoDB.Driver;

public class BookService
{
    private readonly MongoDBContext _mongoDBContext;
    private readonly FileService _fileService;

    public BookService(MongoDBSettings mongoDBSettings, FileService fileService)
    {
        _mongoDBContext = new MongoDBContext(mongoDBSettings);
        _fileService = fileService;
    }

    public async Task<Book> CreateAsyncBook(Book newBook, byte[] iconData)
    {
        // Upload raw bytes to GridFS and return ObjectId
        var objectId = await _fileService.UploadBytesAsync(newBook.IconFileName, iconData);

        // Prepare Book entity
        var book = new Book
        {
            BookName = newBook.BookName,
            Price = newBook.Price,
            Category = newBook.Category,
            Author = newBook.Author,
            IconId = objectId.ToString(),
            IconPath = newBook.IconBase64
        };

        _mongoDBContext.Books.InsertOne(book);

        return book;
    }

    public async Task<Book> UpdateAsyncBook(
        string id,
        Book newBook,
        byte[] iconData)
    {
        // 1) Проверяем наличие книги
        var filter = Builders<Book>.Filter.Eq(b => b.Id, id);
        var existing = await _mongoDBContext.Books.Find(filter).FirstOrDefaultAsync();
        if (existing == null)
            throw new AppException("Book not found");

        // 2. Удаляем старый файл, если он был
        if (ObjectId.TryParse(existing.IconId, out var oldObjectId))
        {
            await _fileService.DeleteFileAsync(oldObjectId);
        }

        // 2) Загружаем новый файл
        var newObjectId = await _fileService.UploadBytesAsync(
            newBook.IconFileName,
            iconData
        );

        // 3) Собираем UpdateDefinition — обновляем только нужные поля
        var update = Builders<Book>.Update
            .Set(b => b.BookName, newBook.BookName)
            .Set(b => b.Price, newBook.Price)
            .Set(b => b.Category, newBook.Category)
            .Set(b => b.Author, newBook.Author)
            .Set(b => b.IconId, newObjectId.ToString())
            .Set(b => b.IconPath, newBook.IconBase64);

        // 4) Применяем атомарно
        await _mongoDBContext.Books.UpdateOneAsync(filter, update);

        // 5) Возвращаем уже обновлённый документ
        return (await _mongoDBContext.Books.Find(filter).FirstAsync())!;
    }

    public async Task<Book?> GetByIdAsync(string id)
    {
        return await _mongoDBContext.Books.Find(b => b.Id == id).FirstOrDefaultAsync();
    }

    public List<Book> Get() => _mongoDBContext.Books.Find(book => true).ToList();

    public Book GetOneBook(string id) => _mongoDBContext.Books.Find<Book>(book => book.Id == id).FirstOrDefault();

    public void Remove(Book bookIn, string iconId)
    {
        _mongoDBContext.Books.DeleteOne(book => book.Id == bookIn.Id);

        _fileService.DeleteFile(iconId);
    }

    public void Remove(string id) => _mongoDBContext.Books.DeleteOne(book => book.Id == id);
}

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

    public async Task<Book> Update(string id, Book newBook, byte[] iconData)
    {
        var objectId = await _fileService.UploadBytesAsync(newBook.IconFileName, iconData);

        newBook.IconId = objectId.ToString();

        _mongoDBContext.Books.ReplaceOne(book => book.Id == id, newBook);

        return newBook;
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

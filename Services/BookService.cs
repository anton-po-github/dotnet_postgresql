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

    public List<Book> Get() =>
        _mongoDBContext.Books.Find(book => true).ToList();

    public Book GetOneBook(string id) =>
        _mongoDBContext.Books.Find<Book>(book => book.Id == id).FirstOrDefault();

    public async Task<Book> Create(Book book, IFormFile file)
    {
        var objectId = await _fileService.UploadFile(file);

        book.IconId = objectId.ToString();

        book.IconPath = await _fileService.GetBytesByName(file.FileName);

        _mongoDBContext.Books.InsertOne(book);

        return book;
    }

    public async Task<Book> Update(string id, Book bookIn, IFormFile file)
    {
        var objectId = await _fileService.UploadFile(file);

        bookIn.IconId = objectId.ToString();

        bookIn.IconPath = await _fileService.GetBytesByName(file.FileName);

        _mongoDBContext.Books.ReplaceOne(book => book.Id == id, bookIn);

        return bookIn;
    }


    public void Remove(Book bookIn, string iconId)
    {
        _mongoDBContext.Books.DeleteOne(book => book.Id == bookIn.Id);

        _fileService.DeleteFile(iconId);
    }

    public void Remove(string id) =>
        _mongoDBContext.Books.DeleteOne(book => book.Id == id);
}

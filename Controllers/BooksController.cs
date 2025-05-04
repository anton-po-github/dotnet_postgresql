using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Produces("application/json")]
[Consumes("application/json", "multipart/form-data")]
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BookService _bookService;

    public BooksController(BookService bookService)
    {
        _bookService = bookService;
    }

    [Authorize(Roles = "User")]
    [HttpGet]
    public ActionResult<List<Book>> Get() => _bookService.Get();

    [HttpGet("{id:length(24)}", Name = "GetBook")]
    public ActionResult<Book> Get(string id)
    {
        var book = _bookService.GetOneBook(id);

        if (book == null)
        {
            return NotFound();
        }

        return book;
    }

    [HttpPost]
    public async Task<Book> Create([FromBody] Book newBook)
    {
        if (newBook == null)
            throw new AppException("Request body is empty.");

        var iconBytes = getIconBytes(newBook);

        // 2. Call service, passing DTO and decoded bytes
        var book = await _bookService.CreateAsyncBook(newBook, iconBytes);

        return book;
    }

    [HttpPut("{id:length(24)}")]
    public async Task<Book> Update(string id, [FromBody] Book newBook)
    {
        if (newBook == null)
            throw new AppException("Request body is empty.");

        var book = _bookService.GetOneBook(id);
        if (book == null)
            throw new AppException("The book is not found.");

        var iconBytes = getIconBytes(newBook);

        // 2. Call service, passing DTO and decoded bytes
        book = await _bookService.Update(id, newBook, iconBytes);

        return book;
    }

    [HttpDelete("{id:length(24)}")]
    public bool Delete(string id)
    {
        var book = _bookService.GetOneBook(id);

        if (book == null || string.IsNullOrEmpty(book.IconId))
        {
            return false;
        }

        _bookService.Remove(book, book.IconId);

        return true;
    }

    private byte[] getIconBytes(Book newBook)
    {
        // 1. Decode Base64 string into byte[]
        byte[] iconBytes;
        try
        {
            iconBytes = Convert.FromBase64String(newBook.IconBase64);
        }
        catch (FormatException)
        {
            throw new AppException("IconBase64 is not a valid Base64 string."); ;
        }

        return iconBytes;
    }
}

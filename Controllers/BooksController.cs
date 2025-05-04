using Newtonsoft.Json;
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
    public async Task<Book> Create(
     [FromForm(Name = "icon")] IFormFile file,
     [FromForm(Name = "body")] string body)
    {
        // 1. Check the basic arguments
        if (string.IsNullOrWhiteSpace(body))
            throw new AppException("Request body is empty.");

        // 2. Deserialize into a nullable variable and immediately check for null
        Book? book = JsonConvert.DeserializeObject<Book>(body);
        if (book is null)
            throw new AppException("Invalid book data in request.");

        // 4. Call the service, ensuring that book is not null
        return await _bookService.Create(book, file);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<ActionResult<Book>> Update(string id, Book updateBook)
    {
        var book = _bookService.GetOneBook(id);

        if (book == null)
        {
            return NotFound();
        }

        book.BookName = updateBook.BookName;
        book.Category = updateBook.Category;
        book.Price = updateBook.Price;
        book.Author = updateBook.Author;

        var newBook = await _bookService.Update(id, book);

        return newBook;
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
}

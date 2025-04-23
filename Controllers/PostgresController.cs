using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PostgresController : ControllerBase
{
    private PostgresService _postgresService;
    public PostgresController(PostgresService postgresService)
    {
        _postgresService = postgresService;
    }

    [HttpGet("users")]
    public IActionResult GetAllPostgresUsers()
    {
        var users = _postgresService.GetAllPostgresUsers();
        return Ok(users);
    }

    [HttpGet("products")]
    public IActionResult GetAllPostgresProducts()
    {
        var products = _postgresService.GetAllPostgresProducts();
        return Ok(products);
    }

    [HttpPut("users/{id}")]
    public IActionResult Update(int id, UpdatePostgresUserModel model)
    {
        _postgresService.UpdatePostgresUser(id, model);

        return Ok(new { message = "User updated" });
    }

    [HttpDelete("users/{id}")]
    public IActionResult DeletePostgresUser(int id)
    {
        _postgresService.DeletePostgresUser(id);
        return Ok(new { message = "User deleted" });
    }

}

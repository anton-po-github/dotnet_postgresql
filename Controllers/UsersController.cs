using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private UserService _userService;
    private IMapper _mapper;

    private readonly IGenericService<User> _usersGenericService;

    public UsersController(
        UserService userService,
        IGenericService<User> usersGenericService,
        IMapper mapper)
    {
        _usersGenericService = usersGenericService;
        _userService = userService;
        _mapper = mapper;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<Pagination<User>>> GetUsers([FromQuery] UserSpecParams userSpecParams)
    {
        var spec = new UsersParamsSpec(userSpecParams);

        var countSpec = new UsersParamsCountSpec(userSpecParams);

        var countItems = await _usersGenericService.CountAsync(countSpec);

        var users = await _usersGenericService.ListAsync(spec);

        // var data = _mapper.Map<IReadOnlyList<User>, IReadOnlyList<UserToReturnPagDto>>(products);

        return Ok(new Pagination<User>(userSpecParams.PageIndex, userSpecParams.PageSize, countItems, users));
    }

    /*    [Authorize]
       [HttpGet]
       public IActionResult GetAll()
       {
           var users = _userService.GetAll();
           return Ok(users);
       } */

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var user = _userService.GetById(id);
        return Ok(user);
    }

    [HttpPost]
    public IActionResult Create(CreateRequest model)
    {
        _userService.Create(model);
        return Ok(new { message = "User created" });
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, UpdateRequest model)
    {
        _userService.Update(id, model);
        return Ok(new { message = "User updated" });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _userService.Delete(id);
        return Ok(new { message = "User deleted" });
    }
}
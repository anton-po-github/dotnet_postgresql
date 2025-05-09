﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
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

    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<ActionResult<Pagination<User>>> GetUsers([FromQuery] UserSpecParams userSpecParams)
    {
        var spec = new UsersParamsSpec(userSpecParams);

        var countSpec = new UsersParamsCountSpec(userSpecParams);

        var countItems = await _usersGenericService.CountAsync(countSpec);

        var users = await _usersGenericService.ListAsync(spec);

        return Ok(new Pagination<User>(userSpecParams.PageIndex, userSpecParams.PageSize, countItems, users));
    }


    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        var user = _userService.GetById(id);
        return Ok(user);
    }

    [HttpPost]
    public IActionResult Create([FromForm] AddUpdateUser addUser)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null) throw new AppException("UserId not found.");

        _userService.Create(addUser, userId);

        return Ok(new { message = "User created" });
    }

    [HttpPut("{id}")]
    public IActionResult Update(Guid id, [FromForm] AddUpdateUser updateUser)
    {

        _userService.Update(id, updateUser);

        return Ok(new { message = "User updated" });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        _userService.Delete(id);
        return Ok(new { message = "User deleted" });
    }
}

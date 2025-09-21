using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

public class UserService
{
    private UsersContext _userContext;
    private readonly IMapper _mapper;

    public UserService(
        UsersContext userContext,
        UserManager<IdentityUser> userManager,
        IMapper mapper
    )
    {
        _userContext = userContext;
        _mapper = mapper;
    }

    public IEnumerable<User> GetAll()
    {
        return _userContext.Users;
    }

    public User GetById(Guid id)
    {
        return getUser(id);
    }

    public void Create(AddUpdateUser addUser, int userId)
    {
        // validate
        if (_userContext.Users.Any(x => x.Email == addUser.Email))
            throw new AppException("User with the email '" + addUser.Email + "' already exists");

        // map model to new user object
        //  var user = _mapper.Map<User>(addUser);
        var user = new User
        {
            FirstName = addUser.FirstName,
            LastName = addUser.LastName,
            Email = addUser.Email,
            PhotoType = addUser.PhotoType,
            Phone = "937-99-92",
            OwnerId = userId,
            PhotoUrl = ConvertIFormFileToByteArray(addUser.File),
        };

        var userDetails = new UsersDetails { Role = "User", Salary = 1000 };

        user.Details = JsonSerializer.Serialize(userDetails);
        // save user
        _userContext.Users.Add(user);

        _userContext.SaveChanges();
    }

    public void Update(Guid id, AddUpdateUser updateUser)
    {
        var user = getUser(id);

        // validate
        if (
            updateUser.Email != user.Email
            && _userContext.Users.Any(x => x.Email == updateUser.Email)
        )
            throw new AppException("User with the email '" + updateUser.Email + "' already exists");

        user.FirstName = updateUser.FirstName;
        user.LastName = updateUser.LastName;
        user.PhotoType = updateUser.PhotoType;
        user.Email = updateUser.Email;
        user.Phone = "937-99-92";
        user.PhotoUrl = ConvertIFormFileToByteArray(updateUser.File);

        _userContext.Users.Update(user);

        _userContext.SaveChanges();
    }

    public void Delete(Guid id)
    {
        var user = getUser(id);

        _userContext.Users.Remove(user);

        _userContext.SaveChanges();
    }

    private byte[] ConvertIFormFileToByteArray(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            throw new AppException("File not found.");
        ;

        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }
    }

    // helper methods
    private User getUser(Guid id)
    {
        var user = _userContext.Users.Find(id);

        if (user == null)
            throw new System.Collections.Generic.KeyNotFoundException("User not found");

        return user;
    }
}

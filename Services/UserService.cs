using System.Text.Json;
using AutoMapper;

public class UserService
{
    private UsersContext _userContext;
    private readonly IMapper _mapper;

    public UserService(
        UsersContext userContext,
        IMapper mapper)
    {
        _userContext = userContext;
        _mapper = mapper;
    }

    public IEnumerable<User> GetAll()
    {
        return _userContext.Users;
    }

    public User GetById(int id)
    {
        return getUser(id);
    }

    public void Create(CreateRequest model)
    {
        // validate
        if (_userContext.Users.Any(x => x.Email == model.Email))
            throw new AppException("User with the email '" + model.Email + "' already exists");

        // map model to new user object
        var user = _mapper.Map<User>(model);

        user.Phone = "some_phone_here";

        var userDetails = new UsersDetails
        {
            Role = "User",
            Salary = 1000
        };

        user.Details = JsonSerializer.Serialize(userDetails);

        // save user
        _userContext.Users.Add(user);
        _userContext.SaveChanges();
    }

    public void Update(int id, UpdateRequest model)
    {
        var user = getUser(id);

        // validate
        if (model.Email != user.Email && _userContext.Users.Any(x => x.Email == model.Email))
            throw new AppException("User with the email '" + model.Email + "' already exists");

        user.Phone = "updated_phone_here";
        // copy model to user and save
        _mapper.Map(model, user);
        _userContext.Users.Update(user);
        _userContext.SaveChanges();
    }

    public void Delete(int id)
    {
        var user = getUser(id);
        _userContext.Users.Remove(user);
        _userContext.SaveChanges();
    }

    // helper methods

    private User getUser(int id)
    {
        var user = _userContext.Users.Find(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        return user;
    }
}


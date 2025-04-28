using System.Text.Json;
using AutoMapper;
using dotnet_postgresql.DbContexts;
using dotnet_postgresql.Entities;
using dotnet_postgresql.Helpers;

namespace dotnet_postgresql.Services
{
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

        public User GetById(Guid id)
        {
            return getUser(id);
        }

        public void Create(AddUpdateUser addUser)
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
                Phone = "937-99-92",
                PhotoUrl = ConvertIFormFileToByteArray(addUser.File)
            };

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

        public void Update(Guid id, AddUpdateUser updateUser)
        {
            var user = getUser(id);

            // validate
            if (updateUser.Email != user.Email && _userContext.Users.Any(x => x.Email == updateUser.Email))
                throw new AppException("User with the email '" + updateUser.Email + "' already exists");

            user.FirstName = updateUser.FirstName;
            user.LastName = updateUser.LastName;
            user.Email = updateUser.Email;
            user.Phone = "937-99-92";
            user.PhotoUrl = ConvertIFormFileToByteArray(updateUser.File);

            // copy model to user and save
            // _mapper.Map(model, user);

            _userContext.Users.Update(user);

            _userContext.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var user = getUser(id);

            _userContext.Users.Remove(user);

            _userContext.SaveChanges();
        }

        private byte[] ConvertIFormFileToByteArray(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

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

            if (user == null) throw new System.Collections.Generic.KeyNotFoundException("User not found");

            return user;
        }
    }


}

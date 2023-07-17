using System.Data;
using ChatR.Data;
using ChatR.Dto;
using MongoDB.Driver;

namespace ChatR.Service;

public class AuthService
{
    private readonly MongoDbContext _mongoContext;
    private readonly IMongoCollection<User> _usersCollection;

    public AuthService(MongoDbContext mongoContext)
    {
        _mongoContext = mongoContext;
        var database = _mongoContext.GetDatabase("ChatDB");
        _usersCollection = database.GetCollection<User>("users");
    }

    public async Task<string> Signup(SignupDto signupDto)
    {
        var user = new User
        {
            Username = signupDto.Username,
            Email = signupDto.Email,
            Password = signupDto.Password,
            CreatedAt = DateTime.Now
        };

        var alreadyAUser = await _usersCollection
            .Find(x => x.Username == signupDto.Username || x.Email == signupDto.Email)
            .AnyAsync();

        if (alreadyAUser) throw new DuplicateNameException("A user with that name or email already existsA");

        await _usersCollection.InsertOneAsync(user);
        return user.Username;
    }

    public async Task<string> Login(LoginDto loginDto)
    {
        var user = await _usersCollection
            .Find(x => x.Username == loginDto.Username && x.Password == loginDto.Password)
            .AnyAsync();

        if (!user) throw new Exception("User does not exist");

        return loginDto.Username;
    }
}
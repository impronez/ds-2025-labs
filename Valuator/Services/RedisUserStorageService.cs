using Microsoft.AspNetCore.Identity;
using Services.Common;
using StackExchange.Redis;
using Valuator.Models;

namespace Valuator.Services;

public class RedisUserStorageService : IUserStorageService
{
    private const string RedisUsersShardKey = "USERS";
    
    private readonly IDatabase _database;
    private readonly IPasswordHasher<User> _passwordHasher;

    public RedisUserStorageService(string redisPassword)
    {
        string connectionString = EnvironmentConfiguration.GetRedisConnectionString(RedisUsersShardKey);

        var configurationOptions = new ConfigurationOptions
        {
            Password = redisPassword,
            EndPoints = { connectionString }
        };

        IConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(configurationOptions);
        _database = multiplexer.GetDatabase();

        _passwordHasher = new PasswordHasher<User>();
    }
    
    public async Task<User?> FindByUserNameAsync(string username)
    {
        string redisKey = GetRedisKey(username);
        HashEntry[] hashEntries = await _database.HashGetAllAsync(redisKey);

        if (hashEntries.Length == 0)
        {
            return null;
        }

        var user = new User
        {
            UserName = username,
            Id = Guid.TryParse(
                hashEntries.FirstOrDefault(x => x.Name == "Id").Value, out var guid)
                ? guid
                : Guid.Empty,
            PasswordHash = hashEntries.FirstOrDefault(x => x.Name == "PasswordHash").Value.HasValue
                ? hashEntries.FirstOrDefault(x => x.Name == "PasswordHash").Value.ToString()
                : string.Empty
        };

        return user;
    }

    public async Task<User> CreateAsync(string userName, string password)
    {
        var user = new User
        {
            UserName = userName
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        string redisKey = GetRedisKey(user.UserName);
        var hashEntries = new HashEntry[]
        {
            new("Id", user.Id.ToString()),
            new("PasswordHash", user.PasswordHash)
        };

        await _database.HashSetAsync(redisKey, hashEntries);

        return user;
    }
    
    private static string GetRedisKey(string username) => $"user:login:{username}";
}
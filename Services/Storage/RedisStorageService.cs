using StackExchange.Redis;

namespace Services.Storage
{
	public class RedisStorageService : IStorageService
	{
		public readonly IDatabase _database;

		public RedisStorageService(IConnectionMultiplexer redis)
		{
			_database = redis.GetDatabase();
		}

		public void Save(string key, string value)
		{
			_database.StringSet(key, value);
		}

		public string? GetValue(string key)
		{
			return _database.StringGet(key);
		}

		public List<string> GetAllValuesByKeyPrefix(string keyPrefix)
		{
			var values = new List<string>();

			var textsKeys = (RedisResult[])_database.Execute("KEYS", $"{keyPrefix}-*");

			foreach (RedisResult key in textsKeys)
			{
				values.Add(GetValue(key.ToString()));
			}

			return values;
		}
	}
}

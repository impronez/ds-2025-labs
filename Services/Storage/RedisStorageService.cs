using Services.Common;
using StackExchange.Redis;

namespace Services.Storage
{
    public class RedisStorageService : IStorageService
    {
        public RedisStorageService()
        {
        }

        public void SaveShardKey(string id, string shardKey)
        {
            IDatabase database = ConnectToDatabase("MAIN");

            database.StringSet(id, shardKey.ToUpper());
            
            Console.WriteLine($"LOOKUP: {id}, {shardKey}");
        }

        public void SaveById(string key, string value, string id)
        {
            string shardKey = GetShardKey(id);
            
            IDatabase database = ConnectToDatabase(shardKey);
            
            database.StringSet(key, value);
            
            Console.WriteLine($"LOOKUP: {id}, {shardKey}");
        }

        public void SaveByShardKey(string key, string value, string shardKey)
        {
            IDatabase database = ConnectToDatabase(shardKey);
            
            database.StringSet(key, value);
        }

        public string? GetById(string id, string key)
        {
            string shardKey = GetShardKey(id);
            
            IDatabase database = ConnectToDatabase(shardKey);
            
            Console.WriteLine($"LOOKUP: {id}, {shardKey}");
            
            return database.StringGet(key);
        }

        public string? GetByShardKey(string shardKey, string key)
        {
            IDatabase database = ConnectToDatabase(shardKey);
            
            return database.StringGet(key);
        }

        public List<string> GetAllValuesByKeyPrefix(string keyPrefix, string shardKey)
        {
            IDatabase database = ConnectToDatabase(shardKey);
            
            var values = new List<string>();

            var textsKeys = (RedisResult[])database.Execute("KEYS", $"{keyPrefix}-*");

            foreach (RedisResult key in textsKeys)
            {
                values.Add(GetByShardKey(shardKey, key.ToString()));
            }

            return values;
        }
        
        private string GetShardKey(string id)
        {
            IDatabase database = ConnectToDatabase("MAIN");
            
            var shardKey = database.StringGet(id);
            if (string.IsNullOrEmpty(shardKey))
            {
                throw new KeyNotFoundException("Shard key not found");
            }
            
            return shardKey!;
        }

        private IDatabase ConnectToDatabase(string shardKey)
        {
            string connectionString = EnvironmentConfiguration.GetRedisConnectionString(shardKey);

            IConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);
            
            return redis.GetDatabase();
        }
    }
}
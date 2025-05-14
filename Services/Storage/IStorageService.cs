namespace Services.Storage
{
	public interface IStorageService
	{
		public void SaveShardKey(string id, string shardKey);
		public void SaveById(string key, string value, string id);
		public void SaveByShardKey(string key, string value, string shardKey);
		public string? GetById(string id, string key);
		public string? GetByShardKey(string shardKey, string key);
		public List<string> GetAllValuesByKeyPrefix(string keyPrefix, string shardKey);
	}
}

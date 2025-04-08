namespace Services.Storages
{
	public interface IStorageService
	{
		public void Save(string key, string value);
		public string? GetValue(string key);
		public List<string> GetAllValuesByKeyPrefix(string keyPrefix);
	}
}

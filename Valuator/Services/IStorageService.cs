namespace Valuator.Services
{
	public interface IStorageService
	{
		public void SetValue(string key, string value);
		public string? GetValue(string key);
		public List<string> GetAllValuesByKeyPrefix(string keyPrefix);
	}
}

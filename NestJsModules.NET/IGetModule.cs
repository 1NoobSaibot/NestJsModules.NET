namespace NestJsModules
{
	public interface IGetModule
	{
		object? Get(string key);
		T? Get<T>(string key);
		T? Get<T>();
	}
}

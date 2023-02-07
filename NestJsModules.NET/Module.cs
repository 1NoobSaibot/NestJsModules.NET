namespace NestJsModules
{
	public class Module
	{
		private Dictionary<string, object?> _singletoneScope = new Dictionary<string, object?>();


		public void Add<T>(string key, T value)
		{
			_singletoneScope[key] = value;
		}


		public void Add<T>(T? obj)
		{
			_singletoneScope.Add(typeof(T).ToString(), obj);
		}


		public T? Get<T>(string key)
		{
			return (T?)_singletoneScope[key];
		}


		public T? Get<T>()
		{
			return (T?)_singletoneScope[typeof(T).ToString()];
		}
	}
}
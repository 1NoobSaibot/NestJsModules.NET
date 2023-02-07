namespace NestJsModules
{
	public class Module
	{
		private Dictionary<string, object?> _singletoneScope = new Dictionary<string, object?>();
		private Dictionary<string, object?> _exports = new Dictionary<string, object?>();
		private List<Module> _children = new List<Module>();


		#region AddMethods
		public void Add(string key, object? value)
		{
			_singletoneScope[key] = value;
		}


		public void Add<T>(string key, T? value)
		{
			_singletoneScope[key] = value;
		}


		public void Add<T>(T? obj)
		{
			Add(typeof(T).ToString(), obj);
		}


		public void AddForExport(string key, object? value)
		{
			_exports[key] = value;
		}


		public void AddForExport<T>(string key, T? value)
		{
			_exports[key] = value;
		}


		public void AddForExport<T>(T? obj)
		{
			AddForExport(typeof(T).ToString(), obj);
		}
		#endregion


		#region GetMethods
		public object? Get(string key)
		{
			return _singletoneScope.GetValueOrDefault(key)
				?? _exports.GetValueOrDefault(key)
				?? _FindFirstInChildrenExports(key);
		}


		public T? Get<T>(string key)
		{
			return (T?)Get(key);
		}


		public T? Get<T>()
		{
			return Get<T>(typeof(T).ToString());
		}
		#endregion


		#region Import
		public void Import(Module subModules)
		{
			_children.Add(subModules);
		}


		public void Import(IEnumerable<Module> subModules)
		{
			_children.AddRange(subModules);
		}
		#endregion


		private object? _GetExport(string key)
		{
			return _exports[key];
		}


		private object? _FindFirstInChildrenExports(string key)
		{
			foreach (var module in _children)
			{
				try
				{
					return module._GetExport(key);
				}
				catch (KeyNotFoundException)
				{
					continue;
				}
			}

			throw new KeyNotFoundException($"Dependency {key} was not provided");
		}
	}
}
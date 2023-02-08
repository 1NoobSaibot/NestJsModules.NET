namespace NestJsModules
{
	public class Module : IGetModule
	{
		private Dictionary<string, Binding> _bindings
			= new Dictionary<string, Binding>();
		private List<Module> _children = new List<Module>();


		#region Binding
		public void BindValue<T>(string key, T? value, bool forExport = false)
		{
			if (forExport)
			{
				_bindings[key] = Binding.BindValueForExport(value);
			}
			else
			{
				_bindings[key] = Binding.BindValue(value);
			}
		}


		public void BindValue<T>(T? obj)
		{
			string key = typeof(T).ToString();
			BindValue(key, obj);
		}


		public void InstantiateValue<TBase, TDerived>(bool forExport = false) where TDerived : TBase
		{
			string key = typeof(TBase).ToString();
			Type typeInfo = typeof(TDerived);

			if (forExport)
			{
				_bindings[key] = Binding.InstantiateForExport(typeInfo);
			}
			else
			{
				_bindings[key] = Binding.Instantiate(typeInfo);
			}
		}


		public void InstantiateValue<T>(bool forExport = false)
		{
			InstantiateValue<T, T>(forExport);
		}


		public void BindValueForExport<T>(string key, T? value)
		{
			BindValue(key, value, true);
		}


		public void BindValueForExport<T>(T? obj)
		{
			BindValueForExport(typeof(T).ToString(), obj);
		}


		public void InstantiateValueForExport<TBase, TDerived>() where TDerived : TBase
		{
			InstantiateValue<TBase, TDerived>(true);
		}


		public void InstantiateValueForExport<T>()
		{
			InstantiateValueForExport<T, T>();
		}
		#endregion


		#region GetMethods
		public object? Get(string key)
		{
			if (_bindings.ContainsKey(key))
			{
				var binding = _bindings[key];
				return binding.GetValue(this);
			}

			return _FindFirstInChildrenExports(key);
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
			var binding = _bindings[key];
			if (binding.IsExported)
			{
				return binding.GetValue(this);
			}

			throw new KeyNotFoundException($"A value by key {key} was not provided or provided not for export");
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
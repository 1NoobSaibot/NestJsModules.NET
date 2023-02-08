using System.Reflection;

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


		public void Add<TBase, TDerived>() where TDerived : TBase
		{
			Type type = typeof(TDerived);
			if (type.IsInterface || type.IsAbstract)
			{
				throw new ArgumentException($"Cannot instantiate interface or abstract type {type.Name}");
			}

			Add(typeof(TBase).ToString(), typeof(TDerived));
		}


		public void Add<T>()
		{
			Add<T, T>();
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


		public void AddForExport<TBase, TDerived>() where TDerived : TBase
		{
			Type type = typeof(TDerived);
			if (type.IsInterface || type.IsAbstract)
			{
				throw new ArgumentException($"Cannot instantiate interface or abstract type {type.Name}");
			}
			AddForExport(typeof(TBase).ToString(), type);
		}


		public void AddForExport<T>()
		{
			AddForExport<T, T>();
		}
		#endregion


		#region GetMethods
		public object? Get(string key)
		{
			if (_singletoneScope.ContainsKey(key))
			{
				var value = _singletoneScope[key];
				return _CheckValue(key, value, _singletoneScope);
			}

			if (_exports.ContainsKey(key))
			{
				var value = _exports[key];
				return _CheckValue(key, value, _exports);
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
			var value = _exports[key];
			return _CheckValue(key, value, _exports);
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


		private object? _CheckValue(string key, object? value, Dictionary<string, object?> dict)
		{
			if (value is Type type)
			{
				object instance = _CreateInstance(type);
				dict[key] = instance;
				return instance;
			}

			return value;
		}


		private object _CreateInstance(Type type)
		{
			object instance = _FindConstructorAndConstruct(type);
			_InjectProps(type, instance);
			_InjectFields(type, instance);
			return instance;
		}


		private void _InjectProps(Type type, object instance)
		{
			foreach (PropertyInfo prop in type.GetRuntimeProperties())
			{
				var attrs = prop.GetCustomAttributes();
				Inject? data = attrs.FirstOrDefault((attr) => attr is Inject) as Inject;
				
				if (data != null)
				{
					string key = data.Key ?? prop.PropertyType.ToString();
					object? value = Get(key);
					prop.SetValue(instance, value);
				}
			}
		}


		private void _InjectFields(Type type, object instance)
		{
			foreach (FieldInfo field in type.GetRuntimeFields())
			{
				var attrs = field.GetCustomAttributes();
				Inject? data = attrs.FirstOrDefault((attr) => attr is Inject) as Inject;

				if (data != null)
				{
					string key = data.Key ?? field.FieldType.ToString();
					object? value = Get(key);
					field.SetValue(instance, value);
					continue;
				}
			}
		}


		private object _FindConstructorAndConstruct(Type type)
		{
			foreach (var ctor in type.GetConstructors())
			{
				if (_IsInjectConstructor(ctor))
				{
					return _Construct(ctor);
				}
			}

			foreach (var ctor in type.GetConstructors())
			{
				if (ctor.GetParameters().Length == 0)
				{
					return _Construct(ctor);
				}
			}

			throw new Exception($"The class {type} has no injectable constructor or constructor without parameters");
		}


		private bool _IsInjectConstructor(ConstructorInfo ctor)
		{
			foreach (var attribute in ctor.GetCustomAttributes())
			{
				if (attribute is InjectConstructor)
				{
					return true;
				}
			}

			return false;
		}


		private object _Construct(ConstructorInfo ctor)
		{
			if (ctor.GetParameters().Length == 0)
			{
				return ctor.Invoke(null);
			}

			var parameters = ctor.GetParameters();
			object?[] args = new object[parameters.Length];
			for (int i = 0; i < args.Length; i++)
			{
				args[i] = _ProvideValue(parameters[i]);
			}

			return ctor.Invoke(args);
		}


		private object? _ProvideValue(ParameterInfo param)
		{
			foreach (var attr in param.GetCustomAttributes())
			{
				if (attr is Inject injectData)
				{
					string key = injectData.Key ?? param.GetType().ToString();
					return Get(key);
				}
			}

			return Get(param.GetType().ToString());
		}
	}
}
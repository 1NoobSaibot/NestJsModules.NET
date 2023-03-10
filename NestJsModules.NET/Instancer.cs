using System.Reflection;

namespace NestJsModules
{
	internal class Instancer
	{
		private readonly IGetModule _module;
		public object Value { get; private set; }

		public Instancer(Type type, IGetModule module)
		{
			_module = module;
			Value = _CreateInstance(type);
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
					object? value = _module.Get(key);
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
					object? value = _module.Get(key);
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
					string key = injectData.Key ?? param.ParameterType.ToString();
					return _module.Get(key);
				}
			}

			return _module.Get(param.ParameterType.ToString());
		}
	}
}

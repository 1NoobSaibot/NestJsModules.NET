namespace NestJsModules
{
	internal class Binding
	{
		public readonly bool IsExported = false;
		private bool _requiresInstantiate = false;
		private object? _value;


		public Binding(object? value, bool isExported, bool needInstantiation)
		{
			_value = value;
			IsExported = isExported;
			_requiresInstantiate = needInstantiation;
		}


		public object? GetValue(IGetModule module)
		{
			if (_requiresInstantiate)
			{
				_CreateInstance(module);
			}

			return _value; 
		}


		public static Binding BindValue(object? value)
		{
			return new Binding(value, false, false);
		}

		public static Binding BindValueForExport(object? value)
		{
			return new Binding(value, true, false);
		}

		public static Binding Instantiate(Type type)
		{
			if (_CanMakeInstance(type))
			{
				return new Binding(type, false, true);
			}

			throw new ArgumentException("Cannot instantiate object from value which is not a System.Type instance");
		}


		public static Binding InstantiateForExport(Type type)
		{
			if (_CanMakeInstance(type))
			{
				return new Binding(type, true, true);
			}

			throw new ArgumentException("Cannot instantiate object from value which is not a System.Type instance");
		}


		private void _CreateInstance(IGetModule module)
		{
			if (_value is Type type)
			{
				Instancer maker = new Instancer(type, module);
				_value = maker.Value;
				_requiresInstantiate = false;
				return;
			}

			throw new Exception("NeedInstantiation flag is set, but value is not a type instance");
		}


		private static bool _CanMakeInstance(Type t)
		{
			return !(t.IsInterface || t.IsAbstract);
		}
	}
}

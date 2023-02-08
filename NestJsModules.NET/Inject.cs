namespace NestJsModules
{
	[AttributeUsage(
		AttributeTargets.Property
		| AttributeTargets.Parameter
		| AttributeTargets.Field
	)]
	public class Inject : Attribute
	{
		public string? Key { get; private set; }


		public Inject() { }


		public Inject(string key)
		{
			Key = key;
		}
	}
}

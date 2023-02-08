using NestJsModules;

namespace Tests
{
	[TestClass]
	public class InjectTest
	{
		[TestMethod]
		public void ShouldProvideAValueToPropWithInjectAttribute()
		{
			Module module = new Module();
			module.Add(PRIVATE_READONLY_KEY, PRIVATE_READONLY_VALUE);
			module.Add(PRIVATE_PROP_KEY, PRIVATE_PROP_VALUE);
			module.Add(CONSTRUCT_ARG_KEY,CONSTRUCT_ARG_VALUE);
			module.Add<ISpeaker, Speaker>();

			var speaker = module.Get<ISpeaker>();
			Assert.AreEqual(PRIVATE_READONLY_VALUE, speaker!.SayHi());
			Assert.AreEqual(PRIVATE_PROP_VALUE, speaker!.SaySomethingElse());
			Assert.AreEqual(CONSTRUCT_ARG_VALUE, speaker!.GotFromConstructor);
		}


		private interface ISpeaker
		{
			string? SayHi();
			string? SaySomethingElse();
			string GotFromConstructor { get; }
		}

		private class Speaker : ISpeaker
		{
			[Inject(PRIVATE_READONLY_KEY)]
			private readonly string? _privateField;

			[Inject(PRIVATE_PROP_KEY)]
			private string? _privateProp { get; set; }
			public string GotFromConstructor { get; }


			[InjectConstructor]
			public Speaker(
				[Inject(CONSTRUCT_ARG_KEY)]string arg
			)
			{
				GotFromConstructor = arg;
			}

			public string? SayHi()
			{
				return _privateField;
			}


			public string? SaySomethingElse()
			{
				return _privateProp;
			}
		}

		private const string PRIVATE_READONLY_KEY = "privateREadonly";
		private const string PRIVATE_READONLY_VALUE = "Whassup??";
		private const string PRIVATE_PROP_KEY = "getOnlyProp";
		private const string PRIVATE_PROP_VALUE = "getOnlyPropVALUE";
		private const string CONSTRUCT_ARG_KEY = "constructKey";
		private const string CONSTRUCT_ARG_VALUE = "constructValue";

	}
}
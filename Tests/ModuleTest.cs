using NestJsModules;

namespace Tests
{
	[TestClass]
	public class ModuleTest
	{
		[TestMethod]
		public void ShouldAddAndReturnSomeValueByString()
		{
			const string key = "key";
			const string value = "value";

			Module module = new Module();
			module.Add(key, value);

			Assert.AreEqual(value, module.Get<string>(key));
		}


		[TestMethod]
		public void ShouldAddAndReturnValueByItsType()
		{
			const string value = "value";

			Module module = new Module();
			module.Add(value);

			Assert.AreEqual(value, module.Get<string>());
		}


		[TestMethod]
		public void ShouldAddAndReturnValueByItsInterface()
		{
			Module module = new Module();
			module.Add<ISpeaker>(new Speaker());

			Assert.AreEqual(HI_STRING, module!.Get<ISpeaker>()!.SayHi());
			Assert.IsInstanceOfType(module.Get<ISpeaker>(), typeof(ISpeaker));
		}


		[TestMethod]
		public void ShouldThrowAnErrorIfDependencyWasNotProvided()
		{
			Module module = new Module();
			Assert.ThrowsException<KeyNotFoundException>(module.Get<string>);
		}


		[TestMethod]
		public void ShouldGetDependencyFromSubmoduleExports()
		{
			var submodule = new Module();
			submodule.AddForExport<ISpeaker>(new Speaker());

			var rootmodule = new Module();
			rootmodule.Import(submodule);

			var speaker = rootmodule.Get<ISpeaker>();
			Assert.IsNotNull(speaker);
			Assert.AreEqual(HI_STRING, speaker.SayHi());
		}


		[TestMethod]
		public void ShouldNotGetDependencyFromSubmoduleWhenItIsNotForExport()
		{
			var submodule = new Module();
			submodule.Add<ISpeaker>(new Speaker());

			var rootmodule = new Module();
			rootmodule.Import(submodule);

			Assert.ThrowsException<KeyNotFoundException>(rootmodule.Get<ISpeaker>);
		}


		private interface ISpeaker
		{
			string SayHi();
		}

		private class Speaker : ISpeaker
		{
			public string SayHi()
			{
				return HI_STRING;
			}
		}

		private const string HI_STRING = "Whassup??";
	}
}

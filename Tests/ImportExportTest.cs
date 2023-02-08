using NestJsModules;

namespace Tests
{
	[TestClass]
	public class ImportExportTest
	{
		[TestMethod]
		public void ShouldGetDependencyFromSubmoduleExports()
		{
			var submodule = new Module();
			submodule.BindValueForExport<ISpeaker>(new Speaker());

			var rootmodule = new Module();
			rootmodule.Import(submodule);

			var speaker = rootmodule.Get<ISpeaker>();
			Assert.IsNotNull(speaker);
			Assert.AreEqual(HI_STRING, speaker.SayHi());
		}


		[TestMethod]
		public void ShouldNotGetDependencyFromChildrenInternalScopes()
		{
			var submodule = new Module();
			submodule.BindValue<ISpeaker>(new Speaker());

			var rootmodule = new Module();
			rootmodule.Import(submodule);

			Assert.ThrowsException<KeyNotFoundException>(rootmodule.Get<ISpeaker>);
		}


		[TestMethod]
		public void ShouldNotGetExportsOfStepChildren()
		{
			var stepson = new Module();
			stepson.InstantiateValueForExport<Speaker>();

			var son = new Module();
			son.Import(stepson);

			var root = new Module();
			root.Import(son);

			// Check speaker is provided in exports
			var speaker = son.Get<Speaker>();
			Assert.AreEqual(HI_STRING, speaker!.SayHi());

			// But you cannot get it if exporter is not your direct child.
			Assert.ThrowsException<KeyNotFoundException>(root.Get<Speaker>);
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

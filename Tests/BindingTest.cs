using NestJsModules;

namespace Tests
{
	[TestClass]
	public class BindingTest
	{
		[TestMethod]
		public void BindingByString()
		{
			const string key = "key";
			const string value = "value";

			Module module = new Module();
			Assert.ThrowsException<KeyNotFoundException>(() => module.Get(key));

			module.BindValue(key, value);
			Assert.AreEqual(value, module.Get<string>(key));
		}


		[TestMethod]
		public void BindingByType()
		{
			const string value = "value";

			Module module = new Module();
			Assert.ThrowsException<KeyNotFoundException>(module.Get<string>);

			module.BindValue(value);
			Assert.AreEqual(value, module.Get<string>());
		}


		[TestMethod]
		public void BindingByInterface()
		{
			Module module = new Module();
			Assert.ThrowsException<KeyNotFoundException>(module.Get<ISpeaker>);

			module.BindValue<ISpeaker>(new Speaker());
			Assert.IsInstanceOfType(module.Get<ISpeaker>(), typeof(ISpeaker));
			Assert.AreEqual(HI_STRING, module!.Get<ISpeaker>()!.SayHi());
		}


		[TestMethod]
		public void BindingByClassWithInstantiation()
		{
			Module module = new Module();
			Assert.ThrowsException<KeyNotFoundException>(module.Get<Speaker>);

			module.InstantiateValue<Speaker>();
			Assert.AreEqual(HI_STRING, module!.Get<Speaker>()!.SayHi());
			Assert.IsInstanceOfType(module.Get<Speaker>(), typeof(Speaker));
		}


		[TestMethod]
		public void BindingByInterfaceWithInstantiation()
		{
			Module module = new Module();
			Assert.ThrowsException<KeyNotFoundException>(module.Get<ISpeaker>);

			module.InstantiateValue<ISpeaker, Speaker>();
			Assert.AreEqual(HI_STRING, module!.Get<ISpeaker>()!.SayHi());
			Assert.IsInstanceOfType(module.Get<ISpeaker>(), typeof(ISpeaker));
		}


		[TestMethod]
		public void BindingForExportWorksAsBinding()
		{
			Module module = new Module();
			Assert.ThrowsException<KeyNotFoundException>(module.Get<ISpeaker>);

			module.InstantiateValueForExport<ISpeaker, Speaker>();
			Assert.AreEqual(HI_STRING, module!.Get<ISpeaker>()!.SayHi());
			Assert.IsInstanceOfType(module.Get<ISpeaker>(), typeof(ISpeaker));
		}


		[TestMethod]
		public void InstantiatinghapensOnlyOnce()
		{
			Module module1 = new Module();
			module1.InstantiateValue<Speaker>();
			Speaker? speaker11 = module1!.Get<Speaker>();
			Speaker? speaker12 = module1!.Get<Speaker>();
			Assert.AreEqual<Speaker>(speaker11!, speaker12!);

			Module module2 = new Module();
			module2.InstantiateValueForExport<Speaker>();
			Speaker? speaker21 = module2!.Get<Speaker>();
			Speaker? speaker22 = module2!.Get<Speaker>();
			Assert.AreEqual<Speaker>(speaker21!, speaker22!);

			Assert.AreNotEqual(speaker11, speaker21);
		}


		[TestMethod]
		public void DoesntAllowYouToInstantiateOfAbstractTypes()
		{
			Module module = new Module();
			Assert.ThrowsException<ArgumentException>(() => module.InstantiateValue<ISpeaker>());
			Assert.ThrowsException<ArgumentException>(() => module.InstantiateValue<AbstractSpeaker>());
			Assert.ThrowsException<ArgumentException>(() => module.InstantiateValue<ISpeaker, ISpeaker>());
			Assert.ThrowsException<ArgumentException>(() => module.InstantiateValue<ISpeaker, AbstractSpeaker>());
			Assert.ThrowsException<ArgumentException>(() => module.InstantiateValue<AbstractSpeaker, AbstractSpeaker>());
		}


		[TestMethod]
		public void AllowYouToInstantiateValueTypes()
		{
			Module module = new Module();
			module.InstantiateValue<ISpeaker, StructSpeaker>();
			ISpeaker? speaker = module.Get<ISpeaker>();
			Assert.IsInstanceOfType(speaker, typeof(StructSpeaker));
			Assert.AreEqual(HI_STRING, speaker!.SayHi());
		}


		private interface ISpeaker
		{
			string SayHi();
		}


		private abstract class AbstractSpeaker : ISpeaker {
			public abstract string SayHi();
		}


		private class Speaker : ISpeaker
		{
			public string SayHi()
			{
				return HI_STRING;
			}
		}


		private struct StructSpeaker : ISpeaker
		{
			public StructSpeaker() { }

			public string SayHi()
			{
				return HI_STRING;
			}
		}


		private const string HI_STRING = "Whassup??";
	}
}

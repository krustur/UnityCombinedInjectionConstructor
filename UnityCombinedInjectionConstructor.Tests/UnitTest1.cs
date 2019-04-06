using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;
using Unity.Injection;

namespace UnityCombinedInjectionConstructor.Tests
{
    public interface ISomeClass
    {
        ISomeDependency SomeDependency { get; }
        string SomeStringValue { get; }
    }

    public class SomeClass : ISomeClass
    {
        public ISomeDependency SomeDependency { get; }
        public string SomeStringValue { get; }

        public SomeClass(ISomeDependency someDependency, string someStringValue)
        {
            SomeDependency = someDependency;
            SomeStringValue = someStringValue;
        }
    }

    public interface ISomeDependency { }
    public class SomeDependency : ISomeDependency { }

    [TestClass]
    public class UnitTest1
    {
        private ISomeClass _resolved;

        [TestInitialize]
        public void UnitTest1_Initialize()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterType<ISomeClass, SomeClass>(
                //new Unity.Injection.InjectionConstructor(
                //new ResolvedParameter<ISomeDependency>(),
                //new InjectionParameter(typeof(string), "hej")
                //)
                new CombinedInjectionConstructor(new InjectionParameter<string>("hej"))
                );
            container.RegisterType<ISomeDependency, SomeDependency>();

            _resolved = container.Resolve<ISomeClass>();
        }

        [TestMethod]
        public void UnitTest1_ResolvedOk()
        {
            Assert.IsNotNull(_resolved);
        }

        [TestMethod]
        public void UnitTest1_ResolvedDependencyOk()
        {
            Assert.IsNotNull(_resolved.SomeDependency);
        }

        [TestMethod]
        public void UnitTest1_ResolvedStringOk()
        {
            Assert.AreEqual("hej", _resolved.SomeStringValue);
        }
    }
}

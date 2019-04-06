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

        public SomeClass(ISomeDependency someDependency, string someStringValue, ISomeOtherDependency someOtherDependency)
        {
            SomeDependency = someDependency;
            SomeStringValue = someStringValue;
        }
    }

    public interface ISomeDependency { }
    public class SomeDependency : ISomeDependency { }

    public interface ISomeOtherDependency { }
    public class SomeOtherDependency : ISomeOtherDependency { }

    [TestClass]
    public class CombinedInjectionConstructor_Resolve
    {
        private ISomeClass _resolved;

        [TestInitialize]
        public void CombinedInjectionConstructor_Resolve_Initialize()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterType<ISomeClass, SomeClass>(
                //new Unity.Injection.InjectionConstructor(
                //new ResolvedParameter<ISomeDependency>(),
                //new InjectionParameter(typeof(string), "hej")
                //)
                new CombinedInjectionConstructor(
                    new InjectionParameter<string>("hej"),
                    new ResolvedParameter<ISomeOtherDependency>()
                    )
                );
            container.RegisterType<ISomeDependency, SomeDependency>();
            container.RegisterType<ISomeOtherDependency, SomeOtherDependency>();

            _resolved = container.Resolve<ISomeClass>();
        }

        [TestMethod]
        public void CombinedInjectionConstructor_Resolve_ResolvedOk()
        {
            Assert.IsNotNull(_resolved);
        }

        [TestMethod]
        public void CombinedInjectionConstructor_Resolve_ResolvedDependencyOk()
        {
            Assert.IsNotNull(_resolved.SomeDependency);
        }

        [TestMethod]
        public void CombinedInjectionConstructor_Resolve_ResolvedStringOk()
        {
            Assert.AreEqual("hej", _resolved.SomeStringValue);
        }
    }
}

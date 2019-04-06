using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Unity;

namespace UnityCombinedInjectionConstructor.Tests
{
    public interface ISomeClassMultipleCtors
    {
    }

    public class SomeClassMultipleCtors : ISomeClassMultipleCtors
    {
        public SomeClassMultipleCtors()
        {
        }

        public SomeClassMultipleCtors(ISomeDependency someDependency)
        {
        }
    }

    [TestClass]
    public class CombinedInjectionConstructor_MultipleConstructors_Resolve
    {

        [TestInitialize]
        public void CombinedInjectionConstructor_Resolve_Initialize()
        {
        }

        [TestMethod]
        public void CombinedInjectionConstructor_MultipleConstructors_Resolve_Exception()
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var container = new UnityContainer();
                container.RegisterType<ISomeClassMultipleCtors, SomeClassMultipleCtors>(
                    new CombinedInjectionConstructor()
                );
            });
        }
    }
}

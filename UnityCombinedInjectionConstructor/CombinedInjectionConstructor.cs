using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Injection;

namespace UnityCombinedInjectionConstructor.Tests
{
    public class CombinedInjectionConstructor : InjectionConstructor
    {
        private readonly object[] _arguments;
        private object[] _data;

        public override object[] Data
        {
            get { return _data; }
        }

        public CombinedInjectionConstructor(params object[] arguments) : base(arguments)
        {
            _arguments = arguments;
        }

        public override void AddPolicies<TContext, TPolicySet>(Type registeredType, Type mappedToType, string name, ref TPolicySet policies)
        {
            var constructor = FindDefaultConstructor(mappedToType);
            var constructorParameterInfos = constructor.GetParameters();
            var defaultResolvedParameters = CreateDefaultResolvedParameters(constructorParameterInfos);
            var resolvedParameters = ReplaceDefaultResolvesWithInjectionResolves(defaultResolvedParameters);
            _data = resolvedParameters.ToArray();

            var ctor = FindConstructor(mappedToType);

            base.AddPolicies<TContext, TPolicySet>(registeredType, mappedToType, name, ref policies);
        }

        private List<ParameterBase> ReplaceDefaultResolvesWithInjectionResolves(List<ResolvedParameter> defaultResolvedParameters)
        {
            var resolvedParameters = new List<ParameterBase>();
            var paramValuesPosition = 0;

            foreach (var defaultResolvedParameter in defaultResolvedParameters)
            {
                var useDefault = true;
                if (paramValuesPosition < _arguments.Length)
                {
                    var parameterValue = _arguments.ElementAt(paramValuesPosition);

                    switch (parameterValue)
                    {
                        case InjectionParameter _:
                            if (defaultResolvedParameter.ParameterType == ((InjectionParameter)parameterValue).ParameterType)
                            {
                                resolvedParameters.Add((InjectionParameter)parameterValue);
                                paramValuesPosition++;
                                useDefault = false;
                            }

                            break;
                        case ResolvedParameter _:
                            if (defaultResolvedParameter.ParameterType == ((ResolvedParameter)parameterValue).ParameterType)
                            {
                                resolvedParameters.Add((ResolvedParameter)parameterValue);
                                paramValuesPosition++;
                                useDefault = false;
                            }

                            break;
                        default:
                            throw new NotImplementedException(string.Format("Injection paramter of type {0} is not implemented.", parameterValue.GetType().Name));
                    }
                }

                if (useDefault)
                {
                    resolvedParameters.Add(defaultResolvedParameter);
                }
            }

            return resolvedParameters;
        }

        private static List<ResolvedParameter> CreateDefaultResolvedParameters(ParameterInfo[] constructorParameterInfos)
        {
            return constructorParameterInfos
                .Select(parameterInfo => new ResolvedParameter(parameterInfo.ParameterType))
                .ToList();
        }

        private ConstructorInfo FindConstructor(Type typeToCreate)
        {
            var instanceConstructors = typeToCreate.GetTypeInfo()
                .DeclaredConstructors
                .Where(c => c.IsStatic == false && c.IsPublic); ;
            ConstructorInfo constructorInfo = null;
            foreach (var ctor in instanceConstructors)
            {
                if (_data.MatchMemberInfo(ctor))
                {
                    constructorInfo = ctor;
                }
            }
            return constructorInfo;
        }

        private ConstructorInfo FindDefaultConstructor(Type mappedToType)
        {
            var constructorInfos = mappedToType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            var constructors = constructorInfos
                //.Select(x => new
                //{
                //    Constructor = x,
                //    Length = x.GetParameters().Length
                //})
                //.OrderByDescending(x => x.Length)
                .ToList();

            if (constructors.Count != 1)
            {
                throw new Exception($"Could not find unique constructor on type {mappedToType.Name}. Single constructor required for {nameof(UnityCombinedInjectionConstructor)}");
            }

            return constructors.First();
        }
    }

}
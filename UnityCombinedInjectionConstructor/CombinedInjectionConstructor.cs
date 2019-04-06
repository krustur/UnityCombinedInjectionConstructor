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
            /*
            Unity.Injection
            {
        public static class InjectionMatching
        {
            #region Signature matching

            public static bool MatchMemberInfo(this object[] data, MethodBase info)

            */
            //var matcher = new ParameterMatcher(_injectionParameterValues);
            //var typeToCreateReflector = new Unity.Configuration.ReflectionHelper(typeToCreate);
            var instanceConstructors = typeToCreate.GetTypeInfo()
                .DeclaredConstructors
                .Where(c => c.IsStatic == false && c.IsPublic);;
            ConstructorInfo constructorInfo = null;

            foreach (var ctor in instanceConstructors)
            {
                if (Matches(_data, ctor.GetParameters()))
                //if (Matches(_arguments, ctor.GetParameters()))
                {
                    constructorInfo = ctor;
                }
            }

            return constructorInfo;
        }

        //public virtual bool Matches(List<ParameterBase> parameterValues, IEnumerable<Type> candidate)
        public virtual bool Matches(object[] parameterValues, IEnumerable<Type> candidate)
        {
            List<Type> typeList = new List<Type>(candidate);
            if (parameterValues.Length != typeList.Count)
                return false;
            for (int index = 0; index < parameterValues.Length; ++index)
            {
                //if (!parameterValues[index].MatchesType(typeList[index]))
                if (!((ParameterBase)(parameterValues[index])).ParameterType.MatchesType(typeList[index]))
                    return false;
            }
            return true;
        }

        //public virtual bool Matches(List<ParameterBase> parameterValues, IEnumerable<ParameterInfo> candidate)
        public virtual bool Matches(object[] parameterValues, IEnumerable<ParameterInfo> candidate)
        {
            return Matches(parameterValues, candidate.Select<ParameterInfo, Type>((Func<ParameterInfo, Type>)(pi => pi.ParameterType)));
        }

        private ConstructorInfo FindDefaultConstructor(Type mappedToType)
        {
            var constructorInfos = mappedToType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            var constructors = constructorInfos
                .Select(x => new
                {
                    Constructor = x,
                    Length = x.GetParameters().Length
                })
                .OrderByDescending(x => x.Length)
                .ToList();

            if (constructors.Count > 1 && constructors[0].Length == constructors[1].Length)
            {
                throw new InvalidOperationException("Multiple constructor with same number of paramters. Unable to determine which constructor to use");
            }

            return constructors.First().Constructor;
        }
    }

}
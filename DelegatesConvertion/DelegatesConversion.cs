// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DelegatesConvertion.cs" company="Natan Podbielski">
//   Copyright (c) 2016 - 2017 Natan Podbielski. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DelegatesConversion
{
    public static class DelegatesConversion
    {
        public static Func<TSource, Delegate> CreateConverter<TSource>(Type destinationType)
        {
            var sourceType = typeof(TSource);
            return CreateConverterInternal<Func<TSource, Delegate>>(destinationType, sourceType);
        }

        public static Func<Delegate, Delegate> CreateConverter(Type sourceType, Type destinationType)
        {
            return CreateConverterInternal<Func<Delegate, Delegate>>(destinationType, sourceType);
        }

        private static TConverterDelegate CreateConverterInternal<TConverterDelegate>
            (Type destinationType, Type sourceType) where TConverterDelegate : class
        {
            var sourceParams = GetInvokeMethodParams(sourceType);
            var destParams = GetInvokeMethodParams(destinationType);
            if (ValidateSignatures(sourceParams, destParams))
            {
                return null;
            }
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("dynamicAssembly"),
                AssemblyBuilderAccess.RunAndCollect);
            var module = assemblyBuilder.DefineDynamicModule("module");
            var typeBuilder = module.DefineType("converter_type");
            var methodBuilder = typeBuilder.DefineMethod("converter",
                MethodAttributes.Static | MethodAttributes.Public |
                MethodAttributes.Final, CallingConventions.Standard,
                destinationType, new[] {sourceType});
            var generator = methodBuilder.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldftn, sourceType.GetMethod("Invoke"));
            generator.Emit(OpCodes.Newobj, destinationType.GetConstructors()[0]);
            generator.Emit(OpCodes.Ret);
            var type = typeBuilder.CreateType();
            var converter = type.GetMethod("converter").CreateDelegate(typeof(TConverterDelegate)) as TConverterDelegate;
            return converter;
        }

        private static Type[] GetInvokeMethodParams(Type delegateType)
        {
            return delegateType.GetMethod("Invoke").GetParameters().Select(p => p.ParameterType).ToArray();
        }

        private static bool ValidateSignatures(Type[] sourceParams, Type[] destParams)
        {
            if (sourceParams.Length == destParams.Length)
            {
                for (var i = 0; i < sourceParams.Length; i++)
                {
                    if (sourceParams[i] != destParams[i])
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}
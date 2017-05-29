using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DelegatesConvertion
{
    internal static class DelegatesConvertion
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
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("dynamicAssembly"),
                AssemblyBuilderAccess.RunAndCollect);
            var module = assemblyBuilder.DefineDynamicModule("module");
            var typeBuilder = module.DefineType("converter_type");
            var methodBuilder = typeBuilder.DefineMethod("converter", MethodAttributes.Static | MethodAttributes.Public |
                                                                      MethodAttributes.Final, CallingConventions.Standard,
                destinationType, new[] { sourceType });
            var generator = methodBuilder.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldftn, sourceType.GetMethod("Invoke"));
            generator.Emit(OpCodes.Newobj, destinationType.GetConstructors()[0]);
            generator.Emit(OpCodes.Ret);
            var type = typeBuilder.CreateType();
            var converter = type.GetMethod("converter").CreateDelegate(typeof(TConverterDelegate)) as TConverterDelegate;
            return converter;
        }
    }
}
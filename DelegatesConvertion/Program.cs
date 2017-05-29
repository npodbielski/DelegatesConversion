// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Natan Podbielski">
//   Copyright (c) 2016 - 2017 Natan Podbielski. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DelegatesConvertion
{
    public delegate void CustomEvent(object sender, EventArgs args);

    internal class Program
    {
        private static event CustomEvent Event;

        protected static void OnEvent()
        {
            Event?.Invoke(null, EventArgs.Empty);
        }

        private static Func<TSource, TDest> CreateConverter<TSource, TDest>()
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("dynamicAssembly"),
                AssemblyBuilderAccess.RunAndCollect);
            var module = assemblyBuilder.DefineDynamicModule("module");
            var typeBuilder = module.DefineType("converter_type");
            var methodBuilder = typeBuilder.DefineMethod("converter", MethodAttributes.Static | MethodAttributes.Public |
                                                                        MethodAttributes.Final, CallingConventions.Standard,
                typeof(TDest), new[] { typeof(TSource) });
            var generator = methodBuilder.GetILGenerator();
            var t = typeof(CustomEvent);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldftn, typeof(TSource).GetMethod("Invoke"));
            generator.Emit(OpCodes.Newobj, t.GetConstructors()[0]);
            generator.Emit(OpCodes.Ret);
            var type = typeBuilder.CreateType();
            var converter = (Func<TSource, TDest>)type.GetMethod("converter").CreateDelegate(typeof(Func<TSource, TDest>));
            return converter;
        }

        private static void Main(string[] args)
        {
            Event += (sender, eventArgs) => { };

            var converter = CreateConverter<Action<object, EventArgs>, CustomEvent>();

            Action<object, EventArgs> onEvent = (sender, eventArgs) =>
            {
                var a = 1;
            };
            var d = converter(onEvent);
            var add = typeof(Program).GetEvent("Event", BindingFlags.Static | BindingFlags.NonPublic).GetAddMethod(true);
            add.Invoke(null, new object[] { d });
            OnEvent();
        }
    }
}
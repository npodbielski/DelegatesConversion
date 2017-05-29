// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Natan Podbielski">
//   Copyright (c) 2016 - 2017 Natan Podbielski. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Reflection;

namespace DelegatesConvertion
{
    public delegate void CustomEvent(object sender, EventArgs args);

    internal class Program
    {
        private static event CustomEvent Event;

        protected static void InvokeEvent()
        {
            Event?.Invoke(null, EventArgs.Empty);
        }

        private static void Main(string[] args)
        {
            Event += (sender, eventArgs) => { };

            var converter = DelegatesConvertion.CreateConverter<Action<object, EventArgs>>(typeof(CustomEvent));
            var typelessConverter = DelegatesConvertion.CreateConverter<Action<object, EventArgs>>(typeof(CustomEvent));

            Action<object, EventArgs> onEvent1 = (sender, eventArgs) =>
            {
                Debug.WriteLine("created by converter with concrete types");
            };
            Action<object, EventArgs> onEvent2 = (sender, eventArgs) =>
            {
                Debug.WriteLine("created by converter with Delegate types");
            };
            var d1 = converter(onEvent1);
            var d2 = typelessConverter(onEvent2);
            var add = typeof(Program).GetEvent("Event", BindingFlags.Static | BindingFlags.NonPublic).GetAddMethod(true);
            add.Invoke(null, new object[] { d2 });
            add.Invoke(null, new object[] { d1 });
            InvokeEvent();
        }
    }
}
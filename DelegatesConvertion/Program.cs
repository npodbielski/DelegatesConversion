// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Natan Podbielski">
//   Copyright (c) 2016 - 2017 Natan Podbielski. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace DelegatesConvertion
{
    internal delegate void CustomEvent(object sender, EventArgs args);

    public class Events
    {
        public event EventHandler<EventArgs> Event1;
        public event EventHandler<UnhandledExceptionEventArgs> Event2;
    }

    internal class Program
    {
        public static event CustomEvent Event;

        protected static void OnEvent()
        {
            Event?.Invoke(null, EventArgs.Empty);
        }

        private static EventHandler<T> ConvertToEventHandler<S, T>(Action<S, T> d)
        {
            return (s, e) => { d((S)s, e); };
        }

        private static void Handler(object sender, EventArgs e)
        {
        }

        public static Events events = new Events();

        private static void Main(string[] args)
        {
            Action<object, EventArgs> onEvent = (sender, eventArgs) => { };
            Action<object, EventArgs> handler = (sender, eventArgs) => { };
            Action<object, object> objectHandler = (sender, eventArgs) => { };
            Action<Events, EventArgs> noObjectHandler = (sender, eventArgs) => { };
            Event += onEvent.Invoke;
            Event += Handler;
            events.Event1 += handler.Invoke;
            events.Event2 += handler.Invoke;
            events.Event1 += objectHandler.Invoke;
            events.Event2 += objectHandler.Invoke;
            events.Event1 += ConvertToEventHandler<Events, EventArgs>(noObjectHandler);
            events.Event2 += ConvertToEventHandler<Events, UnhandledExceptionEventArgs>(noObjectHandler);
            var t = typeof(CustomEvent);
            //Activator.CreateInstance(t, handler.Invoke);
        }
    }
}
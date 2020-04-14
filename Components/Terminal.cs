using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    public static class Terminal
    {
        public static void WriteLine(State.EventState state, string message)
        {
            MessageShow(state, message);
            State.SendMessage(state, message);
        }
        public static void WriteLine(State.EventState state, string format, params object[] opt)
        {
            var message = string.Format(format, opt);
            WriteLine(state, message);
        }

        private static void MessageShow(State.EventState state, string message)
        {
            if (state < State.EventState.Log)
            {
                Console.WriteLine(message);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("\t>>>>> " + message);
            }
        }
    }
}

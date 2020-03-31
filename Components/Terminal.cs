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
            Console.WriteLine(message);
            State.SendMessage(state, message);
        }
        public static void WriteLine(State.EventState state, string format, params object[] opt)
        {
            var text = string.Format(format, opt);
            Console.WriteLine(text);
            State.SendMessage(state, text);
        }
    }
}

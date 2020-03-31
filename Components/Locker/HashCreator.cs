using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components.Locker
{
    class HashCreator
    {
        public static string GetString(int length)
        {
            string hash = string.Empty;
            for (int i = 0; i < length; i++)
            {
                var x = State.RandomSource.Next(15).ToString("x");
                if (State.RandomSource.Next(1000) > 500) { x = x.ToUpper(); }
                hash += x;
            }
            return hash;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Utility
{
    internal static class Randomizer
    {
        private static object ___lockobj = new object();
        private static Random random { get; set; } = new Random();
        private static double BMR(double ave = 0, double sigma = 1)
        {
            double x, y;
            lock (___lockobj)
            {
                x = random.NextDouble();
                y = random.NextDouble();
            }
            return sigma * Math.Sqrt(-2.0 * Math.Log(x)) * Math.Cos(2.0 * Math.PI * y) + ave;
        }
        private static double GetBoth
        {
            get
            {
                lock (___lockobj)
                { return Get * 2 - 1; }
            }
        }
        private static double Get
        {
            get
            {
                lock (___lockobj)
                { return random.NextDouble(); }
            }
        }

        public enum Sign
        {
            Const,
            Both,
            Plus,
            Minus,
        }
        public static void Noize(ref Components.Real[] data, Sign sign = Sign.Both, double center = 0, double amplify = 1.0)
        {
            switch (sign)
            {
                case Sign.Both:
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = BMR(center, amplify);
                    }
                    break;
                case Sign.Plus:
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = Get * amplify;
                    }
                    break;
                case Sign.Minus:
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = -Get * amplify;
                    }
                    break;
                case Sign.Const:
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = amplify;
                    }
                    break;
                default:
                    break;
            }
        }

        public static void Fluctuation(ref Components.Real[] data, double amplify = 1.0)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] += BMR(0, amplify);
                data[i] = Math.Min(1.0 / data.Length, Math.Max(-1.0 / data.Length, data[i]));
            }
        }
    }
}

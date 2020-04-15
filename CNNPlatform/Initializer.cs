using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CNNPlatform
{
    public static class Initializer
    {
        private static void State_ExceptionCatched(Exception ex)
        {
            throw ex;
        }

        public static bool Terminate { get; set; } = false;

        public static int Generation { get; set; } = 0;

        public static void Startup()
        {
            Components.State.ExceptionCatched += State_ExceptionCatched;

            Components.State.SetAssembly(Assembly.GetAssembly(typeof(Initializer)));
            Components.State.AddSharedSourceGroup("CNNPlatform.Function.SharedMethod");
            Components.State.AddSourceGroup("CNNPlatform.Function.Process");

            var gpuInitialized = Components.State.Initialize();

        }
    }
}

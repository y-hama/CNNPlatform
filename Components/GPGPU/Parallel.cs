using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components.GPGPU
{
    public static class Parallel
    {
        public static void For(int start, int end, Action<int> func)
        {
#if PARALLEL
            System.Threading.Tasks.Parallel.For(start, end, func);
#else
            for (int i = start; i < end; i++) { func(i); }
#endif
        }
    }
}

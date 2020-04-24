using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Tasks
{
    public static void ForStep(int start, int end, Action<int> func)
    {
        for (int i = start; i < end; i++) { func(i); }
    }

    public static void ForParallel(int start, int end, Action<int> func)
    {
#if PARALLEL
        Parallel.For(start, end, i => { func(i); });
#else
        for (int i = start; i < end; i++) { func(i); }
#endif
    }
}
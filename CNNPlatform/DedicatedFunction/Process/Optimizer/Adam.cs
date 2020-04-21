using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace CNNPlatform.DedicatedFunction.Process.Optimizer
{
    class Adam : OptimizerBase
    {
        private Components.Real[] m { get; set; }
        private Components.Real[] v { get; set; }

        private const double a = 0.002;
        private const double b1 = 0.9;
        private const double b2 = 0.999;
        private const double eps = 1e-8;

        public override double Update(ref Real[] _w, Real[] diff, bool doUpdate, double rho = 0)
        {
            var w = _w;
            if (!Initialized)
            {
                Initialized = true;
                m = new Real[w.Length];
                v = new Real[w.Length];
            }
            double _rho = (a + rho) / 2;
            double delta = 0;
            Components.GPGPU.Parallel.For(0, w.Length, i =>
            {
                m[i] = b1 * m[i] + (1 - b1) * diff[i];
                v[i] = b2 * v[i] + (1 - b2) * diff[i] * diff[i];
                var mhat = m[i] / (1 - Math.Pow(b1, Iteration));
                var vhat = v[i] / (1 - Math.Pow(b2, Iteration));
                var dw = (mhat / (Math.Sqrt(vhat + eps)));
                if (doUpdate) { w[i] = w[i] - _rho * dw; }
                delta += dw;
            });
            return delta;
        }
    }
}

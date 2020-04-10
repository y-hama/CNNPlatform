using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace CNNPlatform.DedicatedFunction.Process.Optimizer
{
    class AMSGrad : OptimizerBase
    {
        private Components.Real[] m { get; set; }
        private Components.Real[] v { get; set; }
        private Components.Real[] n { get; set; }

        private const double eta = 0.1;
        private const double b1 = 0.9;
        private const double b2 = 0.999;
        private const double eps = 1e-8;

        private const double a = 100;
        private const double b = 0.01;

        private double Clip(double e, double t)
        {
            double nh, nl, rat;
            nh = (1 + (10 / ((1 - b) * (t + 10)))) * eta;
            //nl = (1 - (990 / ((1 - b) * (t + 1000) + 1))) * eta;
            nl = Math.Pow((1 - Math.Cos((Math.PI / 4) * (1 - 1.0 / ((t / 2) + 1)))), 8) * eta;
            rat = (1.0 / Math.Sqrt(t));

            double cp = Math.Max(nl, Math.Min(nh, e));
            return rat * cp;
        }

        public override double Update(ref Real[] _w, Real[] diff, double rho)
        {
            var w = _w;
            if (!Initialized)
            {
                m = new Real[w.Length];
                v = new Real[w.Length];
                n = new Real[w.Length];
                Initialized = true;
            }
            double delta = 0;
            Components.GPGPU.Parallel.For(0, w.Length, i =>
            {
                double g = diff[i];
                m[i] = b1 * m[i] + (1 - b1) * g;
                v[i] = b2 * v[i] + (1 - b2) * g * g;
                double mhat = m[i] / (1 - Math.Pow(b1, Iteration));
                double vhat = v[i] / (1 - Math.Pow(b2, Iteration));
                n[i] = Clip(1.0 / Math.Sqrt(v[i]), Iteration);
                double dw = n[i] * (mhat / (Math.Sqrt(vhat + eps)));
                w[i] = w[i] - dw;
                delta += dw;
            });
            return delta;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace CNNPlatform.Function.Process.Optimizer
{
    class Adam : OptimizerBase
    {
        private bool Initialize { get; set; } = false;

        private Components.Real[] m { get; set; }
        private Components.Real[] v { get; set; }
        private Components.Real t { get; set; }

        private const double a = 0.002;
        private const double b1 = 0.9;
        private const double b2 = 0.999;
        private const double eps = 1e-8;

        public override double Update(ref Real[] w, Real[] diff, double rho = 0)
        {
            if (!Initialize)
            {
                Initialize = true;
                m = new Real[w.Length];
                v = new Real[w.Length];
                t = 0;
            }
            double _rho = (a + rho) / 2;
            double delta = 0;
            t++;
            for (int i = 0; i < w.Length; i++)
            {
                m[i] = b1 * m[i] + (1 - b1) * diff[i];
                v[i] = b2 * v[i] + (1 - b2) * diff[i] * diff[i];
                var mhat = m[i] / (1 - Math.Pow(b1, t));
                var vhat = v[i] / (1 - Math.Pow(b2, t));
                var dw = _rho * (mhat / (Math.Sqrt(vhat + eps)));
                w[i] = w[i] - dw;
                delta += dw;
            }
            return delta;
        }
    }
}

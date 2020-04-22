using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace CNNPlatform.DedicatedFunction.Process.Optimizer
{
    class AdaSelf : OptimizerBase
    {

        #region AdamBase
        private Real[] m { get; set; }
        private Real[] v { get; set; }

        private const double b1 = 0.9;
        private const double b2 = 0.999;
        private const double eps = 1e-8;
        private const double eps_h = 1e+8;

        private Real dw_Adam(int i, Real diff)
        {
            double g = diff;
            m[i] = b1 * m[i] + (1 - b1) * g;
            v[i] = b2 * v[i] + (1 - b2) * g * g;
            var mhat = m[i] / (1 - Math.Pow(b1, Iteration));
            var vhat = v[i] / (1 - Math.Pow(b2, Iteration));
            double dw = mhat / (Math.Sqrt(vhat + eps));
            return dw;
        }
        #endregion

        #region SGDBase
        private double s1 = 1e-4;
        private Real dw_SGD(int i, Real diff, Real max)
        {
            return s1 * diff;
        }
        #endregion

        #region Parameter
        private Real[] pd { get; set; }
        private Real[] vd { get; set; }
        private Real[] ad { get; set; }
        #endregion

        private const double center = 0.5;
        private const double eta1 = 0.005;
        private const double eta2 = 0.001;
        private const double bd = 0.99;

        public override double Update(ref Real[] _w, Real[] diff, bool doUpdate, double rho = 0)
        {
            var w = _w;
            if (!Initialized)
            {
                #region Parameter Initialize
                m = new Real[w.Length];
                v = new Real[w.Length];
                pd = new Real[w.Length];
                vd = new Real[w.Length];
                ad = new Real[w.Length];
                #endregion

                Initialized = true;
            }
            double delta = 0;
            double max = diff.Max(x => Math.Abs(x));
            double sign = max > 1 ? -1 : 1;
            s1 = Math.Pow(10, sign * (Math.Ceiling(Math.Log10(max <= eps ? eps : max))));
            double bdt = Math.Pow(bd, Iteration);
            double eta = bdt * eta1 + (1 - bdt) * eta2;
            Tasks.ForParallel(0, w.Length, i =>
            {
                var dw_a = dw_Adam(i, diff[i]);
                var dw_s = dw_SGD(i, diff[i], max);

                var vvd = diff[i] - pd[i];
                var aad = vvd - vd[i];

                var rt = Math.Abs(vvd / (aad + eps));
                var np = 1.0 / (1 + Math.Exp(-(rt - center) / Math.PI));
                var dw = (np * dw_a + (1 - np) * dw_s);

                delta += dw;
                if (doUpdate) { w[i] = w[i] - (eta) * dw; }

                pd[i] = ((1 - np) * pd[i] + (np) * diff[i]);
                vd[i] = ((1 - np) * vd[i] + (np) * vvd);
                ad[i] = ((1 - np) * ad[i] + (np) * aad);
            });

            return delta;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace CNNPlatform.Function.Process.Optimizer
{
    class AdaDelta : OptimizerBase
    {
        public override double Update(ref Real[] w, Real[] diff, double rho)
        {
            return 0;
        }
    }
}

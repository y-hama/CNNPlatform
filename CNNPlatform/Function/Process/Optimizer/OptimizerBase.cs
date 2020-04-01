using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Function.Process.Optimizer
{
    abstract class OptimizerBase
    {
        public abstract double Update(ref Components.Real[] w, Components.Real[] diff, double rho);
    }
}

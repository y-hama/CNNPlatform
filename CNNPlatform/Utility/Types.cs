using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Utility
{
    class Types
    {
        public enum Optimizer
        {
            Adam,
            AMSGrad,
            AdaSelf,
        }

        public enum Activator
        {
            Sigmoid,
            ReLU,
            Mish,
        }
    }
}

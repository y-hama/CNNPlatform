using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Layer
{
    class Affine : LayerBase
    {
        public Affine(bool createFunctions) : base(createFunctions)
        {
        }

        protected override void FunctionCreator()
        {
            ForwardFunction = new DedicatedFunction.Process.AffineForward();
            BackFunction = new DedicatedFunction.Process.AffineBack();
        }
    }
}

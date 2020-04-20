using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Layer
{
    class Pooling : LayerBase
    {
        public Pooling(bool createFunctions) : base(createFunctions)
        {
        }

        protected override void FunctionCreator()
        {
            ForwardFunction = new DedicatedFunction.Process.PoolingForward();
            BackFunction = new DedicatedFunction.Process.PoolingBack();
        }
    }
}

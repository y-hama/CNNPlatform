using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Layer
{
    class Reshape : LayerBase
    {
        public Reshape(bool createFunctions) : base(createFunctions)
        {
        }

        protected override void FunctionCreator()
        {
            ForwardFunction = new DedicatedFunction.Process.ReshapeForward();
            BackFunction = new DedicatedFunction.Process.ReshapeBack();
        }
    }
}

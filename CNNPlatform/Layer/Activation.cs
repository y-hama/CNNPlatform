﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Layer
{
    class Activation : LayerBase
    {
        public Activation(bool createFunctions) : base(createFunctions)
        {
        }

        protected override void FunctionCreator()
        {
            ForwardFunction = new DedicatedFunction.Process.ActivationForward();
            BackFunction = new DedicatedFunction.Process.ActivationBack();
        }
    }
}

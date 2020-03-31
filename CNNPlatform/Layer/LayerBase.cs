using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Layer
{
    class LayerBase
    {
        public Components.GPGPU.ComputeVariable Variable { get; set; }

        public Components.GPGPU.Function.FunctionBase ForwardFunction { get; set; }
        public Components.GPGPU.Function.FunctionBase BackFunction { get; set; }
    }
}

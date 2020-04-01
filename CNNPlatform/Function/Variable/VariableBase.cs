using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Function.Variable
{
    abstract class VariableBase : Components.GPGPU.ComputeVariable
    {
        public int BatchCount { get; set; }

        public int InWidth { get; set; }
        public int InHeight { get; set; }

        public int InputChannels { get; set; }

        public double OutScale { get; set; } = 1;
        public int OutWidth { get; protected set; } = 0;
        public int OutHeight { get; protected set; } = 0;
        public int OutputChannels { get; set; }

        public int InSize { get { return InWidth * InHeight; } }
        public int InArea { get { return InputChannels * InSize; } }
        public int InTotal { get { return BatchCount * InArea; } }

        public int OutSize { get { return OutWidth * OutHeight; } }
        public int OutArea { get { return OutputChannels * OutSize; } }
        public int OutTotal { get { return BatchCount * OutArea; } }


        public Components.RNdMatrix Input;
        public Components.RNdMatrix Output;

        public Components.RNdMatrix Sigma;
        public Components.RNdMatrix Propagator;

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Variable
{
    abstract class VariableBase : Components.GPGPU.ComputeVariable
    {
        public string GetSizeStatus
        {
            get
            {
                return string.Format("(c:{0}, w:{1}, h:{2}) -> (c:{3}, w:{4}, h:{5})",
                    InputChannels, InWidth, InHeight, OutputChannels, OutWidth, OutHeight);
            }
        }


        public int BatchCount { get; set; }

        public int InWidth { get; set; }
        public int InHeight { get; set; }

        public int InputChannels { get; set; }

        public int OutWidth { get; set; } = 0;
        public int OutHeight { get; set; } = 0;
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

        public Components.Real[] Error { get; private set; } = new Components.Real[8];

        protected abstract void ConfirmField(object shared);
        public VariableBase Confirm(object shared)
        {
            ConfirmField(shared);
            Input = new Components.RNdMatrix(BatchCount, InputChannels, InWidth, InHeight);
            Output = new Components.RNdMatrix(BatchCount, OutputChannels, OutWidth, OutHeight);
            Sigma = new Components.RNdMatrix(BatchCount, OutputChannels, OutWidth, OutHeight);
            Propagator = new Components.RNdMatrix(BatchCount, InputChannels, InWidth, InHeight);
            return this;
        }

        public abstract string GetStatus { get; }

        public abstract void UpdateParameter(object parameter);
        public abstract void OverwriteParameter(ref object parameter);

        public abstract string EncodeParameter();
        public abstract string EncodeOption();
    }
}

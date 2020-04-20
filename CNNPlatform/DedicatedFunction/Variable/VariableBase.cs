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

        public string GetCaption
        {
            get
            {
                return string.Format("{0}", this.GetType().Name.Substring(0, 2));
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

        protected abstract void EncodeParameterCore(ref string res);
        public string EncodeParameter()
        {
            string res = string.Empty;
            res += BatchCount.ToString() + " ";
            res += InWidth.ToString() + " ";
            res += InHeight.ToString() + " ";
            res += InputChannels.ToString() + " ";
            res += OutWidth.ToString() + " ";
            res += OutHeight.ToString() + " ";
            res += OutputChannels.ToString() + " ";
            EncodeParameterCore(ref res);
            return res;
        }
        public abstract string EncodeOption();

        public abstract void CoreClone(ref VariableBase _clone);

        public VariableBase Clone()
        {
            var clone = (VariableBase)Activator.CreateInstance(this.GetType());
            clone.BatchCount = BatchCount;
            clone.InWidth = BatchCount;
            clone.InHeight = BatchCount;
            clone.InputChannels = BatchCount;
            clone.OutWidth = BatchCount;
            clone.OutHeight = BatchCount;
            clone.OutputChannels = BatchCount;
            CoreClone(ref clone);
            return clone;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Variable
{
    class PoolingVariable : VariableBase
    {
        public int CompressSize { get; set; } = 1;
        public int ExpandSize { get; set; } = 1;

        public Components.RNdMatrix Map;

        public override string GetStatus
        {
            get
            {
                string ext = string.Empty;
                ext += CompressSize.ToString() + ", ";
                ext += ExpandSize.ToString() + ", ";
                return ext;
            }
        }

        protected override void ConfirmField(object shared)
        {
            CompressSize = CompressSize <= 0 ? 1 : CompressSize;
            ExpandSize = ExpandSize <= 0 ? 1 : ExpandSize;

            OutputChannels = InputChannels;
            OutWidth = (int)(((double)ExpandSize / (double)CompressSize) * InWidth);
            OutHeight = (int)(((double)ExpandSize / (double)CompressSize) * InHeight);

            Map = new Components.RNdMatrix(BatchCount, OutputChannels, InWidth, InHeight);

            if (shared != null)
            {
                var obj = shared as Utility.Shared.ModelParameter;
                var w = new Utility.Shared.ModelParameter.WeightData(0);
                obj.Weignt.Add(w);
            }
        }

        public override void UpdateParameter(object parameter)
        {
        }

        public override void OverwriteParameter(ref object parameter)
        {
        }

        public override string EncodeParameter()
        {
            string res = string.Empty;
            res += BatchCount.ToString() + " ";
            res += InWidth.ToString() + " ";
            res += InHeight.ToString() + " ";
            res += InputChannels.ToString() + " ";
            res += OutWidth.ToString() + " ";
            res += OutHeight.ToString() + " ";
            res += OutputChannels.ToString() + " ";

            res += CompressSize.ToString() + " ";
            res += ExpandSize.ToString() + " ";
            return res;
        }

        public override string EncodeOption()
        {
            string res = string.Empty;
            return res;
        }
    }
}

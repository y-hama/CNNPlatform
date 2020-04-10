using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Variable
{
    class ActivationVariable : VariableBase
    {
        public Utility.Types.Activator ActivationType { get; set; } = Utility.Types.Activator.ReLU;

        protected override void ConfirmField(object shared)
        {
            if (shared != null)
            {
                var obj = shared as Utility.Shared.ModelParameter;
                var w = new Utility.Shared.ModelParameter.WeightData(0);
                obj.Weignt.Add(w);
            }
            OutputChannels = InputChannels;
            OutWidth = InWidth;
            OutHeight = InHeight;
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
            return res;
        }

        public override string EncodeOption()
        {
            string res = string.Empty;
            return res;
        }
    }
}

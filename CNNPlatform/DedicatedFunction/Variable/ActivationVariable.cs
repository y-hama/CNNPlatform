using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace CNNPlatform.DedicatedFunction.Variable
{
    class ActivationVariable : VariableBase
    {
        public Utility.Types.Activator ActivationType { get; set; } = Utility.Types.Activator.ReLU;


        public override string GetStatus
        {
            get
            {
                string ext = string.Empty;
                ext += ActivationType.ToString() + ", ";
                return ext;
            }
        }

        protected override void ConfirmField(object shared)
        {
            OutputChannels = InputChannels;
            OutWidth = InWidth;
            OutHeight = InHeight;
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

        protected override void EncodeParameterCore(ref string res)
        {
            res += ActivationType.ToString() + " ";
        }

        public override string EncodeOption()
        {
            string res = string.Empty;
            return res;
        }

        protected override void DecodeParameterCore(object[] values)
        {
            ActivationType = (Utility.Types.Activator)Enum.Parse(typeof(Utility.Types.Activator), values[0].ToString());
        }

        protected override void DecodeOption(List<object> values)
        {
        }

        public override void SaveObject(DirectoryInfo location)
        {
        }

        public override void CoreClone(ref VariableBase _clone)
        {
            (_clone as ActivationVariable).ActivationType = ActivationType;
        }
    }
}

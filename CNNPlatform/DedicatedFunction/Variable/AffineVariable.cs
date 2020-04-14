using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Variable
{
    class AffineVariable : VariableBase
    {
        public Utility.Types.Optimizer OptimizerType { get; set; } = Utility.Types.Optimizer.Adam;

        public double Rho { get; set; } = 0.001;

        public Components.RNdMatrix Weight;

        public Components.Real[] WeightDifference;

        public override string GetStatus
        {
            get
            {
                string ext = string.Empty;
                return ext;
            }
        }

        protected override void ConfirmField(object shared)
        {
            if (shared != null)
            {
                var obj = shared as Utility.Shared.ModelParameter;
                var w = new Utility.Shared.ModelParameter.WeightData(2);
                w.Data[0] = new Components.RNdMatrix(InArea + 1, OutArea, 1, 1);
                Utility.Randomizer.Noize(ref w.Data[0].Data, Utility.Randomizer.Sign.Both, 1.0 / (InArea));
                obj.Weignt.Add(w);

                Weight = new Components.RNdMatrix(w.Data[0].Shape);
                Weight.Data = (w.Data[0].Data.Clone()) as Components.Real[];
                WeightDifference = w.Difference.Clone() as Components.Real[];
            }
            else
            {
                Weight = (new Components.RNdMatrix(InArea, OutArea, 1, 1)) as Components.RNdMatrix;
            }
        }

        public override void UpdateParameter(object parameter)
        {
            var weight = parameter as Utility.Shared.ModelParameter.WeightData;

            Weight.Data = weight.Data[0].Data.Clone() as Components.Real[];
            WeightDifference = weight.Difference.Clone() as Components.Real[];
        }

        public override void OverwriteParameter(ref object parameter)
        {
            var weight = parameter as Utility.Shared.ModelParameter.WeightData;

            weight.Data[0].Data = Weight.Data.Clone() as Components.Real[];
            weight.Difference = WeightDifference.Clone() as Components.Real[];
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

            res += OptimizerType.ToString() + " ";
            res += Rho.ToString() + " ";
            return res;
        }

        public override string EncodeOption()
        {
            var str = Weight.Data.Select(x => x.ToString()).ToArray();
            var strw = "{" + Weight.BatchSize + "," + Weight.Channels + "," + Weight.Width + "," + Weight.Height + "}(";
            for (int i = 0; i < Weight.Length; i++)
            {
                if (i != 0) { strw += ","; }
                strw += str[i];
            }
            strw += ")";
            return strw;
        }
    }
}

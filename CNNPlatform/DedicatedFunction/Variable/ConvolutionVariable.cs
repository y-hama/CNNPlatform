using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Variable
{
    class ConvolutionVariable : VariableBase
    {
        public double OutScale { get; set; } = 1;

        public int KernelSize { get; set; }
        public int KernelArea { get { return 2 * KernelSize + 1; } }
        public int KernelLength { get { return KernelArea * KernelArea; } }
        public int KernelExpand { get; set; } = 1;

        public Utility.Types.Optimizer OptimizerType { get; set; } = Utility.Types.Optimizer.Adam;

        public Components.RNdMatrix WeightBias;
        public Components.RNdMatrix WeightKernel;

        public Components.Real[] WeightDifference;

        public double Rho { get; set; } = 0.0001;

        protected override void ConfirmField(object shared)
        {
            if (shared != null)
            {
                var obj = shared as Utility.Shared.ModelParameter;
                var w = new Utility.Shared.ModelParameter.WeightData(2);
                w.Data[0] = new Components.RNdMatrix(OutputChannels, 1, 1, 1);
                w.Data[1] = new Components.RNdMatrix(InputChannels, OutputChannels, 2 * KernelSize + 1, 2 * KernelSize + 1);
                Utility.Randomizer.Noize(ref w.Data[0].Data, Utility.Randomizer.Sign.Both, 1.0 / (InputChannels * KernelLength));
                Utility.Randomizer.Noize(ref w.Data[1].Data, Utility.Randomizer.Sign.Both, 1.0 / (KernelLength));
                obj.Weignt.Add(w);

                WeightBias = new Components.RNdMatrix(w.Data[0].Shape);
                WeightBias.Data = (w.Data[0].Data.Clone()) as Components.Real[];
                WeightKernel = new Components.RNdMatrix(w.Data[1].Shape);
                WeightKernel.Data = (w.Data[1].Data.Clone()) as Components.Real[];
                WeightDifference = w.Difference.Clone() as Components.Real[];
            }
            else
            {
                WeightBias = (new Components.RNdMatrix(OutputChannels, 1, 1, 1)) as Components.RNdMatrix;
                WeightKernel = (new Components.RNdMatrix(InputChannels, OutputChannels, 2 * KernelSize + 1, 2 * KernelSize + 1)) as Components.RNdMatrix;
            }

            OutWidth = (int)(OutScale * InWidth);
            OutHeight = (int)(OutScale * InHeight);
        }

        public override void UpdateParameter(object parameter)
        {
            var weight = parameter as Utility.Shared.ModelParameter.WeightData;

            WeightBias.Data = weight.Data[0].Data.Clone() as Components.Real[];
            WeightKernel.Data = weight.Data[1].Data.Clone() as Components.Real[];
            WeightDifference = weight.Difference.Clone() as Components.Real[];
        }

        public override void OverwriteParameter(ref object parameter)
        {
            var weight = parameter as Utility.Shared.ModelParameter.WeightData;

            weight.Data[0].Data = WeightBias.Data.Clone() as Components.Real[];
            weight.Data[1].Data = WeightKernel.Data.Clone() as Components.Real[];
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

            res += OutScale.ToString() + " ";
            res += KernelSize.ToString() + " ";
            res += KernelExpand.ToString() + " ";
            res += OptimizerType.ToString() + " ";
            res += Rho.ToString() + " ";

            return res;
        }

        public override string EncodeOption()
        {
            var strw = "{" + WeightBias.BatchSize + "," + WeightBias.Channels + "," + WeightBias.Width + "," + WeightBias.Height + "}(";
            for (int i = 0; i < WeightBias.Length; i++)
            {
                if (i != 0) { strw += ","; }
                strw += WeightBias.Data[i].ToString();
            }
            strw += ")";

            var strk = "{" + WeightKernel.BatchSize + "," + WeightKernel.Channels + "," + WeightKernel.Width + "," + WeightKernel.Height + "}(";
            for (int i = 0; i < WeightKernel.Length; i++)
            {
                if (i != 0) { strk += ","; }
                strk += WeightKernel.Data[i].ToString();
            }
            strk += ")";
            return strw + "\n" + strk;
        }
    }
}

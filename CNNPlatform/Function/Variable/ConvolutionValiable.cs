using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Function.Variable
{
    class ConvolutionValiable : VariableBase
    {
        public int KernelSize { get; set; }
        public int KernelArea { get { return 2 * KernelSize + 1; } }
        public int KernelLength { get { return KernelArea * KernelArea; } }
        public int KernelExpand { get; set; } = 1;


        public Components.RNdMatrix WeightBias;
        public Components.RNdMatrix WeightKernel;

        public Components.Real[] WeightDifference;

        public double Rho { get; set; } = 0.0001;

        protected override void ConfirmField(object shared)
        {
            if (shared != null)
            {
                var obj = shared as SharedObject;
                var w = new CNNPlatform.SharedObject.WeightData(2);
                w.Data[0] = new Components.RNdMatrix(InputChannels, OutputChannels, 1, 1);
                w.Data[1] = new Components.RNdMatrix(InputChannels, OutputChannels, 2 * KernelSize + 1, 2 * KernelSize + 1);
                Utility.Randomizer.Noize(ref w.Data[0].Data, Utility.Randomizer.Sign.Both, 0.1 / (InputChannels * OutputChannels));
                Utility.Randomizer.Noize(ref w.Data[1].Data, Utility.Randomizer.Sign.Both, 0.1 / Math.Pow(2 * KernelSize + 1, 2));
                obj.Weignt.Add(w);

                WeightBias = new Components.RNdMatrix(w.Data[0].Shape);
                WeightBias.Data = (w.Data[0].Data.Clone()) as Components.Real[];
                WeightKernel = new Components.RNdMatrix(w.Data[1].Shape);
                WeightKernel.Data = (w.Data[1].Data.Clone()) as Components.Real[];
                WeightDifference = w.Difference.Clone() as Components.Real[];
            }
            else
            {
                WeightBias = (new Components.RNdMatrix(InputChannels, OutputChannels, 1, 1)) as Components.RNdMatrix;
                WeightKernel = (new Components.RNdMatrix(InputChannels, OutputChannels, 2 * KernelSize + 1, 2 * KernelSize + 1)) as Components.RNdMatrix;
            }

            OutWidth = (int)(OutScale * InWidth);
            OutHeight = (int)(OutScale * InHeight);
            Output = new Components.RNdMatrix(BatchCount, OutputChannels, OutWidth, OutHeight);

            Propagator = new Components.RNdMatrix(BatchCount, InputChannels, InWidth, InHeight);
        }

        public override void UpdateParameter(object parameter)
        {
            var weight = parameter as SharedObject.WeightData;

            WeightBias.Data = weight.Data[0].Data.Clone() as Components.Real[];
            WeightKernel.Data = weight.Data[1].Data.Clone() as Components.Real[];
            WeightDifference = weight.Difference.Clone() as Components.Real[];
        }

        public override void OverwriteParameter(ref object parameter)
        {
            var weight = parameter as SharedObject.WeightData;

            weight.Data[0].Data = WeightBias.Data.Clone() as Components.Real[];
            weight.Data[1].Data = WeightKernel.Data.Clone() as Components.Real[];
            weight.Difference = WeightDifference.Clone() as Components.Real[];
        }

    }
}

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


        public Components.RNdMatrix WeightKernel;
        public Components.RNdMatrix WeightBias;

        public double Rho { get; set; } = 0.0001;

        protected override void ConfirmField(object shared)
        {
            if (shared != null)
            {
                var obj = shared as SharedObject;
                var w = new CNNPlatform.SharedObject.WeigntData();
                w.Data = new Components.RNdObject[2];
                WeightBias = (w.Data[0] = new Components.RNdMatrix(InputChannels, OutputChannels, 1, 1)) as Components.RNdMatrix;
                WeightKernel = (w.Data[1] = new Components.RNdMatrix(InputChannels, OutputChannels, 2 * KernelSize + 1, 2 * KernelSize + 1)) as Components.RNdMatrix;
                //Utility.Randomizer.Noize(ref WeightBias.Data, Utility.Randomizer.Sign.Both, 1.0 / (InputChannels * OutputChannels));
                Utility.Randomizer.Noize(ref WeightKernel.Data, Utility.Randomizer.Sign.Both, 1.0 / Math.Pow(2 * KernelSize + 1, 2));
                obj.Weignt.Add(w);
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
            var weight = parameter as SharedObject.WeigntData;

            WeightBias.Data = weight.Data[0].Data.Clone() as Components.Real[];
            WeightKernel.Data = weight.Data[1].Data.Clone() as Components.Real[];
        }


    }
}

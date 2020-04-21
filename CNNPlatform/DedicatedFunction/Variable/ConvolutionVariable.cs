using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

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
        public double Rho { get; set; } = 0.0001;

        public Components.RNdMatrix WeightBias;
        public Components.RNdMatrix WeightKernel;

        public Components.Real[] WeightDifference;


        public override string GetStatus
        {
            get
            {
                string ext = string.Empty;
                ext += KernelSize.ToString() + ", ";
                ext += KernelExpand.ToString() + ", ";
                ext += OptimizerType.ToString() + ", ";
                return ext;
            }
        }

        protected override void ConfirmField(object shared)
        {
            OutWidth = (int)(OutScale * InWidth);
            OutHeight = (int)(OutScale * InHeight);

            double sd_b = Math.Sqrt(1.0 / (InputChannels * KernelLength));
            double sd_k = Math.Sqrt(1.0 / (InputChannels * KernelLength));
            if (shared != null)
            {
                var obj = shared as Utility.Shared.ModelParameter;
                var w = new Utility.Shared.ModelParameter.WeightData(2);
                w.Data[0] = new Components.RNdMatrix(OutputChannels, 1, 1, 1);
                w.Data[1] = new Components.RNdMatrix(InputChannels, OutputChannels, 2 * KernelSize + 1, 2 * KernelSize + 1);
                Utility.Randomizer.Noize(ref w.Data[0].Data, Utility.Randomizer.Sign.Both, 0, sd_b);
                Utility.Randomizer.Noize(ref w.Data[1].Data, Utility.Randomizer.Sign.Both, 0, sd_k);
                obj.Weignt.Add(w);

                WeightBias = new Components.RNdMatrix(w.Data[0].Shape);
                WeightBias.Data = (w.Data[0].Data.Clone()) as Components.Real[];
                WeightKernel = new Components.RNdMatrix(w.Data[1].Shape);
                WeightKernel.Data = (w.Data[1].Data.Clone()) as Components.Real[];
                WeightDifference = w.Difference.Clone() as Components.Real[];
            }
            else
            {
                if (!ObjectDecoded)
                {
                    WeightBias = (new Components.RNdMatrix(OutputChannels, 1, 1, 1)) as Components.RNdMatrix;
                    WeightKernel = (new Components.RNdMatrix(InputChannels, OutputChannels, 2 * KernelSize + 1, 2 * KernelSize + 1)) as Components.RNdMatrix;
                    Utility.Randomizer.Noize(ref WeightBias.Data, Utility.Randomizer.Sign.Both, 0, sd_b);
                    Utility.Randomizer.Noize(ref WeightKernel.Data, Utility.Randomizer.Sign.Both, 0, sd_k);
                }
                WeightDifference = new Components.Real[2];
            }
        }

        public override void UpdateParameter(object parameter)
        {
            if (parameter != null)
            {
                var weight = parameter as Utility.Shared.ModelParameter.WeightData;

                WeightBias.Data = weight.Data[0].Data.Clone() as Components.Real[];
                WeightKernel.Data = weight.Data[1].Data.Clone() as Components.Real[];
                WeightDifference = weight.Difference.Clone() as Components.Real[];
            }
        }

        public override void OverwriteParameter(ref object parameter)
        {
            if (parameter != null)
            {
                var weight = parameter as Utility.Shared.ModelParameter.WeightData;

                weight.Data[0].Data = WeightBias.Data.Clone() as Components.Real[];
                weight.Data[1].Data = WeightKernel.Data.Clone() as Components.Real[];
                weight.Difference = WeightDifference.Clone() as Components.Real[];
            }
        }

        protected override void EncodeParameterCore(ref string res)
        {
            res += OutScale.ToString() + " ";
            res += KernelSize.ToString() + " ";
            res += KernelExpand.ToString() + " ";
            res += OptimizerType.ToString() + " ";
            res += Rho.ToString() + " ";
        }

        public override string EncodeOption()
        {
            return WeightBias.Hash + "\n" + WeightKernel.Hash;
        }

        protected override void DecodeParameterCore(object[] values)
        {
            OutScale = Convert.ToDouble(values[0]);
            KernelSize = Convert.ToInt32(values[1]);
            KernelExpand = Convert.ToInt32(values[2]);
            OptimizerType = (Utility.Types.Optimizer)Enum.Parse(typeof(Utility.Types.Optimizer), values[3].ToString());
            Rho = Convert.ToDouble(values[4]);
        }

        protected override void DecodeOption(List<object> values)
        {
            WeightBias = values[0] as RNdMatrix;
            WeightKernel = values[1] as RNdMatrix;
        }

        public override void SaveObject(DirectoryInfo location)
        {
            WeightBias.Save(location);
            WeightKernel.Save(location);
        }

        public override void CoreClone(ref VariableBase _clone)
        {
            (_clone as ConvolutionVariable).OutScale = OutScale;
            (_clone as ConvolutionVariable).KernelSize = KernelSize;
            (_clone as ConvolutionVariable).KernelExpand = KernelExpand;
            (_clone as ConvolutionVariable).OptimizerType = OptimizerType;
            (_clone as ConvolutionVariable).Rho = Rho;
            (_clone as ConvolutionVariable).WeightBias = WeightBias.Clone() as Components.RNdMatrix; ;
            (_clone as ConvolutionVariable).WeightKernel = WeightKernel.Clone() as Components.RNdMatrix; ;
        }
    }
}

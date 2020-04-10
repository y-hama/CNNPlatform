using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace CNNPlatform.DedicatedFunction.Process
{
    class ActivationForward : Components.GPGPU.Function.FunctionBase
    {
        protected override void CreateGpuSource()
        {
        }

        #region 
        private int BatchCount;

        private int InWidth;
        private int InHeight;
        private int InputChannels;

        private int OutWidth;
        private int OutHeight;
        private int OutputChannels;

        private int InSize;
        private int InArea;
        private int InTotal;

        private int OutSize;
        private int OutArea;
        private int OutTotal;

        private Utility.Types.Activator ActivationType;

        private Components.Real[] Input;
        private Components.Real[] Output;
        #endregion

        protected override void ConvertVariable(ComputeVariable _variable)
        {
            var variable = _variable as Variable.ActivationVariable;

            BatchCount = variable.BatchCount;

            InWidth = variable.InWidth;
            InHeight = variable.InHeight;
            InputChannels = variable.InputChannels;

            OutWidth = variable.OutWidth;
            OutHeight = variable.OutHeight;
            OutputChannels = variable.OutputChannels;

            InSize = variable.InSize;
            InArea = variable.InArea;
            InTotal = variable.InTotal;

            OutSize = variable.OutSize;
            OutArea = variable.OutArea;
            OutTotal = variable.OutTotal;

            ActivationType = variable.ActivationType;

            Input = variable.Input.Data;
            Output = variable.Output.Data;
        }

        protected override void CpuFunction()
        {
            switch (ActivationType)
            {
                case Utility.Types.Activator.Sigmoid:
                    Parallel(0, OutTotal, i0 =>
                    {
                        float x = Input[i0];
                        Output[i0] = 1.0 / (1 + exp(-x));
                    });
                    break;
                case Utility.Types.Activator.ReLU:
                    Parallel(0, OutTotal, i0 =>
                    {
                        float x = Input[i0];
                        Output[i0] = (x > 0 ? 1 : 0.01) * x;
                    });
                    break;
                case Utility.Types.Activator.Mish:
                    Parallel(0, OutTotal, i0 =>
                    {
                        float x = Input[i0];
                        float s = log(1.0f + exp(x));
                        Output[i0] = x * tanh(s);
                    });
                    break;
                default:
                    break;
            }
        }

        protected override void GpuFunction()
        {
            switch (ActivationType)
            {
                case Utility.Types.Activator.Sigmoid:
                    SwitchSellection(GpuSource[0].Name);
                    break;
                case Utility.Types.Activator.ReLU:
                    SwitchSellection(GpuSource[1].Name);
                    break;
                case Utility.Types.Activator.Mish:
                    SwitchSellection(GpuSource[2].Name);
                    break;
                default:
                    break;
            }
        }
    }
}

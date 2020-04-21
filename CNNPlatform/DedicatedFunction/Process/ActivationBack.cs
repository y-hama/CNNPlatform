using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace CNNPlatform.DedicatedFunction.Process
{
    class ActivationBack : Components.GPGPU.Function.ParameterizedFunctionBase
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

        private Components.Real[] Sigma;
        private Components.Real[] Propagator;

        private Components.Real[] Error;
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
            Sigma = variable.Sigma.Data;
            Propagator = variable.Propagator.Data;

            Error = variable.Error;
        }

        protected override void CpuFunction()
        {
            switch (ActivationType)
            {
                case Utility.Types.Activator.Sigmoid:
                    Parallel(0, OutTotal, i0 =>
                    {
                        float x = Output[i0];
                        float s = 1.0f / (1 + exp(-x));
                        Propagator[i0] = x * Sigma[i0];
                    });
                    break;
                case Utility.Types.Activator.ReLU:
                    Parallel(0, OutTotal, i0 =>
                    {
                        float x = Output[i0];
                        Propagator[i0] = (x > 0 ? 1 : 0) * Sigma[i0];
                    });
                    break;
                case Utility.Types.Activator.Mish:
                    Parallel(0, OutTotal, i0 =>
                    {
                        float x = Output[i0];
                        float a = 0;
                        if (x > 2 * Math.PI)
                        {
                            a = 1;
                        }
                        else if (x < -2 * Math.PI)
                        {
                            a = 0;
                        }
                        else
                        {
                            float w = 4 * (x + 1) + 4 * exp(2 * x) + exp(3 * x) + (4 * x + 6) * exp(x);
                            float s = 2 * exp(x) + exp(2 * x) + 2;
                            a = (exp(x) * w / (s * s));
                        }
                        Propagator[i0] = a * Sigma[i0];
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

        protected override bool UpdateConditionCheck(ref bool doUpdateCalculation)
        {
            doUpdateCalculation = (Variable as DedicatedFunction.Variable.VariableBase).UpdateRequest;
            return true;
        }

        public override void Update(bool doUpdateCalculation)
        {
            (Variable as DedicatedFunction.Variable.VariableBase).CalcurationError(ref Error);
        }
    }
}

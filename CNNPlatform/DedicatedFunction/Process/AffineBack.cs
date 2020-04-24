using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace CNNPlatform.DedicatedFunction.Process
{
    class AffineBack : DedicatedParameterizedFunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new GpgpuSource.gp_affineback_weight());
            AddSource(new GpgpuSource.gp_affineback_prop());
        }

        Optimizer.OptimizerBase WeightOptimizer { get; set; }

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

        private Utility.Types.Optimizer OptimizerType;
        private double Rho;

        private Components.Real[] Weight;
        private Components.Real[] dWeight;

        private Components.Real[] Input;
        private Components.Real[] Output;
        private Components.Real[] Sigma;
        private Components.Real[] Propagator;

        private Components.Real[] Error;

        private Components.Real[] Difference;
        #endregion

        protected override void CreateOption()
        {
            WeightOptimizer = Optimizer.OptimizerBase.CreateInstance(OptimizerType, (Variable as Variable.AffineVariable).OptimizerWeightBuffer, Weight);
        }

        protected override void ConvertVariable(ComputeVariable _variable)
        {
            var variable = _variable as Variable.AffineVariable;

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

            OptimizerType = variable.OptimizerType;
            Rho = variable.Rho;

            Weight = variable.Weight.Data;

            Input = variable.Input.Data;
            Output = variable.Output.Data;
            Sigma = variable.Sigma.Data;
            Propagator = variable.Propagator.Data;

            Error = variable.Error;
            Difference = variable.WeightDifference;
        }

        protected override void CpuFunction()
        {
            dWeight = Weight.Clone() as Components.Real[];
            Parallel(0, (InArea + 1) * OutArea, i0 =>
            {
                int ool = (int)(i0 / (InArea + 1));
                int iil = i0 - (int)(ool * (InArea + 1));
                int widx = ool * (InArea + 1) + (iil);
                dWeight[widx] = 0;
                for (int b = 0; b < BatchCount; b++)
                {
                    int sidx = b * OutArea + ool;
                    int iidx = b * InArea + iil;
                    dWeight[widx] += Sigma[sidx] * (iil == 0 ? 1 : Input[iidx - 1]);
                }
                dWeight[widx] /= BatchCount;
            });

            Parallel(0, BatchCount, i0 =>
            {
                Parallel(0, InArea, i1 =>
                {
                    int iidx = i0 * InArea + i1;
                    Propagator[iidx] = 0;
                    for (int o = 0; o < OutArea; o++)
                    {
                        int oidx = i0 * OutArea + o;
                        int widx = o * (InArea + 1) + (i1 + 1);
                        Propagator[iidx] += Sigma[oidx] * Weight[widx];
                    }
                });
            });
        }

        protected override void GpuFunction()
        {
            dWeight = Weight.Clone() as Components.Real[];
            SwitchSellection(GpuSource[0].Name);
            using (var _input = ConvertBuffer(Input))
            using (var _sigma = ConvertBuffer(Sigma))
            using (var _dweight = ConvertBuffer(dWeight))
            {
                SetParameter(_input);
                SetParameter(_sigma);
                SetParameter(_dweight);

                SetParameter(BatchCount, ValueMode.INT);
                SetParameter(InWidth, ValueMode.INT);
                SetParameter(InHeight, ValueMode.INT);
                SetParameter(InputChannels, ValueMode.INT);
                SetParameter(OutWidth, ValueMode.INT);
                SetParameter(OutHeight, ValueMode.INT);
                SetParameter(OutputChannels, ValueMode.INT);
                SetParameter(InSize, ValueMode.INT);
                SetParameter(InArea, ValueMode.INT);
                SetParameter(InTotal, ValueMode.INT);
                SetParameter(OutSize, ValueMode.INT);
                SetParameter(OutArea, ValueMode.INT);
                SetParameter(OutTotal, ValueMode.INT);

                Execute((InArea + 1) * OutArea);
                ReadBuffer(_dweight, ref dWeight);
            }

            SwitchSellection(GpuSource[1].Name);
            using (var _propagator = ConvertBuffer(Propagator))
            using (var _sigma = ConvertBuffer(Sigma))
            using (var _weight = ConvertBuffer(Weight))
            {
                SetParameter(_propagator);
                SetParameter(_sigma);
                SetParameter(_weight);

                SetParameter(BatchCount, ValueMode.INT);
                SetParameter(InWidth, ValueMode.INT);
                SetParameter(InHeight, ValueMode.INT);
                SetParameter(InputChannels, ValueMode.INT);
                SetParameter(OutWidth, ValueMode.INT);
                SetParameter(OutHeight, ValueMode.INT);
                SetParameter(OutputChannels, ValueMode.INT);
                SetParameter(InSize, ValueMode.INT);
                SetParameter(InArea, ValueMode.INT);
                SetParameter(InTotal, ValueMode.INT);
                SetParameter(OutSize, ValueMode.INT);
                SetParameter(OutArea, ValueMode.INT);
                SetParameter(OutTotal, ValueMode.INT);

                Execute(BatchCount, InArea);
                ReadBuffer(_propagator, ref Propagator);
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
            double ep = ((double)1.0 / (InputChannels * OutputChannels));
            Difference[0] = WeightOptimizer.Update(ref Weight, dWeight, doUpdateCalculation, ep * Rho);
        }
    }
}

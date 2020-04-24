using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace CNNPlatform.DedicatedFunction.Process
{
    class ConvolutionBack : DedicatedParameterizedFunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new Process.GpgpuSource.gp_convback_bias());
            AddSource(new Process.GpgpuSource.gp_convback_kernel());
            AddSource(new Process.GpgpuSource.gp_convback_prop());
        }

        Optimizer.OptimizerBase BiasOptimizer { get; set; }
        Optimizer.OptimizerBase KernelOptimizer { get; set; }

        #region 
        private int BatchCount;

        private int InWidth;
        private int InHeight;
        private int InputChannels;

        private float OutScale;
        private int OutWidth;
        private int OutHeight;
        private int OutputChannels;

        private int InSize;
        private int InArea;
        private int InTotal;

        private int OutSize;
        private int OutArea;
        private int OutTotal;

        private int KernelSize;
        private int KernelArea;
        private int KernelLength;
        private int KernelExpand;

        public Utility.Types.Optimizer OptimizerType { get; set; } = Utility.Types.Optimizer.Adam;
        private double rho;

        private Components.Real[] Input;
        private Components.Real[] Output;
        private Components.Real[] Sigma;
        private Components.Real[] Propagator;

        private Components.Real[] WeightKernel;
        private Components.Real[] WeightBias;
        private Components.Real[] dKernel;
        private Components.Real[] dBias;

        private Components.Real[] Error;

        private Components.Real[] Difference;
        #endregion

        protected override void CreateOption()
        {
            BiasOptimizer = Optimizer.OptimizerBase.CreateInstance(OptimizerType, (Variable as Variable.ConvolutionVariable).OptimizerBiasBuffer, WeightBias);
            KernelOptimizer = Optimizer.OptimizerBase.CreateInstance(OptimizerType, (Variable as Variable.ConvolutionVariable).OptimizerKernelBuffer, WeightKernel);
        }

        protected override void ConvertVariable(ComputeVariable _variable)
        {
            var variable = _variable as Variable.ConvolutionVariable;

            BatchCount = variable.BatchCount;

            InWidth = variable.InWidth;
            InHeight = variable.InHeight;
            InputChannels = variable.InputChannels;

            OutScale = (float)variable.OutScale;
            OutWidth = variable.OutWidth;
            OutHeight = variable.OutHeight;
            OutputChannels = variable.OutputChannels;

            InSize = variable.InSize;
            InArea = variable.InArea;
            InTotal = variable.InTotal;

            OutSize = variable.OutSize;
            OutArea = variable.OutArea;
            OutTotal = variable.OutTotal;

            KernelSize = variable.KernelSize;
            KernelArea = variable.KernelArea;
            KernelLength = variable.KernelLength;
            KernelExpand = variable.KernelExpand;

            OptimizerType = variable.OptimizerType;
            rho = variable.Rho;

            Input = variable.Input.Data;
            Output = variable.Output.Data;
            Sigma = variable.Sigma.Data;
            Propagator = variable.Propagator.Data;
            WeightKernel = variable.WeightKernel.Data;
            WeightBias = variable.WeightBias.Data;

            Error = variable.Error;
            Difference = variable.WeightDifference;
        }

        protected override void CpuFunction()
        {
            dBias = (Components.Real[])WeightBias.Clone();
            Parallel(0, OutputChannels, i0 =>
            {
                int bidx = i0;
                dBias[bidx] = 0;
                for (int b = 0; b < BatchCount; b++)
                {
                    for (int i = 0; i < OutSize; i++)
                    {
                        int oidx = b * OutArea + i0 * OutSize + i;
                        dBias[bidx] += Sigma[oidx];
                    }
                }
                //dBias[bidx] /= BatchCount;
            });

            dKernel = (Components.Real[])WeightKernel.Clone();
            Parallel(0, InputChannels, i0 =>
            {
                Parallel(0, OutputChannels, i1 =>
                {
                    Parallel(0, KernelLength, i2 =>
                    {
                        int t = (int)(i2 / KernelArea);
                        int s = i2 - t * (KernelArea);
                        int _s = s - KernelSize, _t = t - KernelSize;
                        int kidx = i0 * (OutputChannels * KernelLength) + i1 * KernelLength + i2;
                        dKernel[kidx] = 0;
                        for (int b = 0; b < BatchCount; b++)
                        {
                            for (int i = 0; i < OutSize; i++)
                            {
                                int oy = (int)(i / OutWidth);
                                int ox = (int)(i - oy * OutWidth);
                                int oidx = b * OutArea + i1 * OutSize + i;

                                int _ix = (int)(((float)ox * OutScale) + _s * KernelExpand);
                                int _iy = (int)(((float)oy * OutScale) + _t * KernelExpand);
                                if (_ix >= 0 && _iy >= 0 && _ix < InWidth && _iy < InHeight)
                                {
                                    int _idx = b * InArea + i0 * InSize + _iy * InWidth + _ix;
                                    dKernel[kidx] += Input[_idx] * Sigma[oidx];
                                }
                            }
                        }
                        //dKernel[kidx] /= BatchCount;
                    });
                });
            });

            Parallel(0, BatchCount, i0 =>
            {
                Parallel(0, InputChannels, i1 =>
                {
                    Parallel(0, InSize, i2 =>
                    {
                        int y = (int)(i2 / InWidth);
                        int x = i2 - y * InWidth;
                        int pidx = i0 * InArea + i1 * InSize + i2;
                        Propagator[pidx] = 0;
                        for (int k = 0; k < KernelLength; k++)
                        {
                            int t = k / KernelArea;
                            int s = k - t * KernelArea;
                            int ox = (int)(((float)x / OutScale) - KernelExpand * (s - KernelSize));
                            int oy = (int)(((float)y / OutScale) - KernelExpand * (t - KernelSize));
                            if (ox >= 0 && oy >= 0 && ox < OutWidth && oy < OutHeight)
                            {
                                for (int och = 0; och < OutputChannels; och++)
                                {
                                    int oidx = i0 * OutArea + och * OutSize + oy * OutWidth + ox;
                                    int kidx = i1 * (OutputChannels * KernelLength) + och * KernelLength + k;
                                    Propagator[pidx] += Sigma[oidx] * WeightKernel[kidx];
                                }
                            }
                        }
                    });
                });
            });
        }

        protected override void GpuFunction()
        {
            dBias = (Components.Real[])WeightBias.Clone();

            SwitchSellection(GpuSource[0].Name);
            using (var _sigma = ConvertBuffer(Sigma))
            using (var _dbias = ConvertBuffer(dBias))
            {
                SetParameter(_sigma);
                SetParameter(_dbias);

                SetParameter(BatchCount, ValueMode.INT);
                SetParameter(InWidth, ValueMode.INT);
                SetParameter(InHeight, ValueMode.INT);
                SetParameter(InputChannels, ValueMode.INT);
                SetParameter(OutScale, ValueMode.FLOAT);
                SetParameter(OutWidth, ValueMode.INT);
                SetParameter(OutHeight, ValueMode.INT);
                SetParameter(OutputChannels, ValueMode.INT);
                SetParameter(InSize, ValueMode.INT);
                SetParameter(InArea, ValueMode.INT);
                SetParameter(InTotal, ValueMode.INT);
                SetParameter(OutSize, ValueMode.INT);
                SetParameter(OutArea, ValueMode.INT);
                SetParameter(OutTotal, ValueMode.INT);
                SetParameter(KernelSize, ValueMode.INT);
                SetParameter(KernelArea, ValueMode.INT);
                SetParameter(KernelLength, ValueMode.INT);
                SetParameter(KernelExpand, ValueMode.INT);

                Execute(OutputChannels);
                ReadBuffer(_dbias, ref dBias);
            }

            dKernel = (Components.Real[])WeightKernel.Clone();
            SwitchSellection(GpuSource[1].Name);
            using (var _input = ConvertBuffer(Input))
            using (var _sigma = ConvertBuffer(Sigma))
            using (var _dkernel = ConvertBuffer(dKernel))
            {
                SetParameter(_input);
                SetParameter(_sigma);
                SetParameter(_dkernel);

                SetParameter(BatchCount, ValueMode.INT);
                SetParameter(InWidth, ValueMode.INT);
                SetParameter(InHeight, ValueMode.INT);
                SetParameter(InputChannels, ValueMode.INT);
                SetParameter(OutScale, ValueMode.FLOAT);
                SetParameter(OutWidth, ValueMode.INT);
                SetParameter(OutHeight, ValueMode.INT);
                SetParameter(OutputChannels, ValueMode.INT);
                SetParameter(InSize, ValueMode.INT);
                SetParameter(InArea, ValueMode.INT);
                SetParameter(InTotal, ValueMode.INT);
                SetParameter(OutSize, ValueMode.INT);
                SetParameter(OutArea, ValueMode.INT);
                SetParameter(OutTotal, ValueMode.INT);
                SetParameter(KernelSize, ValueMode.INT);
                SetParameter(KernelArea, ValueMode.INT);
                SetParameter(KernelLength, ValueMode.INT);
                SetParameter(KernelExpand, ValueMode.INT);

                Execute(InputChannels, OutputChannels, KernelLength);
                ReadBuffer(_dkernel, ref dKernel);
            }

            SwitchSellection(GpuSource[2].Name);
            using (var _sigma = ConvertBuffer(Sigma))
            using (var _kernel = ConvertBuffer(WeightKernel))
            using (var _propagator = ConvertBuffer(Propagator))
            {
                SetParameter(_sigma);
                SetParameter(_kernel);
                SetParameter(_propagator);

                SetParameter(BatchCount, ValueMode.INT);
                SetParameter(InWidth, ValueMode.INT);
                SetParameter(InHeight, ValueMode.INT);
                SetParameter(InputChannels, ValueMode.INT);
                SetParameter(OutScale, ValueMode.FLOAT);
                SetParameter(OutWidth, ValueMode.INT);
                SetParameter(OutHeight, ValueMode.INT);
                SetParameter(OutputChannels, ValueMode.INT);
                SetParameter(InSize, ValueMode.INT);
                SetParameter(InArea, ValueMode.INT);
                SetParameter(InTotal, ValueMode.INT);
                SetParameter(OutSize, ValueMode.INT);
                SetParameter(OutArea, ValueMode.INT);
                SetParameter(OutTotal, ValueMode.INT);
                SetParameter(KernelSize, ValueMode.INT);
                SetParameter(KernelArea, ValueMode.INT);
                SetParameter(KernelLength, ValueMode.INT);
                SetParameter(KernelExpand, ValueMode.INT);

                Execute(BatchCount, InputChannels, InSize);
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

            double ep = (OutScale == 0 ? 1 : 1.0 / OutScale) * ((double)KernelLength / (InputChannels * OutputChannels));
            Difference[0] = BiasOptimizer.Update(ref WeightBias, dBias, doUpdateCalculation, ep * rho);
            Difference[1] = KernelOptimizer.Update(ref WeightKernel, dKernel, doUpdateCalculation, ep * rho);
        }
    }
}

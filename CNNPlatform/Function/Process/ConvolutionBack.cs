using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace CNNPlatform.Function.Process
{
    class ConvolutionBack : Components.GPGPU.Function.ParameterizedFunctionBase
    {
        protected override void CreateGpuSource()
        {
            BiasOptimizer = new Optimizer.Adam();
            KernelOptimizer = new Optimizer.Adam();
        }

        Optimizer.OptimizerBase BiasOptimizer { get; set; }
        Optimizer.OptimizerBase KernelOptimizer { get; set; }

        #region 
        private int BatchCount;

        private int InWidth;
        private int InHeight;
        private int InputChannels;

        private double OutScale;
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

        private double rho;

        private Components.Real[] Input;
        private Components.Real[] Output;
        private Components.Real[] Sigma;
        private Components.Real[] Propagator;

        private Components.Real[] WeightKernel;
        private Components.Real[] WeightBias;
        private Components.Real[] dKernel;
        private Components.Real[] dBias;

        private Components.Real[] Difference;
        #endregion

        protected override void ConvertVariable(ComputeVariable _variable)
        {
            var variable = _variable as Variable.ConvolutionValiable;

            BatchCount = variable.BatchCount;

            InWidth = variable.InWidth;
            InHeight = variable.InHeight;
            InputChannels = variable.InputChannels;

            OutScale = variable.OutScale;
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

            rho = variable.Rho;

            Input = variable.Input.Data;
            Output = variable.Output.Data;
            Sigma = variable.Sigma.Data;
            Propagator = variable.Propagator.Data;
            WeightKernel = variable.WeightKernel.Data;
            WeightBias = variable.WeightBias.Data;

            Difference = variable.WeightDifference;
        }

        protected override void CpuFunction()
        {
            dBias = (Components.Real[])WeightBias.Clone();
            Parallel(0, InputChannels, i0 =>
            {
                Parallel(0, OutputChannels, i1 =>
                {
                    int bidx = i0 * OutputChannels + i1;
                    dBias[bidx] = 0;
                    for (int b = 0; b < BatchCount; b++)
                    {
                        for (int i = 0; i < OutSize; i++)
                        {
                            int oidx = b * OutArea + i1 * OutSize + i;
                            dBias[bidx] += Sigma[oidx];
                        }
                    }

                });
            });

            dKernel = (Components.Real[])WeightKernel.Clone();
            Parallel(0, InputChannels, i0 =>
            {
                Parallel(0, OutputChannels, i1 =>
                {
                    Parallel(0, KernelLength, i2 =>
                    {
                        int s = (int)(i2 / KernelArea);
                        int t = i2 - s * (KernelArea);
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
                    });
                });
            });

            Parallel(0, BatchCount, b =>
            {
                Parallel(0, InputChannels, ich =>
                {
                    Parallel(0, InSize, idx =>
                    {
                        int y = (int)(idx / InWidth);
                        int x = idx - y * InWidth;
                        int pidx = b * InArea + ich * InSize + idx;
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
                                    int oidx = b * OutArea + och * OutSize + oy * OutWidth + ox;
                                    int kidx = ich * (OutputChannels * KernelLength) + och * KernelLength + k;
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
            throw new NotImplementedException();
        }

        protected override bool UpdateConditionCheck()
        {
            return true;
        }

        public override void Update()
        {
            Difference[0] = BiasOptimizer.Update(ref WeightBias, dBias, (rho / (BatchCount)));
            Difference[1] = KernelOptimizer.Update(ref WeightKernel, dKernel, (rho / (BatchCount * KernelLength)));
        }
    }
}

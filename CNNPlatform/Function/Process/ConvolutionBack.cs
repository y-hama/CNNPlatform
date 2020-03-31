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
        }

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
        }

        protected override void CpuFunction()
        {
            dKernel = (Components.Real[])WeightKernel.Clone();
            Parallel(0, InputChannels, i0 =>
            {
                Parallel(0, OutputChannels, i1 =>
                {
                    Parallel(0, KernelLength, i2 =>
                    {
                        float _stride = 1.0f / (float)OutScale;
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

                                int _ix = (int)((float)ox * _stride) + _s * KernelExpand;
                                int _iy = (int)((float)oy * _stride) + _t * KernelExpand;
                                if (_ix >= 0 && _iy >= 0 && _ix < InWidth && _iy < InHeight)
                                {
                                    int _idx = b * InArea + i1 * InSize + _iy * InWidth + _ix;
                                    dKernel[kidx] += Input[_idx] * Sigma[oidx];
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
            for (int i = 0; i < dKernel.Length; i++)
            {
                WeightKernel[i] += -(rho / BatchCount) * dKernel[i];
            }
        }
    }
}

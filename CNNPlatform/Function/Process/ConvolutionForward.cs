using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace CNNPlatform.Function.Process
{
    class ConvolutionForward : Components.GPGPU.Function.FunctionBase
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

        private Components.Real[] Input;
        private Components.Real[] Output;

        private Components.Real[] WeightKernel;
        private Components.Real[] WeightBias;
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

            Input = variable.Input.Data;
            Output = variable.Output.Data;
            WeightKernel = variable.WeightKernel.Data;
            WeightBias = variable.WeightBias.Data;
        }

        protected override void CpuFunction()
        {

            Parallel(0, BatchCount, bcnt =>
            {
                Parallel(0, OutputChannels, och =>
                {
                    Parallel(0, OutSize, idx =>
                    {
                        float output = 0;
                        int y = (int)(idx / OutWidth);
                        int x = idx - y * OutWidth;
                        int otx = bcnt * OutArea + och * OutSize + idx;

                        double calccount = 0;
                        for (int k = 0; k < KernelLength; k++)
                        {
                            int t = k / KernelArea;
                            int s = k - t * KernelArea;
                            int ix = (int)(x / OutScale) + KernelExpand * (s - KernelSize);
                            int iy = (int)(y / OutScale) + KernelExpand * (t - KernelSize);
                            if (ix >= 0 && ix < InWidth && iy >= 0 && iy < InHeight)
                            {
                                calccount++;
                                for (int ich = 0; ich < InputChannels; ich++)
                                {
                                    int itx = bcnt * InArea + ich * InSize + iy * InWidth + ix;
                                    int ktx = ich * (OutputChannels * KernelLength) + och * KernelLength + k;
                                    int btx = ich * OutputChannels + och;
                                    output += Input[itx] * WeightKernel[ktx] + WeightBias[btx];
                                }
                            }
                        }
                        Output[otx] = output;
                        //if (calccount > 0) { Output[otx] /= calccount; } 
                    });
                });
            });
        }

        protected override void GpuFunction()
        {
        }
    }
}

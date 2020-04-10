using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace CNNPlatform.DedicatedFunction.Process
{
    class ConvolutionForward : Components.GPGPU.Function.FunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new Process.GpgpuSource.gp_convfowd());
        }

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

        private Components.Real[] Input;
        private Components.Real[] Output;

        private Components.Real[] WeightKernel;
        private Components.Real[] WeightBias;
        #endregion

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

            Input = variable.Input.Data;
            Output = variable.Output.Data;
            WeightKernel = variable.WeightKernel.Data;
            WeightBias = variable.WeightBias.Data;
        }

        protected override void CpuFunction()
        {
            Parallel(0, BatchCount, i0 =>
            {
                Parallel(0, OutputChannels, i1 =>
                {
                    Parallel(0, OutSize, i2 =>
                    {
                        float output = 0;
                        int y = (int)(i2 / OutWidth);
                        int x = i2 - y * OutWidth;
                        int otx = i0 * OutArea + i1 * OutSize + i2;

                        for (int k = 0; k < KernelLength; k++)
                        {
                            int t = k / KernelArea;
                            int s = k - t * KernelArea;
                            int ix = (int)(((float)x / OutScale) + KernelExpand * (s - KernelSize));
                            int iy = (int)(((float)y / OutScale) + KernelExpand * (t - KernelSize));
                            if (ix >= 0 && ix < InWidth && iy >= 0 && iy < InHeight)
                            {
                                for (int ich = 0; ich < InputChannels; ich++)
                                {
                                    int itx = i0 * InArea + ich * InSize + iy * InWidth + ix;
                                    int ktx = ich * (OutputChannels * KernelLength) + i1 * KernelLength + k;
                                    output += Input[itx] * WeightKernel[ktx];
                                }
                            }
                        }
                        Output[otx] = output + WeightBias[i1];
                    });
                });
            });
        }

        protected override void GpuFunction()
        {
            using (var _input = ConvertBuffer(Input))
            using (var _output = ConvertBuffer(Output))
            using (var _bias = ConvertBuffer(WeightBias))
            using (var _kernel = ConvertBuffer(WeightKernel))
            {
                SetParameter(_input);
                SetParameter(_output);
                SetParameter(_bias);
                SetParameter(_kernel);

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

                Execute(BatchCount, OutputChannels, OutSize);
                ReadBuffer(_output, ref Output);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace CNNPlatform.Function.Process
{
    class Convolution : Components.GPGPU.Function.FunctionBase
    {
        protected override void CreateGpuSource()
        {
        }

        private Components.Real[] input;
        private Components.Real[] output;

        public int width { get; set; }
        public int height { get; set; }

        public int ich { get; set; }
        public int och { get; set; }

        private int batch { get; set; }

        private Components.Real[] kernel;
        public int ksize { get; set; }
        public int karea { get; set; }
        public int klength { get; set; }

        private int TotalSize { get { return width * height; } }
        private int InTotalArea { get { return ich * TotalSize; } }
        private int InTotalLength { get { return batch * InTotalArea; } }
        private int OutTotalArea { get { return och * TotalSize; } }
        private int OutTotalLength { get { return batch * OutTotalArea; } }

        protected override void ConvertVariable(ComputeVariable _variable)
        {
            var variable = _variable as Variable.ConvolutionValiable;
            variable.Output = new Components.RNdMatrix(variable.BatchCount, variable.OutputChannels, variable.Width, variable.Height);

            width = variable.Width;
            height = variable.Height;
            ich = variable.InputChannels;
            och = variable.OutputChannels;
            batch = variable.BatchCount;

            input = variable.Input.Data;
            output = variable.Output.Data;

            kernel = variable.WeightKernel.Data;
            ksize = variable.KernekSize;
            karea = 2 * ksize + 1;
            klength = karea * karea;
        }

        protected override void CpuFunction()
        {
            Parallel(0, batch, b =>
            {
                Parallel(0, och, oc =>
                {
                    Parallel(0, TotalSize, p =>
                    {
                        int x = (int)(p / height);
                        int y = p - x * height;

                        int toidx = b * OutTotalArea + oc * TotalSize + x * height + y;
                        float opt = 0;

                        for (int k = 0; k < klength; k++)
                        {
                            int ks = (int)(k / karea);
                            int kt = k - ks * karea;
                            ks -= ksize; kt -= ksize;
                            int ix = x + ks;
                            int iy = y + kt;
                            if (ix >= 0 && ix < width && iy >= 0 && iy < height)
                            {
                                for (int ic = 0; ic < ich; ic++)
                                {
                                    opt += input[b * InTotalArea + ic * TotalSize + ix * height + iy] * kernel[ic * (och * klength) + oc * karea + k];
                                }
                            }
                        }

                        output[toidx] = opt;
                    });
                });
            });
        }

        protected override void GpuFunction()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CNNPlatform.Utility.Shared;

namespace CNNPlatform.Model
{
    class Model
    {
        private List<Layer.LayerBase> Layer { get; set; } = new List<CNNPlatform.Layer.LayerBase>();

        public Layer.LayerBase this[int idx]
        {
            get { return Layer[idx]; }
        }

        public int LayerCount { get { return Layer.Count; } }

        private int BatchCount { get; set; }
        private int InputWidth { get; set; }
        private int InputHeight { get; set; }
        private int InputChannels { get; set; }

        public Model(int batch, int inputWidth, int inputHeight, int inputChannels)
        {
            BatchCount = batch;
            InputWidth = inputWidth;
            InputHeight = inputHeight;
            InputChannels = inputChannels;
        }

        public void AddLayer(Layer.LayerBase layer)
        {
            Layer.Add(layer);
        }

        public void AddConvolution(ModelParameter instance, int outch, int kernelsize, int expand, double rho = 0.001, double scale = 1)
        {
            int inw = InputWidth, inh = InputHeight, inch = InputChannels;
            if (LayerCount != 0)
            {
                inw = Layer[LayerCount - 1].Variable.OutWidth;
                inh = Layer[LayerCount - 1].Variable.OutHeight;
                inch = Layer[LayerCount - 1].Variable.OutputChannels;
            }

            Layer.Add(new Layer.Convolution()
            {
                Variable = new Function.Variable.ConvolutionValiable()
                {
                    BatchCount = BatchCount,
                    InputChannels = inch,
                    InWidth = inw,
                    InHeight = inh,

                    OutputChannels = outch,
                    OutScale = scale,

                    KernelSize = kernelsize,
                    KernelExpand = expand,
                    Rho = rho,
                }.Confirm(instance),
            });
        }
    }
}

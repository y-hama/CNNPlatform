using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CNNPlatform.Utility.Shared;
using CNNPlatform.DedicatedFunction.Process.Optimizer;

namespace CNNPlatform.Model
{
    class Model
    {
        public int Generation { get; set; }
        public int Epoch { get; set; }
        public double Error { get; set; }

        private List<Layer.LayerBase> Layer { get; set; } = new List<CNNPlatform.Layer.LayerBase>();

        public Layer.LayerBase this[int idx]
        {
            get { return Layer[idx]; }
        }

        public int LayerCount { get { return Layer.Count; } }

        private ModelParameter Instance { get; set; }

        private int BatchCount { get; set; }
        private int InputWidth { get; set; }
        private int InputHeight { get; set; }
        private int InputChannels { get; set; }

        public Model(ModelParameter instance, int batch, int inputWidth, int inputHeight, int inputChannels)
        {
            Instance = instance;
            BatchCount = batch;
            InputWidth = inputWidth;
            InputHeight = inputHeight;
            InputChannels = inputChannels;
        }

        public void Save(string filename)
        {
            string text = "!>" + Generation + " " + Error + "\n";
            text += this.GetType().ToString() + "\n";
            foreach (var item in Layer)
            {
                text += item.Encode();
            }
            using (var fs = new System.IO.StreamWriter(filename, false))
            {
                fs.WriteLine(text);
            }
        }

        public void AddLayer(Layer.LayerBase layer)
        {
            Layer.Add(layer);
        }

        private void GetLayerInputInfomation(out int inw, out int inh, out int inch)
        {
            inw = InputWidth; inh = InputHeight; inch = InputChannels;
            if (LayerCount != 0)
            {
                inw = Layer[LayerCount - 1].Variable.OutWidth;
                inh = Layer[LayerCount - 1].Variable.OutHeight;
                inch = Layer[LayerCount - 1].Variable.OutputChannels;
            }
        }

        public void AddConvolution(int outch, int kernelsize, int expand,
            Utility.Types.Optimizer type = Utility.Types.Optimizer.Adam, double scale = 1, double rho = 0.0005)
        {
            int inw, inh, inch;
            GetLayerInputInfomation(out inw, out inh, out inch);

            Layer.Add(new Layer.Convolution()
            {
                Variable = new DedicatedFunction.Variable.ConvolutionVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = inch,
                    InWidth = inw,
                    InHeight = inh,

                    OutputChannels = outch,
                    OutScale = scale,

                    KernelSize = kernelsize,
                    KernelExpand = expand,
                    OptimizerType = type,
                    Rho = rho,
                }.Confirm(Instance),
            });
        }

        public void AddPooling(int compress, int expand)
        {
            int inw, inh, inch;
            GetLayerInputInfomation(out inw, out inh, out inch);

            Layer.Add(new Layer.Pooling()
            {
                Variable = new DedicatedFunction.Variable.PoolingVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = inch,
                    InWidth = inw,
                    InHeight = inh,

                    CompressSize = compress,
                    ExpandSize = expand,
                }.Confirm(Instance),
            });
        }

        public void AddActivation(Utility.Types.Activator type = Utility.Types.Activator.ReLU)
        {
            int inw, inh, inch;
            GetLayerInputInfomation(out inw, out inh, out inch);

            Layer.Add(new Layer.Activation()
            {
                Variable = new DedicatedFunction.Variable.ActivationVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = inch,
                    InWidth = inw,
                    InHeight = inh,

                    ActivationType = type,
                }.Confirm(Instance),
            });
        }
    }
}

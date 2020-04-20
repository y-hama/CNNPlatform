using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CNNPlatform.Utility.Shared;
using CNNPlatform.DedicatedFunction.Process.Optimizer;
using CNNPlatform.Layer;
using CNNPlatform.DedicatedFunction.Variable;

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

        internal LayerBase InputLayer { get { return Layer[0]; } }
        internal LayerBase OutputLayer { get { return Layer[LayerCount - 1]; } }

        public int LayerCount { get { return Layer.Count; } }

        private ModelParameter Instance { get; set; }

        private int BatchCount { get; set; }
        private int InputChannels { get; set; }
        private int InputWidth { get; set; }
        private int InputHeight { get; set; }

        public Model(ModelParameter instance, int batch, int inputChannels, int inputWidth, int inputHeight)
        {
            Instance = instance;
            BatchCount = batch;
            InputChannels = inputChannels;
            InputWidth = inputWidth;
            InputHeight = inputHeight;
        }

        public void Save(string filename)
        {
            string text = "!>" + Generation + " " + Error + "\n";
            text += this.GetType().ToString() + "\n";
            string[] tmp = new string[LayerCount];
            Components.GPGPU.Parallel.For(0, LayerCount, i =>
            {
                tmp[i] = Layer[i].Encode();
            });
            foreach (var item in tmp)
            {
                text += item;
            }
            using (var fs = new System.IO.StreamWriter(filename, false))
            {
                fs.WriteLine(text);
            }
        }

        public Model Clone()
        {
            var model = new Model(null, BatchCount, InputChannels, InputWidth, InputHeight)
            {
                Generation = Generation,
                Epoch = Epoch,
            };
            for (int i = 0; i < LayerCount; i++)
            {
                model.Layer.Add(Layer[i].Clone());
            }
            return model;
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

        private void AddLayer(Layer.LayerBase layer)
        {
            Layer.Add(layer);
            Console.WriteLine(string.Format("Layer({0,2}) : {1} -> {2}", LayerCount, layer.GetType().Name, layer.ParameterStatus));
        }

        #region CreateReverseLayer(private)
        private Layer.LayerBase ReverseConvolution(VariableBase _variable)
        {
            var variable = _variable as ConvolutionVariable;

            return new Layer.Convolution()
            {
                Direction = LayerBase.DirectionPattern.TurnBack,
                Variable = new DedicatedFunction.Variable.ConvolutionVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = variable.OutputChannels,
                    InWidth = variable.OutWidth,
                    InHeight = variable.OutHeight,

                    OutputChannels = variable.InputChannels,
                    OutScale = 1.0 / variable.OutScale,

                    KernelSize = variable.KernelSize,
                    KernelExpand = variable.KernelExpand,
                    OptimizerType = variable.OptimizerType,
                    Rho = variable.Rho,
                }.Confirm(Instance),
            };
        }

        private Layer.LayerBase ReversePooling(VariableBase _variable)
        {
            var variable = _variable as PoolingVariable;

            return new Layer.Pooling()
            {
                Direction = LayerBase.DirectionPattern.TurnBack,
                Variable = new DedicatedFunction.Variable.PoolingVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = variable.OutputChannels,
                    InWidth = variable.OutWidth,
                    InHeight = variable.OutHeight,

                    CompressSize = variable.ExpandSize,
                    ExpandSize = variable.CompressSize,
                }.Confirm(Instance),
            };
        }

        private Layer.LayerBase ReverseActivation(VariableBase _variable)
        {
            var variable = _variable as ActivationVariable;

            return new Layer.Activation()
            {
                Direction = LayerBase.DirectionPattern.TurnBack,
                Variable = new DedicatedFunction.Variable.ActivationVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = variable.OutputChannels,
                    InWidth = variable.OutWidth,
                    InHeight = variable.OutHeight,

                    ActivationType = variable.ActivationType,
                }.Confirm(Instance),
            };
        }

        private Layer.LayerBase ReverseAffine(VariableBase _variable)
        {
            var variable = _variable as AffineVariable;

            return new Layer.Affine()
            {
                Direction = LayerBase.DirectionPattern.TurnBack,
                Variable = new DedicatedFunction.Variable.AffineVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = variable.OutputChannels,
                    InWidth = variable.OutWidth,
                    InHeight = variable.OutHeight,

                    OutputChannels = variable.InputChannels,
                    OutWidth = variable.InWidth,
                    OutHeight = variable.InHeight,

                    OptimizerType = variable.OptimizerType,
                    Rho = variable.Rho,
                }.Confirm(Instance),
            };
        }
        #endregion

        #region AddLayer(Public)
        public void AddReverse()
        {
            var list = new List<Layer.LayerBase>();
            for (int i = LayerCount - 1; i >= 0; i--)
            {
                var type = Layer[i].GetType();
                if (Layer[i] is Layer.Convolution)
                {
                    list.Add(ReverseConvolution(Layer[i].Variable));
                }
                else if (Layer[i] is Layer.Pooling)
                {
                    list.Add(ReversePooling(Layer[i].Variable));
                }
                else if (Layer[i] is Layer.Activation)
                {
                    list.Add(ReverseActivation(Layer[i].Variable));
                }
                else if (Layer[i] is Layer.Affine)
                {
                    list.Add(ReverseAffine(Layer[i].Variable));
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                AddLayer(list[i]);
            }
        }

        public void AddConvolution(int outch, int kernelsize, int expand,
            Utility.Types.Optimizer type = Utility.Types.Optimizer.Adam, double scale = 1, double rho = 0.0005)
        {
            int inw, inh, inch;
            GetLayerInputInfomation(out inw, out inh, out inch);

            AddLayer(new Layer.Convolution()
            {
                Direction = LayerBase.DirectionPattern.Through,
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

            AddLayer(new Layer.Pooling()
            {
                Direction = LayerBase.DirectionPattern.Through,
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

            AddLayer(new Layer.Activation()
            {
                Direction = LayerBase.DirectionPattern.Through,
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

        public void AddAffine(int outcount, Utility.Types.Optimizer type = Utility.Types.Optimizer.Adam, double rho = 0.0005)
        {
            int inw, inh, inch;
            GetLayerInputInfomation(out inw, out inh, out inch);

            AddLayer(new Layer.Affine()
            {
                Direction = LayerBase.DirectionPattern.Through,
                Variable = new DedicatedFunction.Variable.AffineVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = inch,
                    InWidth = inw,
                    InHeight = inh,

                    OutputChannels = 1,
                    OutWidth = 1,
                    OutHeight = outcount,

                    OptimizerType = type,
                    Rho = rho,
                }.Confirm(Instance),
            });
        }
        #endregion

        #region Process
        private Components.RNdMatrix Teacher { get; set; } = new Components.RNdMatrix(new int[] { 0, 0, 0, 0 });
        public void Learning(Components.RNdMatrix input, Components.RNdMatrix teacher, bool isIteration)
        {
            if (isIteration)
            {
                DedicatedFunction.Process.Optimizer.OptimizerBase.Iteration++;
                Epoch = (int)DedicatedFunction.Process.Optimizer.OptimizerBase.Iteration;
            }
            Layer[0].Variable.Input = input;
            Teacher = teacher;
            #region LearningProcess
            for (int i = 0; i < LayerCount; i++)
            {
                this[i].ForwardFunction.Do(this[i].Variable);
                if (i < this.LayerCount - 1)
                {
                    (this[i].Variable as DedicatedFunction.Variable.VariableBase).Output.CopyTo((this[i + 1].Variable as DedicatedFunction.Variable.VariableBase).Input);
                }
            }
            Layer[LayerCount - 1].Variable.Sigma = Layer[LayerCount - 1].Variable.Output - teacher;
            for (int i = this.LayerCount - 1; i >= 0; i--)
            {
                this[i].BackFunction.Do(this[i].Variable);
                if (i > 0)
                {
                    (this[i].Variable as DedicatedFunction.Variable.VariableBase).Propagator.CopyTo((this[i - 1].Variable as DedicatedFunction.Variable.VariableBase).Sigma);
                }
            }
            Initializer.Generation++;
            this.Generation = Initializer.Generation;
            this.Error = Layer[LayerCount - 1].Variable.Error[0];
            #endregion
        }
        #endregion

        #region Visualize
        public Components.RNdMatrix ShowResult(int width, int height)
        {
            var viweset = new Components.RNdMatrix[]
            {
                Layer[0].Variable.Input,
                Teacher.AreaSize == 0 ? new Components.RNdMatrix(OutputLayer.Variable.Output.Shape) : Teacher,
                (Components.RNdMatrix)Layer[LayerCount - 1].Variable.Sigma.Abs(),
                Layer[LayerCount - 1].Variable.Output,
            };
            return Components.Imaging.View.ConvertToResultImage(viweset, width, height);
        }

        public Components.RNdMatrix ShowProcess(double scale = 1)
        {
            List<Components.RNdMatrix> source = new List<Components.RNdMatrix>();
            List<string> caption = new List<string>();
            for (int i = 0; i < LayerCount; i++)
            {
                caption.Add(Layer[i].Variable.GetCaption);
                source.Add(Layer[i].Variable.Output.Clone() as Components.RNdMatrix);
            }
            return Components.Imaging.View.ConvertToProcessImage(caption, source, scale);
        }
        #endregion
    }
}

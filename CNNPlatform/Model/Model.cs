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

        private Model(string modelbasefilename, int batch)
        {
            string readtoend = string.Empty;
            using (var fs = new System.IO.StreamReader(modelbasefilename))
            {
                readtoend = fs.ReadToEnd();
            }
            string[] basesplit = readtoend.Split(new string[] { "!!!!!!!!!!!!!!!>" }, StringSplitOptions.RemoveEmptyEntries);

            string modelparam = basesplit[0];
            #region ModelParmeter
            string[] mpsegment = modelparam.Split('\n');
            var modelname = mpsegment[0];
            var glparam = mpsegment[1];
            var sizeparam = mpsegment[2];
            int readtmp;
            #region globalparam
            var gppsplit = glparam.Split(' ');
            int.TryParse(gppsplit[0], out readtmp);
            Epoch = readtmp;
            int.TryParse(gppsplit[1], out readtmp);
            Generation = readtmp;
            #endregion
            #region sizeparam
            BatchCount = batch;
            var szsplit = sizeparam.Split(' ');
            int.TryParse(szsplit[0], out readtmp);
            InputChannels = readtmp;
            int.TryParse(szsplit[1], out readtmp);
            InputWidth = readtmp;
            int.TryParse(szsplit[2], out readtmp);
            InputHeight = readtmp;
            #endregion
            #endregion

            List<string> layerparams = new List<string>(basesplit[1].Split(new string[] { "!!!!!!!!!!>" }, StringSplitOptions.RemoveEmptyEntries));
            #region LayerParameter
            for (int i = 0; i < layerparams.Count; i++)
            {
                AddLayer(LayerBase.Decode(new System.IO.FileInfo(modelbasefilename).DirectoryName + @"\parameter", layerparams[i], Instance, batch));
            }
            #endregion
        }

        public void Save(string location)
        {
            #region ModelBaseFile
            string text = this.GetType().ToString() + "\n";
            text += Epoch + " " + Generation + " " + Error + "\n";
            text += InputChannels.ToString() + " " + InputWidth.ToString() + " " + InputHeight.ToString() + "\n";
            text += "!!!!!!!!!!!!!!!>";
            string[] tmp = new string[LayerCount];
            for (int i = 0; i < LayerCount; i++)
            {
                tmp[i] = Layer[i].Encode();
            }
            foreach (var item in tmp)
            {
                text += item;
            }
            using (var fs = new System.IO.StreamWriter(location + @"\" + "model" + ".mdl", false))
            {
                fs.WriteLine(text);
            }
            #endregion
            #region ModelParameterFile
            var ploc = new System.IO.DirectoryInfo(location + @"\parameter");
            ploc.Create();
            Components.GPGPU.Parallel.For(0, LayerCount, i => { Layer[i].Variable.SaveObject(ploc); });
            #endregion
        }

        public static Model Load(string location, int batchcount, int epoch = -1)
        {
            var loc = new System.IO.DirectoryInfo(location);
            if (!loc.Exists)
            {
                loc.Create(); return null;
            }
            else if (loc.GetDirectories().Length > 0)
            {
                int maxepo = 0;
                if (epoch < 0)
                {
                    #region
                    int tidx = 0;
                    foreach (var item in loc.GetDirectories())
                    {
                        if (int.TryParse(item.Name, out tidx))
                        {
                            if (maxepo < tidx)
                            {
                                maxepo = tidx;
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    if ((new List<System.IO.DirectoryInfo>(loc.GetDirectories()).Find(x => x.Name == epoch.ToString())) != null)
                    {
                        maxepo = epoch;
                    }
                    else { throw new Exception(); }
                }
                var tdinfo = new System.IO.DirectoryInfo(location + @"\" + maxepo.ToString());
                var mdlfile = new List<System.IO.FileInfo>(tdinfo.GetFiles()).Find(x => x.Extension == ".mdl");
                return new Model(mdlfile.FullName, batchcount);
            }
            return null;
        }

        public Model Clone()
        {
            var model = new Model(null, BatchCount, InputChannels, InputWidth, InputHeight)
            {
                Generation = Generation,
                Epoch = Epoch,
                Error = Error,
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

            return new Layer.Convolution(true)
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

            return new Layer.Pooling(true)
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

            return new Layer.Activation(true)
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

            return new Layer.Affine(true)
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

            AddLayer(new Layer.Convolution(true)
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

            AddLayer(new Layer.Pooling(true)
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

            AddLayer(new Layer.Activation(true)
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

            AddLayer(new Layer.Affine(true)
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
                Epoch++;
                DedicatedFunction.Process.Optimizer.OptimizerBase.Iteration = Epoch;
                Error = 0;
                foreach (var item in Layer)
                {
                    item.RefreshError();
                }
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
            this.Generation++;
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

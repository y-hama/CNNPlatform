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
        #region GeneralParameter
        public int Generation { get; set; }
        public int Epoch { get; set; }
        public double Error { get; set; }
        #endregion

        #region LayerParameter
        private List<Layer.LayerBase> Layer { get; set; } = new List<CNNPlatform.Layer.LayerBase>();

        public Layer.LayerBase this[int idx]
        {
            get { return Layer[idx]; }
        }

        private int BlockIndex { get; set; } = 0;
        public int ReverseStartBlock { get; private set; } = 0;
        public int StartBlock { get; set; } = -1;
        public int EndBlock { get; set; } = -1;
        internal int InputLayerIndex
        {
            get
            {
                int idx = 0;
                if (StartBlock >= 0)
                {
                    for (int i = 0; i < LayerCount; i++)
                    {
                        if (Layer[i].Block == StartBlock)
                        {
                            idx = i; break;
                        }
                    }
                }
                return idx;
            }
        }
        internal int OutputLayerIndex
        {
            get
            {
                int idx = LayerCount - 1;
                if (EndBlock >= 0)
                {
                    if (EndBlock >= 0)
                    {
                        for (int i = 0; i < LayerCount; i++)
                        {
                            if (Layer[i].Block == EndBlock + 1)
                            {
                                idx = i - 1; break;
                            }
                        }
                    }
                }
                return idx;
            }
        }

        internal LayerBase InputLayer { get { return Layer[InputLayerIndex]; } }
        internal LayerBase OutputLayer { get { return Layer[OutputLayerIndex]; } }

        public int LayerCount { get { return Layer.Count; } }
        #endregion

        private ModelParameter Instance { get; set; }

        private int BatchCount { get; set; }
        private int InputChannels { get; set; }
        private int InputWidth { get; set; }
        private int InputHeight { get; set; }

        #region Constructor/Clone
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
            Console.WriteLine("Epoch : {0}", Epoch);
            int.TryParse(gppsplit[1], out readtmp);
            Generation = readtmp;
            int.TryParse(gppsplit[2], out readtmp);
            ReverseStartBlock = readtmp;
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

        #endregion

        #region Save/Load
        public void Save(string location)
        {
            var mloc = new System.IO.DirectoryInfo(location);
            if (mloc.Exists) { mloc.Delete(true); }
            mloc.Create();
            #region ModelBaseFile Create
            string text = this.GetType().ToString() + "\n";
            text += Epoch + " " + Generation + " " + ReverseStartBlock + " " + Error + "\n";
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
            #endregion
            #region ModelParameterFile
            var ploc = new System.IO.DirectoryInfo(location + @"\parameter");
            ploc.Create();
            Tasks.ForParallel(0, LayerCount, i => { Layer[i].Variable.SaveObject(ploc); });
            #endregion
            #region ModelBaseFile Save
            using (var fs = new System.IO.StreamWriter(location + @"\" + "model" + ".mdl", false))
            {
                fs.WriteLine(text);
            }
            #endregion
        }

        public void SaveTemporary(string location)
        {
            var mloc = new System.IO.DirectoryInfo(location);
            if (mloc.Exists) { mloc.Delete(true); }
            mloc.Create();

            var container = new Components.Locker.TagFileController("model");
            var generaltag = container.Root.AddTag("general");
            generaltag.AddValue("Epoch", Epoch);
            generaltag.AddValue("Generation", Generation);
            generaltag.AddValue("ReverseStartBlock", ReverseStartBlock);
            generaltag.AddValue("Error", Error);

            var sizetag = container.Root.AddTag("size");
            sizetag.AddValue("InputChannels", InputChannels);
            sizetag.AddValue("InputWidth", InputWidth);
            sizetag.AddValue("InputHeight", InputHeight);

            var layertag = container.Root.AddTag("layer");
            foreach (var item in Layer)
            {
                item.Encode(ref layertag);
            }

            #region ModelParameterFile
            var ploc = new System.IO.DirectoryInfo(location + @"\parameter");
            ploc.Create();
            Tasks.ForParallel(0, LayerCount, i => { Layer[i].Variable.SaveObject(ploc); });
            #endregion

            container.Save(location + @"\" + "model" + ".mdl");

            Console.WriteLine(container.Tree);
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
                Model model = null;

                while (model == null)
                {
                    try
                    {
                        model = new Model(mdlfile.FullName, batchcount);
                    }
                    catch (Exception)
                    {
                        model = null;
                        if (maxepo > 0)
                        {
                            Console.WriteLine("ModelLoad Error. Try PrevEpoch");
                            maxepo--;
                            tdinfo = new System.IO.DirectoryInfo(location + @"\" + maxepo.ToString());
                            if (tdinfo.Exists)
                            {
                                mdlfile = new List<System.IO.FileInfo>(tdinfo.GetFiles()).Find(x => x.Extension == ".mdl");
                            }
                            else { throw new Exception(); }
                        }
                        else { throw new Exception(); }
                    }
                }
                return model;
            }
            return null;
        }
        #endregion

        #region RequestController
        public void UpdateState(bool request, int targetlayer = -1)
        {
            if (targetlayer < 0)
            {
                foreach (var item in Layer)
                {
                    item.Variable.UpdateRequest = request;
                }
            }
            else
            {
                if (targetlayer < LayerCount)
                {
                    Layer[targetlayer].Variable.UpdateRequest = request;
                }
            }
        }
        #endregion

        #region CreateReverseLayer(private)
        private Layer.LayerBase ReverseConvolution(VariableBase _variable)
        {
            var variable = _variable as ConvolutionVariable;

            return new Layer.Convolution(true)
            {
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
            }.Confirm();
        }

        private Layer.LayerBase ReversePooling(VariableBase _variable)
        {
            var variable = _variable as PoolingVariable;

            return new Layer.Pooling(true)
            {
                Variable = new DedicatedFunction.Variable.PoolingVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = variable.OutputChannels,
                    InWidth = variable.OutWidth,
                    InHeight = variable.OutHeight,

                    CompressSize = variable.ExpandSize,
                    ExpandSize = variable.CompressSize,
                }.Confirm(Instance),
            }.Confirm();
        }

        private Layer.LayerBase ReverseActivation(VariableBase _variable)
        {
            var variable = _variable as ActivationVariable;

            return new Layer.Activation(true)
            {
                Variable = new DedicatedFunction.Variable.ActivationVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = variable.OutputChannels,
                    InWidth = variable.OutWidth,
                    InHeight = variable.OutHeight,

                    ActivationType = variable.ActivationType,
                }.Confirm(Instance),
            }.Confirm();
        }

        private Layer.LayerBase ReverseAffine(VariableBase _variable)
        {
            var variable = _variable as AffineVariable;

            return new Layer.Affine(true)
            {
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
            }.Confirm();
        }

        private Layer.LayerBase ReverseReshape(VariableBase _variable)
        {
            var variable = _variable as ReshapeVariable;

            return new Layer.Reshape(true)
            {
                Variable = new DedicatedFunction.Variable.ReshapeVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = variable.OutputChannels,
                    InWidth = variable.OutWidth,
                    InHeight = variable.OutHeight,

                    OutputChannels = variable.InputChannels,
                    OutWidth = variable.InWidth,
                    OutHeight = variable.InHeight,
                }.Confirm(Instance),
            }.Confirm();
        }
        #endregion

        #region AddLayer(Public)
        public void SetNewBlock()
        {
            BlockIndex++;
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
            if (layer.Block < 0)
            {
                layer.Block = BlockIndex;
            }
            Layer.Add(layer);
            Console.WriteLine(string.Format("Layer({0,2}) block{1,2} : {2} -> {3}", LayerCount, layer.Block, layer.GetType().Name, layer.ParameterStatus));
        }

        public void AddReverse()
        {
            var list = new List<Layer.LayerBase>();
            for (int i = LayerCount - 1; i >= 0; i--)
            {
                var type = Layer[i].GetType();
                LayerBase revlayer = null;
                if (Layer[i] is Layer.Convolution)
                {
                    revlayer = (ReverseConvolution(Layer[i].Variable));
                }
                else if (Layer[i] is Layer.Pooling)
                {
                    revlayer = (ReversePooling(Layer[i].Variable));
                }
                else if (Layer[i] is Layer.Activation)
                {
                    revlayer = (ReverseActivation(Layer[i].Variable));
                }
                else if (Layer[i] is Layer.Affine)
                {
                    revlayer = (ReverseAffine(Layer[i].Variable));
                }
                else if (Layer[i] is Layer.Reshape)
                {
                    revlayer = (ReverseReshape(Layer[i].Variable));
                }
                list.Add(revlayer);
            }

            SetNewBlock();
            ReverseStartBlock = BlockIndex;
            int tmpblk = Layer[LayerCount - 1].Block;
            for (int i = 0; i < list.Count; i++)
            {
                int reverse = list.Count - (i + 1);
                if (tmpblk != Layer[reverse].Block) { SetNewBlock(); }
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
            }.Confirm());
        }

        public void AddPooling(int compress, int expand)
        {
            int inw, inh, inch;
            GetLayerInputInfomation(out inw, out inh, out inch);

            AddLayer(new Layer.Pooling(true)
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
            }.Confirm());
        }

        public void AddActivation(Utility.Types.Activator type = Utility.Types.Activator.ReLU)
        {
            int inw, inh, inch;
            GetLayerInputInfomation(out inw, out inh, out inch);

            AddLayer(new Layer.Activation(true)
            {
                Variable = new DedicatedFunction.Variable.ActivationVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = inch,
                    InWidth = inw,
                    InHeight = inh,

                    ActivationType = type,
                }.Confirm(Instance),
            }.Confirm());
        }

        public void AddAffine(int outcount, Utility.Types.Optimizer type = Utility.Types.Optimizer.Adam, double rho = 0.0005)
        {
            int inw, inh, inch;
            GetLayerInputInfomation(out inw, out inh, out inch);

            AddLayer(new Layer.Affine(true)
            {
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
            }.Confirm());
        }

        public void AddReshape(int outw, int outh)
        {
            int inw, inh, inch;
            GetLayerInputInfomation(out inw, out inh, out inch);

            int outch = (inw * inh * inch) / (outw * outh);
            if (((inw * inh * inch) - (outw * outh * outch) != 0))
            {
                throw new Exception();
            }

            AddLayer(new Layer.Reshape(true)
            {
                Variable = new DedicatedFunction.Variable.ReshapeVariable()
                {
                    BatchCount = BatchCount,
                    InputChannels = inch,
                    InWidth = inw,
                    InHeight = inh,

                    OutputChannels = outch,
                    OutWidth = outw,
                    OutHeight = outh,
                }.Confirm(Instance),
            }.Confirm());

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
            InputLayer.Variable.Input = input;
            Teacher = teacher;
            #region LearningProcess
            for (int i = InputLayerIndex; i <= OutputLayerIndex; i++)
            {
                this[i].ForwardFunction.Do(this[i].Variable);
                if (i < this.LayerCount - 1)
                {
                    (this[i].Variable as DedicatedFunction.Variable.VariableBase).Output.CopyTo((this[i + 1].Variable as DedicatedFunction.Variable.VariableBase).Input);
                }
            }
            OutputLayer.Variable.Sigma = OutputLayer.Variable.Output - teacher;
            for (int i = OutputLayerIndex; i >= InputLayerIndex; i--)
            {
                this[i].BackFunction.Do(this[i].Variable);
                if (i > 0)
                {
                    (this[i].Variable as DedicatedFunction.Variable.VariableBase).Propagator.CopyTo((this[i - 1].Variable as DedicatedFunction.Variable.VariableBase).Sigma);
                }
            }
            this.Generation++;
            this.Error = OutputLayer.Variable.Error[0];
            #endregion
        }

        public void Inference(Components.RNdMatrix input, out Components.RNdMatrix output)
        {
            InputLayer.Variable.Input = input;
            for (int i = InputLayerIndex; i < LayerCount; i++)
            {
                this[i].ForwardFunction.Do(this[i].Variable);
                if (i < this.LayerCount - 1)
                {
                    (this[i].Variable as DedicatedFunction.Variable.VariableBase).Output.CopyTo((this[i + 1].Variable as DedicatedFunction.Variable.VariableBase).Input);
                }
            }
            output = OutputLayer.Variable.Output.Clone() as Components.RNdMatrix;
        }
        #endregion

        #region Visualize
        public Components.RNdMatrix ShowResult(int width, int height)
        {
            var viweset = new Components.RNdMatrix[]
            {
                InputLayer.Variable.Input,
                Teacher.AreaSize == 0 ? new Components.RNdMatrix(OutputLayer.Variable.Output.Shape) : Teacher,
                (Components.RNdMatrix)OutputLayer.Variable.Sigma.Abs(),
                OutputLayer.Variable.Output,
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

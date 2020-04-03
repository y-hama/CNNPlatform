using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Components.Locker;
using CNNPlatform.Utility.Shared;

namespace CNNPlatform
{
    class TheradLearning
    {
        private TheradLearning() { }
        public static TheradLearning Core { get; private set; } = new TheradLearning();

        public int BatchCount { get; set; } = 1;

        #region InnerClass       
        private class BufferingData
        {
            private BufferingData() { }
            public static BufferingData Instance { get; } = new BufferingData();

            public System.Threading.CountdownEvent Signal { get; set; } = new System.Threading.CountdownEvent(1);

            public double Error { get; set; }
            public List<ModelParameter.WeightData> Weignt { get; set; } = new List<ModelParameter.WeightData>();
        }

        private class ImageData
        {
            private ImageData() { }
            public static ImageData Instance { get; } = new ImageData();
            public System.Threading.CountdownEvent LoadSignal { get; set; } = new System.Threading.CountdownEvent(1);
            public System.Threading.CountdownEvent ResetSignal { get; set; } = new System.Threading.CountdownEvent(1);

            public Components.RNdMatrix Input { get; set; }
            public Components.RNdMatrix Teacher { get; set; }
        }
        #endregion

        public void Start()
        {
            // Learningを実施する
            // InferenceとはLockServerとして動作する
            // ControllerとはLockServerとして動作する
            // GPUを使用する

            // GPU初期化
            Initializer.Startup();

            // LockServer構築
            var instance = (ModelParameter)ObjectLocker.CreateServer(ModelParameter.ChannelName, ModelParameter.ObjectName, typeof(ModelParameter));
            var processParameter = (ProcessParameter)ObjectLocker.CreateServer(ProcessParameter.ChannelName, ProcessParameter.ObjectName, typeof(ProcessParameter));

            // Model生成
            Model.Creater.Core.Instance = instance;
            var model = Model.Creater.Core.TestModel(BatchCount);
            var inputvariavble = model[0].Variable as Function.Variable.VariableBase;
            var outputvariavble = model[model.LayerCount - 1].Variable as Function.Variable.VariableBase;
            using (instance.Lock(Components.Locker.Priority.Critical))
            {
                BufferingData.Instance.Weignt = new List<ModelParameter.WeightData>(instance.Weignt);
                instance.Initialized = true;
            }

            #region LockProcess
            Components.RNdMatrix result = null;
            new Task(() =>
            {
                while (!Initializer.Terminate)
                {
                    using (processParameter.Lock())
                    {
                        Initializer.Terminate = processParameter.ExitApplication;
                        Console.WriteLine(Initializer.Terminate);
                        if (result != null)
                        {
                            processParameter.Result = result.Clone() as Components.RNdMatrix;
                            //Components.Imaging.View.Show(processParameter.Result, "sample");
                        }
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            }).Start();
            new Task(() =>
            {
                while (!Initializer.Terminate)
                {
                    BufferingData.Instance.Signal.Wait();
                    using (instance.Lock())
                    {
                        instance.Generation = Initializer.Generatiion;
                        instance.Error = BufferingData.Instance.Error;
                        instance.Weignt = new List<ModelParameter.WeightData>(BufferingData.Instance.Weignt);
                    }
                    BufferingData.Instance.Signal.Reset();
                }
            }).Start();
            #endregion

            #region SourceLoadProcess
            Components.Imaging.FileLoader.Instance.SetLocation(new System.IO.DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"/sample/"));
            new Task(() =>
            {
                Components.RNdMatrix i, t;
                while (!Initializer.Terminate)
                {
                    ImageData.Instance.LoadSignal.Reset();
                    ImageData.Instance.ResetSignal.Reset();
                    Components.Imaging.FileLoader.Instance.LoadImage(
                        inputvariavble.InWidth, inputvariavble.InHeight, outputvariavble.OutWidth, outputvariavble.OutHeight, inputvariavble.BatchCount,
                        inputvariavble.InputChannels, outputvariavble.OutputChannels, out i, out t);
                    ImageData.Instance.Input = i;
                    ImageData.Instance.Teacher = t;
                    ImageData.Instance.LoadSignal.Signal();
                    ImageData.Instance.ResetSignal.Wait();
                }
            }).Start();
            #endregion

            #region LearningProcess
            Components.RNdMatrix teacher = new Components.RNdMatrix(outputvariavble.Output.Shape);
            while (!Initializer.Terminate)
            {
                ImageData.Instance.LoadSignal.Wait();
                inputvariavble.Input.Data = ImageData.Instance.Input.Data.Clone() as Components.Real[];
                teacher.Data = ImageData.Instance.Teacher.Data.Clone() as Components.Real[];
                ImageData.Instance.ResetSignal.Signal();

                #region LearningProcess
                for (int i = 0; i < model.LayerCount; i++)
                {
                    model[i].ForwardFunction.Do(model[i].Variable);
                    if (i < model.LayerCount - 1)
                    {
                        (model[i + 1].Variable as Function.Variable.VariableBase).Input = (model[i].Variable as Function.Variable.VariableBase).Output;
                    }
                }
                outputvariavble.Sigma = outputvariavble.Output - teacher;
                for (int i = model.LayerCount - 1; i >= 0; i--)
                {
                    model[i].BackFunction.Do(model[i].Variable);
                    if (i > 0)
                    {
                        (model[i - 1].Variable as Function.Variable.VariableBase).Sigma = (model[i].Variable as Function.Variable.VariableBase).Propagator;
                    }
                }
                var error = 0.0;
                for (int i = 0; i < outputvariavble.Sigma.Data.Length; i++)
                {
                    error += Math.Abs(outputvariavble.Sigma.Data[i]);
                }
                error /= outputvariavble.Sigma.Length;
                Initializer.Generatiion++;
                #endregion

                var viweset = new Components.RNdMatrix[] { inputvariavble.Input, teacher, (Components.RNdMatrix)outputvariavble.Sigma.Abs(), outputvariavble.Output };
                result = Components.Imaging.View.ConvertToShow(viweset, inputvariavble.InWidth, inputvariavble.InHeight);

                #region Overwrite Signal
                if (BufferingData.Instance.Signal.CurrentCount == BufferingData.Instance.Signal.InitialCount)
                {
                    BufferingData.Instance.Error = error;
                    for (int i = 0; i < model.LayerCount; i++)
                    {
                        var _weight = (object)BufferingData.Instance.Weignt[i];
                        model[i].Variable.OverwriteParameter(ref _weight);
                    }
                    BufferingData.Instance.Signal.Signal();
                }
                #endregion

                //Console.WriteLine(@"gen/" + Initializer.Generatiion + @" err/" + error);
                GC.Collect();
            }
            #endregion
        }
    }
}

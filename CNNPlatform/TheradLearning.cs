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

        public int BatchCount { get; set; } = 4;

        #region InnerClass       
        private class BufferingData
        {
            private BufferingData() { }
            public static BufferingData Instance { get; } = new BufferingData();

            public System.Threading.CountdownEvent Signal { get; set; } = new System.Threading.CountdownEvent(1);
            public System.Threading.CountdownEvent UpdateSignal { get; set; } = new System.Threading.CountdownEvent(1);

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
            var model = Model.Creater.Core.TestModel();
            var inputvariavble = model[0].Variable as DedicatedFunction.Variable.VariableBase;
            var outputvariavble = model[model.LayerCount - 1].Variable as DedicatedFunction.Variable.VariableBase;
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
                    BufferingData.Instance.UpdateSignal.Reset();
                    if (result != null)
                    {
                        using (processParameter.Lock())
                        {
                            Initializer.Terminate = processParameter.ExitApplication;
                            //Console.WriteLine(Initializer.Terminate);
                            if (processParameter.Result == null) { processParameter.Result = new Components.RNdMatrix(result.Shape); }
                            result.CopyTo(processParameter.Result);
                        }
                        Components.Imaging.View.Show(result, "sample");
                    }
                    BufferingData.Instance.UpdateSignal.Wait();
                }
            }).Start();
            new Task(() =>
            {
                while (!Initializer.Terminate)
                {
                    BufferingData.Instance.Signal.Wait();
                    using (instance.Lock())
                    {
                        instance.Generation = Initializer.Generation;
                        instance.Error = BufferingData.Instance.Error;
                        instance.Weignt = new List<ModelParameter.WeightData>(BufferingData.Instance.Weignt);
                    }
                    Console.WriteLine(model.Epoch + " / " + model.Generation + " / " + BufferingData.Instance.Error);
                    BufferingData.Instance.Signal.Reset();
                    BufferingData.Instance.UpdateSignal.Signal();
                }
            }).Start();
            #endregion

            bool iteration = false;
            Components.Imaging.FileLoader.Instance.SetSourceLocation(new System.IO.DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"/sample/"));
            Components.Imaging.FileLoader.Instance.SetResultLocation(new System.IO.DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"/result/"));
            #region SourceLoadProcess
            new Task(() =>
            {
                Components.RNdMatrix i, t;
                while (!Initializer.Terminate)
                {
                    ImageData.Instance.LoadSignal.Reset();
                    ImageData.Instance.ResetSignal.Reset();
                    iteration = Components.Imaging.FileLoader.Instance.LoadImage(
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
            Components.RNdMatrix input = new Components.RNdMatrix(inputvariavble.Input.Shape);
            Components.RNdMatrix teacher = new Components.RNdMatrix(outputvariavble.Output.Shape);
            while (!Initializer.Terminate)
            {
                ImageData.Instance.LoadSignal.Wait();
                input.Data = ImageData.Instance.Input.Data.Clone() as Components.Real[];
                teacher.Data = ImageData.Instance.Teacher.Data.Clone() as Components.Real[];
                ImageData.Instance.ResetSignal.Signal();

                model.Learning(input, teacher, iteration);
                result = model.ShowProcess(1.5);

                #region Overwrite Signal
                if (BufferingData.Instance.Signal.CurrentCount == BufferingData.Instance.Signal.InitialCount)
                {
                    BufferingData.Instance.Error = outputvariavble.Error[1];
                    for (int i = 0; i < model.LayerCount; i++)
                    {
                        var _weight = (object)BufferingData.Instance.Weignt[i];
                        model[i].Variable.OverwriteParameter(ref _weight);
                    }
                    BufferingData.Instance.Signal.Signal();
                }
                #endregion

                //model.Save("test.mdl");
                //Console.WriteLine(@"gen/" + Initializer.Generatiion + @" err/" + error);
                GC.Collect();
            }
            #endregion
        }
    }
}

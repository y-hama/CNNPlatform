using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CNNPlatform
{
    class LearningProcess
    {
        private LearningProcess() { }
        public static LearningProcess Core { get; private set; } = new LearningProcess();

        public int BatchCount { get; set; } = 1;

        private class BufferingData
        {
            private BufferingData() { }
            public static BufferingData Instance { get; } = new BufferingData();

            public System.Threading.CountdownEvent Signal { get; set; } = new System.Threading.CountdownEvent(1);

            public double Error { get; set; }
            public List<Utility.Shared.ModelParameter.WeightData> Weignt { get; set; } = new List<Utility.Shared.ModelParameter.WeightData>();
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

        public void Start()
        {
            // Learningを実施する
            // LockServerとして動作する
            // GPUを使用する

            Initializer.Startup();

            Console.WriteLine(BatchCount);
            var instance = (Utility.Shared.ModelParameter)Components.Locker.ObjectLocker.CreateServer(Utility.Shared.ModelParameter.ChannelName, Utility.Shared.ModelParameter.ObjectName, typeof(Utility.Shared.ModelParameter));
            Model.Creater.Core.Instance = instance;
            var model = Model.Creater.Core.TestModel(BatchCount);

            var inputvariavble = model[0].Variable as Function.Variable.VariableBase;
            var outputvariavble = model[model.LayerCount - 1].Variable as Function.Variable.VariableBase;

            using (instance.Lock())
            {
                instance.Initialized = true;
                BufferingData.Instance.Weignt = new List<Utility.Shared.ModelParameter.WeightData>(instance.Weignt);
            }

            #region OverwriteProcess
            new Task(() =>
            {
                while (!Initializer.Terminate)
                {
                    BufferingData.Instance.Signal.Wait();
                    using (instance.Lock())
                    {
                        //Initializer.Terminate = instance.ExitApplication;
                        instance.Generation = Initializer.Generatiion;
                        instance.Error = BufferingData.Instance.Error;
                        instance.Weignt = new List<Utility.Shared.ModelParameter.WeightData>(BufferingData.Instance.Weignt);
                    }
                    BufferingData.Instance.Signal.Reset();
                }
            })
#if true
            .Start()
#endif
            ;
            #endregion

            #region ImageLoadProcess
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
            })
#if true
            .Start()
#endif
            ;
            #endregion

            Components.Imaging.FileLoader.Instance.SetLocation(new System.IO.DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"/sample/"));
            inputvariavble.Input = new Components.RNdMatrix(inputvariavble.Propagator.Shape);
            Components.RNdMatrix teacher = new Components.RNdMatrix(outputvariavble.Output.Shape);
            //new Task(() =>
            {
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

                    Components.Imaging.View.Show(
                        new Components.RNdMatrix[] { inputvariavble.Input, teacher, (Components.RNdMatrix)outputvariavble.Sigma.Abs(), outputvariavble.Output },
                        "learning...");

                    #region Overwrite Signal
#if true
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
#else
                    using (instance.Lock())
                    {
                        Initializer.Terminate = instance.ExitApplication;
                        instance.Generation = Initializer.Generatiion;
                        instance.Error = error;
                        for (int i = 0; i < model.LayerCount; i++)
                        {
                            var _weight = (object)instance.Weignt[i];
                            model[i].Variable.OverwriteParameter(ref _weight);
                        }
                    }
#endif
                    #endregion

                    GC.Collect();
                }
            }
            //).Start();
        }
    }
}

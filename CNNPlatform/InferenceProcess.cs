using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform
{
    public class InferenceProcess
    {
        public static int ModelGeneration { get; private set; }

        public static System.Threading.CountdownEvent TerminatedSignal { get; private set; } = new System.Threading.CountdownEvent(1);

        public static double LearningError { get; private set; }
        public static List<Components.Real[]> Difference { get; set; } = new List<Components.Real[]>();

        public static Components.RNdMatrix Result { get; private set; }

        private static bool running { get; set; } = false;
        private static bool terminate { get; set; } = false;
        public static bool Terminate
        {
            get { return terminate; }
            set
            {
                terminate = value;
                if (running && terminate)
                {
                    TerminatedSignal.Wait();
                }
            }
        }
        private InferenceProcess() { }
        public static InferenceProcess Core { get; private set; } = new InferenceProcess();

        public void Start()
        {
            // Inferenceを実施する
            // LockClientとして動作する
            // GPUを使用しない

            var instance = Components.Locker.ObjectLocker.CreateClient(CNNPlatform.Utility.Shared.ModelParameter.ChannelName, CNNPlatform.Utility.Shared.ModelParameter.ObjectName) as CNNPlatform.Utility.Shared.ModelParameter;
            bool check = false;
            while (!check)
            {
                System.Threading.Thread.Sleep(1000);
                try
                {
                    using (instance.Lock(Components.Locker.Priority.Critical))
                    {
                        check = instance.Initialized;
                    }
                }
                catch (Exception) { }
            }

            ModelGeneration = 0;
            //new Task(() =>
            {
                running = true;
                var model = Model.Creater.Core.TestModel();
                List<Utility.Shared.ModelParameter.WeightData> weight;
                using (instance.Lock())
                {
                    ModelGeneration = instance.Generation;
                    weight = new List<Utility.Shared.ModelParameter.WeightData>(instance.Weignt);
                }
                for (int i = 0; i < model.LayerCount; i++)
                {
                    model[i].Variable.UpdateParameter(weight[i]);
                    Difference.Add(weight[i].Difference);
                }

                var inputvariavble = model[0].Variable as Function.Variable.ConvolutionValiable;
                var outputvariavble = model[model.LayerCount - 1].Variable as Function.Variable.ConvolutionValiable;
                while (!Terminate)
                {
                    #region ReadWeight 
                    using (var key = instance.LockThrow())
                    {
                        if (key != null)
                        {
                            ModelGeneration = instance.Generation;
                            LearningError = instance.Error;
                            weight = new List<Utility.Shared.ModelParameter.WeightData>(instance.Weignt);
                        }
                    }
                    for (int i = 0; i < model.LayerCount; i++)
                    {
                        model[i].Variable.UpdateParameter(weight[i]);
                        Difference[i] = weight[i].Difference;
                    }
                    #endregion
                    #region InferenceProcess
                    Components.Imaging.Camera.Instance.GetFrame(
                        inputvariavble.InWidth, inputvariavble.InHeight, inputvariavble.BatchCount, inputvariavble.InputChannels,
                        out inputvariavble.Input);

                    for (int i = 0; i < model.LayerCount; i++)
                    {
                        model[i].ForwardFunction.Do(model[i].Variable);
                        if (i < model.LayerCount - 1)
                        {
                            (model[i + 1].Variable as Function.Variable.VariableBase).Input = (model[i].Variable as Function.Variable.VariableBase).Output;
                        }
                    }

                    #endregion
                    Components.Imaging.View.Show(outputvariavble.Output, "inference");
                    Result = outputvariavble.Output.Clone() as Components.RNdMatrix;
                }
                using (var key = instance.Lock(Components.Locker.Priority.Critical))
                {
                    //instance.ExitApplication = true;
                }
                TerminatedSignal.Signal();
            }
            //).Start();
        }
    }
}

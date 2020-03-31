using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform
{
    public class InferenceProcess
    {
        public static System.Threading.CountdownEvent TerminatedSignal { get; private set; } = new System.Threading.CountdownEvent(1);

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

        public void Start(Components.Locker.ObjectLocker.Exclusive _instance)
        {
            var gen = 0;
            var instance = (SharedObject)_instance;
            new Task(() =>
            {
                running = true;
                var model = Model.Creater.Core.TestModel();
                List<SharedObject.WeigntData> weight;
                using (instance.Lock())
                {
                    gen = instance.Generation;
                    weight = new List<SharedObject.WeigntData>(instance.Weignt);
                }
                for (int i = 0; i < model.Layer.Count; i++)
                {
                    model.Layer[i].Variable.UpdateParameter(weight[i]);
                }

                var inputvariavble = model.Layer[0].Variable as Function.Variable.ConvolutionValiable;
                var outputvariavble = model.Layer[model.Layer.Count - 1].Variable as Function.Variable.ConvolutionValiable;
                while (!Terminate)
                {
                    #region ReadWeight
                    using (var key = instance.LockThrow())
                    {
                        if (key != null)
                        {
                            gen = instance.Generation;
                            weight = new List<SharedObject.WeigntData>(instance.Weignt);
                        }
                    }
                    for (int i = 0; i < model.Layer.Count; i++)
                    {
                        model.Layer[i].Variable.UpdateParameter(weight[i]);
                    }
                    #endregion
                    #region InferenceProcess
                    Components.Imaging.Camera.Instance.GetFrame(
                        inputvariavble.InWidth, inputvariavble.InHeight, inputvariavble.BatchCount, inputvariavble.InputChannels,
                        out inputvariavble.Input);

                    for (int i = 0; i < model.Layer.Count; i++)
                    {
                        model.Layer[i].ForwardFunction.Do(model.Layer[i].Variable);
                        if (i < model.Layer.Count - 1)
                        {
                            (model.Layer[i + 1].Variable as Function.Variable.VariableBase).Input = (model.Layer[i].Variable as Function.Variable.VariableBase).Output;
                        }
                    }

                    Components.Imaging.View.Show(outputvariavble.Output, "inference");
                    #endregion
                }
                using (var key = instance.Lock(Components.Locker.Priority.Critical))
                {
                    instance.ExitApplication = true;
                }
                TerminatedSignal.Signal();
            }).Start();
        }
    }
}

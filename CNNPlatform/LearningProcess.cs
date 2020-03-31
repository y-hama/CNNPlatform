using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CNNPlatform
{
    static class LearningProcess
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        internal static void Main(string[] args)
        {
            Initializer.Startup();

            // Learningを実施する
            // LockServerとして動作する

            var instance = (SharedObject)Components.Locker.ObjectLocker.CreateServer(SharedObject.ChannelName, SharedObject.ObjectName, typeof(SharedObject));
            Model.Creater.Core.Instance = instance;
            var model = Model.Creater.Core.TestModel(4);

            var inputvariavble = model.Layer[0].Variable as Function.Variable.VariableBase;
            var outputvariavble = model.Layer[model.Layer.Count - 1].Variable as Function.Variable.VariableBase;

            using (instance.Lock())
            {
                instance.Initialized = true;
            }

            Components.Imaging.FileLoader.Instance.SetLocation(new System.IO.DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"/sample/"));
            Components.RNdMatrix teacher;
            while (!Initializer.Terminate)
            {
                #region LearningProcess
                Components.Imaging.FileLoader.Instance.LoadImage(
                    inputvariavble.InWidth, inputvariavble.InHeight, outputvariavble.OutWidth / inputvariavble.InWidth, inputvariavble.BatchCount,
                    inputvariavble.InputChannels, outputvariavble.OutputChannels, out inputvariavble.Input, out teacher);
                for (int i = 0; i < model.Layer.Count; i++)
                {
                    model.Layer[i].ForwardFunction.Do(model.Layer[i].Variable);
                    if (i < model.Layer.Count - 1)
                    {
                        (model.Layer[i + 1].Variable as Function.Variable.VariableBase).Input = (model.Layer[i].Variable as Function.Variable.VariableBase).Output;
                    }
                }
                outputvariavble.Sigma = outputvariavble.Output - teacher;
                for (int i = model.Layer.Count - 1; i >= 0; i--)
                {
                    model.Layer[i].BackFunction.Do(model.Layer[i].Variable);
                    if (i > 0)
                    {
                        (model.Layer[i - 1].Variable as Function.Variable.VariableBase).Sigma = (model.Layer[i].Variable as Function.Variable.VariableBase).Propagator;
                    }
                }
                Components.Imaging.View.Show(outputvariavble.Output, "learning...");
                #endregion
                Initializer.Generatiion++;
                using (instance.Lock())
                {
                    instance.Generation = Initializer.Generatiion;
                    Initializer.Terminate = instance.ExitApplication;
                }
                Console.WriteLine(Initializer.Generatiion);
            }
        }
    }
}

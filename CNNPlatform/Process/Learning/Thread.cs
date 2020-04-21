using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Process.Learning
{
    public class Thread : ThreadBase
    {
        #region Singleton
        private Thread() { StartTime = DateTime.Now; }
        public static Thread Worker { get; } = new Thread();
        private DateTime StartTime { get; set; }
        #endregion

        private enum FlagState
        {
            Initial,
            Adjustment,
            Update,
        }
        private FlagState State { get; set; } = FlagState.Initial;
        private bool IterationFlag { get; set; }

        public string LoadFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\sample\";

        public bool SaveModel { get; set; } = true;

        private Task.ModelSaver ModelSaver { get; set; }

        protected override int BatchCount { get { return 4; } }

        protected override void SetInputLoaderOption()
        {
            Loader.InputSource = CNNPlatform.Process.Task.InputLoader.Source.File;

            Loader.SourceLocation = new System.IO.DirectoryInfo(LoadFolder);
            Loader.Flip = true;
            Loader.Rotation = true;
            Loader.Offset = true;
        }

        protected override void CreateModelWriter()
        {
            ModelSaver = new CNNPlatform.Process.Task.ModelSaver();
            ModelSaver.SetLocation(new System.IO.DirectoryInfo(ModelSaveFolder));
            ModelSaver.Start();
        }

        protected override void LoadInput()
        {
            Input.Data = Loader.Input.Data.Clone() as Components.Real[];
            Teacher.Data = Loader.Teacher.Data.Clone() as Components.Real[];
            IterationFlag = Loader.EpochIteration;
        }

        protected override void Process()
        {
            if (IterationFlag)
            {
                if (SaveModel)
                {
                    ModelSaver.Pushback(Model.Clone());
                    ModelSaver.Request.Set();
                }
                switch (State)
                {
                    case FlagState.Initial:
                        State = FlagState.Adjustment;
                        break;
                    case FlagState.Adjustment:
                        State = FlagState.Update;
                        {
                            Model.UpdateState(true);
                        }
                        break;
                    case FlagState.Update:
                        break;
                    default:
                        break;
                }
            }

            Model.Learning(Input, Teacher, IterationFlag);

            var result = Model.ShowResult(640, 480);
            var process = Model.ShowProcess();
            Components.Imaging.View.Show(result, "result");
            Components.Imaging.View.Show(process, "process");
            var span = (DateTime.Now - StartTime);
            Console.WriteLine(Model.Epoch + " / " + Model.Generation + " / " + Model.OutputLayer.Variable.Error[0] +
                "  :" + span.Days + ":" + span.Hours + ":" + span.Minutes + ":" + span.Seconds + "'" + span.Milliseconds);

        }
    }
}

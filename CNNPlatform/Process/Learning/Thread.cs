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
        #endregion

        private DateTime StartTime { get; set; }

        private bool IterationFlag { get; set; }

        public string LoadFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"/sample/";

        private Model.Model ModelClone { get; set; }

        protected override void SetInputLoaderOption()
        {
            Loader.InputSource = CNNPlatform.Process.Task.InputLoader.Source.File;

            Loader.SourceLocation = new System.IO.DirectoryInfo(LoadFolder);
        }

        protected override void CreateModelWriter()
        {


            new System.Threading.Thread(() => { }).Start();
        }

        protected override void LoadInput()
        {
            Input.Data = Loader.Input.Data.Clone() as Components.Real[];
            Teacher.Data = Loader.Teacher.Data.Clone() as Components.Real[];
            IterationFlag = Loader.EpochIteration;
        }

        protected override void Process()
        {
            //Model.Learning(Input, Teacher, IterationFlag);

            var result = Model.ShowResult(640, 480);
            var process = Model.ShowProcess();
            Components.Imaging.View.Show(result, "result");
            Components.Imaging.View.Show(process, "process");
            var span = (DateTime.Now - StartTime);
            Console.WriteLine(Model.Epoch + " / " + Model.Generation + " / " + Model.InputLayer.Variable.Error[0] +
                "  :" + span.Days + ":" + span.Hours + ":" + span.Minutes + ":" + span.Seconds + "'" + span.Milliseconds);

            // セーブ中じゃなかったら・・・
            // コピー中にして
            // コピー
            ModelClone = Model.Clone();
            ModelClone.Save("test.mdl");
            // セーブ中を解除
        }

        public void Save(string path)
        {

        }
    }
}

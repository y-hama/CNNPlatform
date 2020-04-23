using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Process.Inference
{
    public class Thread : ThreadBase
    {
        #region Singleton
        private Thread() { StartTime = DateTime.Now; }
        public static Thread Worker { get; } = new Thread();
        private DateTime StartTime { get; set; }
        #endregion

        protected override int BatchCount { get { return 1; } }

        protected override void SetInputLoaderOption()
        {
            Loader.InputSource = CNNPlatform.Process.Task.InputLoader.Source.Camera;
            Loader.FlipX = false;
            Loader.FlipY = false;
            Loader.Rotation = 0;
            Loader.Offset = false;
        }

        protected override void LoadInput()
        {
            Input.Data = Loader.Input.Data.Clone() as Components.Real[];
        }

        protected override void Process()
        {
            Components.RNdMatrix output;
            Model.Inference(Input, out output);

            Components.Imaging.View.Show(output, "output");

            var process = Model.ShowProcess();
            Components.Imaging.View.Show(process, "process");
        }
    }
}

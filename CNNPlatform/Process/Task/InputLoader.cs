using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

namespace CNNPlatform.Process.Task
{
    class InputLoader
    {
        public Components.RNdMatrix Input { get; private set; }
        public Components.RNdMatrix Teacher { get; private set; }

        public enum Source
        {
            Null,
            File,
            Camera,
        }

        public Source InputSource { get; set; } = Source.Null;

        public System.IO.DirectoryInfo SourceLocation { get; set; } = null;
        public int[] InputShape { get; set; } = null;

        public System.IO.DirectoryInfo TeacherLocation { get; set; } = null;
        public int[] TeacherShape { get; set; } = null;

        public ManualResetEvent Load { get; set; } = new ManualResetEvent(false);
        public ManualResetEvent Request { get; set; } = new ManualResetEvent(false);

        public bool EpochIteration { get; private set; }

        public void Start()
        {
            #region Configuration
            switch (InputSource)
            {
                case Source.Null:
                    break;
                case Source.File:
                    if (SourceLocation != null)
                    {
                        Components.Imaging.FileLoader.Instance.SetSourceLocation(SourceLocation);
                    }
                    if (TeacherLocation != null)
                    {
                        Components.Imaging.FileLoader.Instance.SetSourceLocation(TeacherLocation);
                    }
                    break;
                case Source.Camera:
                    break;
                default:
                    break;
            }
            #endregion

            new System.Threading.Thread(() => Process())
            {
                Priority = ThreadPriority.AboveNormal,
            }.Start();
        }

        private void Process()
        {
            while (!Initializer.Terminate)
            {
                Request.Reset();
                Load.Reset();
                switch (InputSource)
                {
                    case Source.Null:
                        break;
                    case Source.File:
                        LoadImage();
                        break;
                    case Source.Camera:
                        break;
                    default:
                        break;
                }
                Load.Set();
                Request.WaitOne();
            }
        }

        private void LoadImage()
        {
            Components.RNdMatrix input, teacher;
            EpochIteration = Components.Imaging.FileLoader.Instance.LoadImage(
                InputShape[0], InputShape[1], InputShape[2], InputShape[3],
                TeacherShape[1], TeacherShape[2], TeacherShape[3],
                out input, out teacher);
            Input = input; Teacher = teacher;
        }
    }
}

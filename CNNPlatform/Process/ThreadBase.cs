using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Components.Locker;
using CNNPlatform.Utility.Shared;

namespace CNNPlatform.Process
{
    public abstract class ThreadBase
    {
        public string ModelSaveFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\model";
        private ModelParameter ModelParameter { get; set; } = null;
        internal Model.Model Model { get; set; } = null;

        protected abstract int BatchCount { get; }

        protected virtual int StartBlock { get; } = -1;
        protected virtual int EndBlock { get; } = -1;

        #region Buffer
        protected Components.RNdMatrix Input { get { return Model.InputLayer.Variable.Input; } }
        protected Components.RNdMatrix Output { get { return Model.OutputLayer.Variable.Output; } }
        protected Components.RNdMatrix Sigma { get { return Model.OutputLayer.Variable.Sigma; } }
        private Components.RNdMatrix teacher = null;
        protected Components.RNdMatrix Teacher
        {
            get
            {
                if (teacher == null)
                {
                    teacher = new Components.RNdMatrix(Output.Shape);
                }
                return teacher;
            }
        }
        #endregion

        internal Task.InputLoader Loader { get; set; } = null;

        public void Start()
        {
            // GPU初期化
            Initializer.Startup();

            // LockServer構築
            // 必要に応じてCreateModelParameterを呼び出す

            CreateModel();
            CreateInputLoader();
            CreateModelWriter();

            while (!Initializer.Terminate)
            {
                Loader.Load.WaitOne();
                LoadInput();
                Loader.Request.Set();

                Process();

                GC.Collect();
            }
        }

        private void CreateModelParameter()
        {
            ModelParameter = (ModelParameter)ObjectLocker.CreateServer(ModelParameter.ChannelName, ModelParameter.ObjectName, typeof(ModelParameter));
        }

        private void CreateModel()
        {
            CNNPlatform.Model.Creater.Core.Instance = ModelParameter;
            // Model生成
            if ((Model = CNNPlatform.Model.Model.Load(ModelSaveFolder, BatchCount)) == null)
            {
                CNNPlatform.Model.Creater.Core.BatchCount = BatchCount;
                Model = CNNPlatform.Model.Creater.Core.BasicImageCreater();
            }
            Console.WriteLine("BatchCount : {0}", BatchCount);
            Model.StartBlock = StartBlock;
            Model.EndBlock = EndBlock;
        }

        private void CreateInputLoader()
        {
            if (Model != null)
            {
                Loader = new CNNPlatform.Process.Task.InputLoader();

                Loader.InputShape = Model.InputLayer.Variable.Input.Shape;
                Loader.TeacherShape = Model.OutputLayer.Variable.Output.Shape;
                SetInputLoaderOption();

                Loader.Start();
            }
        }

        protected abstract void SetInputLoaderOption();

        protected virtual void CreateModelWriter() { }

        protected abstract void LoadInput();
        protected abstract void Process();
    }
}

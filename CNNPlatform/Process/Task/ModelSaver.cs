using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;


namespace CNNPlatform.Process.Task
{
    class ModelSaver
    {
        private System.IO.DirectoryInfo Root { get; set; }
        private BlockingCollection<Model.Model> ModelClone { get; set; } = new BlockingCollection<CNNPlatform.Model.Model>();

        public ManualResetEvent Request { get; set; } = new ManualResetEvent(false);

        public void SetLocation(System.IO.DirectoryInfo root)
        {
            if (!root.Exists) { root.Create(); }
            Root = root;
        }

        public void Start()
        {
            new System.Threading.Thread(() => Process())
            {
                Priority = ThreadPriority.BelowNormal,
            }.Start();
        }

        private void Process()
        {
            while (!Initializer.Terminate)
            {
                Request.WaitOne();
                while (ModelClone.Count > 0)
                {
                    Model.Model model;
                    while (!ModelClone.TryTake(out model)) { System.Threading.Thread.Sleep(1); }
                    Save(model);
                    Console.WriteLine("Epoch({0})Model SaveProcessCompleted", model.Epoch);
                }
                Request.Reset();
            }
        }

        private void Save(Model.Model model)
        {
            var mloc = new System.IO.DirectoryInfo(Root.FullName + @"\" + model.Epoch);
            if (!Root.Exists) { Root.Create(); }
            //model.Save(mloc.FullName);
            model.SaveTemporary(mloc.FullName);
        }

        public void Pushback(Model.Model model)
        {
            while (!ModelClone.TryAdd(model)) { System.Threading.Thread.Sleep(1); }
        }
    }
}

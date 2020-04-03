using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CNNPlatform.Utility.Shared;

namespace ProcessVisualizer
{
    public partial class Form1 : Form
    {
        private ProcessParameter Instance { get; set; }
        private int Generagion { get; set; }

        public Form1()
        {
            InitializeComponent();

            //Components.Locker.Process.Start("CNNPlatform.exe", new bool[] { true, true, true, true, false, false, false, false }, "1 inference");
            //Components.Locker.Process.Start("CNNPlatform.exe", new bool[] { false, false, false, false, false, true, true, false }, "8 learning");

            Components.Locker.Process.Start("CNNPlatform.exe", new bool[] { true, true, true, true, false, false, false, false });

            Instance = Components.Locker.ObjectLocker.CreateClient(ProcessParameter.ChannelName, ProcessParameter.ObjectName) as ProcessParameter;
            bool check = false;
            while (!check)
            {
                System.Threading.Thread.Sleep(1000);
                try
                {
                    using (Instance.Lock(Components.Locker.Priority.Critical))
                    {
                        check = true;
                    }
                }
                catch (Exception)
                { /* 繋がるまでリトライを繰り返す */ }
            }

            timer1.Start();

            new Task(() =>
            {
                Components.RNdMatrix result = null;
                while (true)
                {
                    bool getlock = false;
                    try
                    {
                        using (var key = Instance.LockThrow())
                        {
                            if (key != null)
                            {
                                getlock = true;
                                if (Instance.Result != null)
                                {
                                    if (result == null) { result = new Components.RNdMatrix(Instance.Result.Shape); }
                                    Instance.Result.CopyTo(result);
                                }
                                Generagion = Instance.Generagion;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    if (getlock && result != null)
                    {
                        Components.Imaging.View.Show(result, "result");
                    }
                }
            }).Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            using (Instance.Lock(Components.Locker.Priority.Critical))
            {
                Instance.ExitApplication = true;
            }
        }

        int gen = 0;
        Queue<int> genq { get; set; } = new Queue<int>();
        Queue<Components.Real[]> diff { get; set; } = new Queue<Components.Real[]>();
        Queue<double> error { get; set; } = new Queue<double>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            Text = CNNPlatform.InferenceProcess.ModelGeneration.ToString();
            try
            {
                if (gen != CNNPlatform.InferenceProcess.ModelGeneration)
                {
                    gen = CNNPlatform.InferenceProcess.ModelGeneration;
                    genq.Enqueue(CNNPlatform.InferenceProcess.ModelGeneration);
                    diff.Enqueue(CNNPlatform.InferenceProcess.Difference[0]);
                    error.Enqueue(CNNPlatform.InferenceProcess.LearningError);
                    if (diff.Count > 1000)
                    {
                        genq.Dequeue();
                        diff.Dequeue();
                        error.Dequeue();
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            if (diff.Count > 0)
            {
                chart1.Series[0].Points.Clear();
                chart1.Series[0].Name = "bias";
                chart1.Series[1].Points.Clear();
                chart1.Series[1].Name = "kernel";
                chart1.Series[2].Points.Clear();
                chart1.Series[2].Name = "error";
                var _diff = diff.ToArray();
                var _err = error.ToArray();
                var _gen = genq.ToArray();
                for (int i = 0; i < _diff.Length; i++)
                {
                    chart1.Series[0].Points.AddXY(_gen[i], _diff[i][0]);
                    chart1.Series[1].Points.AddXY(_gen[i], _diff[i][1]);
                    chart1.Series[2].Points.AddXY(_gen[i], _err[i]);
                }
            }

        }
    }
}

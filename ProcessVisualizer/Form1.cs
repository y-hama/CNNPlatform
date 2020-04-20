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

            Components.Locker.Process.Start("CNNPlatform.exe", new bool[] { true, true, true, true, false, false, false, false }, "1 inference");
            //Components.Locker.Process.Start("CNNPlatform.exe", new bool[] { false, false, false, false, false, true, true, false }, "8 learning");

            //Instance = Components.Locker.ObjectLocker.CreateClient(ProcessParameter.ChannelName, ProcessParameter.ObjectName) as ProcessParameter;
            //bool check = false;
            //while (!check)
            //{
            //    System.Threading.Thread.Sleep(1000);
            //    try
            //    {
            //        using (Instance.Lock(Components.Locker.Priority.Critical))
            //        {
            //            check = true;
            //        }
            //    }
            //    catch (Exception)
            //    { /* 繋がるまでリトライを繰り返す */ }
            //}

            //timer1.Start();

            //new Task(() =>
            //{
            //    Components.RNdMatrix result = null;
            //    while (true)
            //    {
            //        bool getlock = false;
            //        try
            //        {
            //            using (var key = Instance.LockThrow())
            //            {
            //                if (key != null)
            //                {
            //                    getlock = true;
            //                    if (Instance.Result != null)
            //                    {
            //                        if (result == null) { result = new Components.RNdMatrix(Instance.Result.Shape); }
            //                        result = Instance.Result;
            //                    }
            //                    Generagion = Instance.Generagion;
            //                }
            //            }
            //        }
            //        catch (Exception)
            //        {
            //            continue;
            //        }
            //        if (getlock && result != null)
            //        {
            //            Components.Imaging.View.Show(result, "result");
            //        }
            //    }
            //}).Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //using (Instance.Lock(Components.Locker.Priority.Critical))
            //{
            //    Instance.ExitApplication = true;
            //}
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessVisualizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Components.Locker.Process.Start("CNNPlatform.exe", new bool[] { true, true, true, true });

            var instance = Components.Locker.ObjectLocker.CreateClient(CNNPlatform.SharedObject.ChannelName, CNNPlatform.SharedObject.ObjectName) as CNNPlatform.SharedObject;
            bool check = false;
            while (!check)
            {
                System.Threading.Thread.Sleep(1000);
                try
                {
                    using (instance.Lock(Components.Locker.Priority.Critical))
                    {
                        check = instance.Initialized;
                    }
                }
                catch (Exception) { }
            }

            CNNPlatform.InferenceProcess.Core.Start(instance);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CNNPlatform.InferenceProcess.Terminate = true;
        }
    }
}

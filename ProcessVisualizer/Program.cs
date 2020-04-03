using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessVisualizer
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
#if !DEBUG
            if (args.Length == 0)
            {
                Components.Locker.Process.Start("ProcessVisualizer.exe", new bool[] { false, false, false, false, false, false, false, true }, "s");
            }
            else
#endif
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform
{
    class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        internal static void Main(string[] args)
        {
            Process.Learning.Thread.Worker.Start();
            //Process.Inference.Thread.Worker.Start();

        }
    }
}

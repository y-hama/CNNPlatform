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
            if (args.Length == 0)
            {
                TheradLearning.Core.Start();
            }
            else
            {
                if (args.Length == 1)
                {
                    uint bcnt;
                    if (UInt32.TryParse(args[0], out bcnt))
                    {
                        LearningProcess.Core.BatchCount = bcnt > 0 ? (int)bcnt : 1;
                    }
                    else { throw new Exception(); }
                }
                else if (args.Length == 2)
                {
                    uint bcnt;
                    string mode;
                    mode = args[1];
                    if (mode == "learning")
                    {
                        if (UInt32.TryParse(args[0], out bcnt))
                        {
                            LearningProcess.Core.BatchCount = bcnt > 0 ? (int)bcnt : 1;
                        }
                        else { throw new Exception(); }
                        TheradLearning.Core.Start();
                    }
                    else if (mode == "inference")
                    {
                        InferenceProcess.Core.Start();
                    }
                }
            }
        }
    }
}

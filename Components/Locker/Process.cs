using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components.Locker
{
    public class Process
    {
        public static System.Diagnostics.Process Start(string path, bool[] useCore, params string[] args)
        {
            if (System.IO.File.Exists(path))
            {
                string arguments = string.Empty;
                foreach (var item in args)
                {
                    arguments += item + " ";
                }
                var pinfo = new System.Diagnostics.ProcessStartInfo(path, arguments);
                var p = new System.Diagnostics.Process();

                var ProcessorAffinity = 0;
                for (int i = 0; i < useCore.Length; i++)
                {
                    ProcessorAffinity |= (useCore[i] ? 1 : 0) << i;
                }
                p.StartInfo = pinfo;
                p.Start();
                p.ProcessorAffinity = (IntPtr)ProcessorAffinity;
                return p;
            }
            else
            {
                return null;
            }
        }

    }
}

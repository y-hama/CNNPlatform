using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Function.Process.GpgpuSource
{
    class gp_convback_kernel : Components.GPGPU.Function.SourceCode
    {
        protected override void ParameterConfigration()
        {
        }
        protected override void CreateSource()
        {
            GlobalID(3);
        }

    }
}

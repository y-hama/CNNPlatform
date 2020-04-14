using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Process.GpgpuSource
{
    class gp_affineback_prop : Components.GPGPU.Function.SourceCode
    {
        protected override void ParameterConfigration()
        {
            AddParameter("Propagator", ObjectType.Array, ElementType.FLOAT);
            AddParameter("Sigma", ObjectType.Array, ElementType.FLOAT);
            AddParameter("Weight", ObjectType.Array, ElementType.FLOAT);

            AddParameter("BatchCount", ObjectType.Value, ElementType.INT);
            AddParameter("InWidth", ObjectType.Value, ElementType.INT);
            AddParameter("InHeight", ObjectType.Value, ElementType.INT);
            AddParameter("InputChannels", ObjectType.Value, ElementType.INT);
            AddParameter("OutWidth", ObjectType.Value, ElementType.INT);
            AddParameter("OutHeight", ObjectType.Value, ElementType.INT);
            AddParameter("OutputChannels", ObjectType.Value, ElementType.INT);
            AddParameter("InSize", ObjectType.Value, ElementType.INT);
            AddParameter("InArea", ObjectType.Value, ElementType.INT);
            AddParameter("InTotal", ObjectType.Value, ElementType.INT);
            AddParameter("OutSize", ObjectType.Value, ElementType.INT);
            AddParameter("OutArea", ObjectType.Value, ElementType.INT);
            AddParameter("OutTotal", ObjectType.Value, ElementType.INT);
        }

        protected override void CreateSource()
        {
            GlobalID(2);
            AddMethodBody(@"
                    int iidx = i0 * InArea + i1;
                    Propagator[iidx] = 0;
                    for (int o = 0; o < OutArea; o++)
                    {
                        int oidx = i0 * OutArea + o;
                        int widx = o * (InArea + 1) + (i1 + 1);
                        Propagator[iidx] += Sigma[oidx] * Weight[widx];
                    }
");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Process.GpgpuSource
{
    class gp_convback_bias : Components.GPGPU.Function.SourceCode
    {
        protected override void ParameterConfigration()
        {
            AddParameter("Sigma", ObjectType.Array, ElementType.FLOAT);
            AddParameter("dBias", ObjectType.Array, ElementType.FLOAT);

            AddParameter("BatchCount", ObjectType.Value, ElementType.INT);
            AddParameter("InWidth", ObjectType.Value, ElementType.INT);
            AddParameter("InHeight", ObjectType.Value, ElementType.INT);
            AddParameter("InputChannels", ObjectType.Value, ElementType.INT);
            AddParameter("OutScale", ObjectType.Value, ElementType.FLOAT);
            AddParameter("OutWidth", ObjectType.Value, ElementType.INT);
            AddParameter("OutHeight", ObjectType.Value, ElementType.INT);
            AddParameter("OutputChannels", ObjectType.Value, ElementType.INT);
            AddParameter("InSize", ObjectType.Value, ElementType.INT);
            AddParameter("InArea", ObjectType.Value, ElementType.INT);
            AddParameter("InTotal", ObjectType.Value, ElementType.INT);
            AddParameter("OutSize", ObjectType.Value, ElementType.INT);
            AddParameter("OutArea", ObjectType.Value, ElementType.INT);
            AddParameter("OutTotal", ObjectType.Value, ElementType.INT);
            AddParameter("KernelSize", ObjectType.Value, ElementType.INT);
            AddParameter("KernelArea", ObjectType.Value, ElementType.INT);
            AddParameter("KernelLength", ObjectType.Value, ElementType.INT);
            AddParameter("KernelExpand", ObjectType.Value, ElementType.INT);
        }

        protected override void CreateSource()
        {
            GlobalID(2);
            AddMethodBody(@"
                int bidx = i0;
                dBias[bidx] = 0;
                for (int b = 0; b < BatchCount; b++)
                {
                    for (int i = 0; i < OutSize; i++)
                    {
                        int oidx = b * OutArea + i0 * OutSize + i;
                        dBias[bidx] += Sigma[oidx];
                    }
                }
");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Function.Process.GpgpuSource
{
    class gp_convback_bias : Components.GPGPU.Function.SourceCode
    {
        protected override void ParameterConfigration()
        {
            AddParameter("sigma", ObjectType.Array, ElementType.FLOAT);
            AddParameter("dbias", ObjectType.Array, ElementType.FLOAT);

            AddParameter("batch", ObjectType.Value, ElementType.INT);
            AddParameter("ich", ObjectType.Value, ElementType.INT);
            AddParameter("och", ObjectType.Value, ElementType.INT);
            AddParameter("OutTotalArea", ObjectType.Value, ElementType.INT);
            AddParameter("OutTotalSize", ObjectType.Value, ElementType.INT);
            AddParameter("klength", ObjectType.Value, ElementType.INT);
            AddParameter("InTotalSize", ObjectType.Value, ElementType.INT);
        }

        protected override void CreateSource()
        {
            GlobalID(2);
            AddMethodBody(@"
                    int tbidx = i0 * (och) + i1;
                    dbias[tbidx] = 0;
                    for (int b = 0; b < batch; b++)
                    {
                        for (int i = 0; i < OutTotalSize; i++)
                        {
                            int toidx = b * OutTotalArea + i1 * OutTotalSize + i;
                            dbias[tbidx] += sigma[toidx];
                        }
                    }
                    dbias[tbidx] /= ich * klength * InTotalSize * batch;
");
        }

    }
}

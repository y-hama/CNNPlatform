using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Process.GpgpuSource
{
    class gp_affineback_weight : Components.GPGPU.Function.SourceCode
    {
        protected override void ParameterConfigration()
        {
            AddParameter("Input", ObjectType.Array, ElementType.FLOAT);
            AddParameter("Sigma", ObjectType.Array, ElementType.FLOAT);
            AddParameter("dWeight", ObjectType.Array, ElementType.FLOAT);

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
            GlobalID(1);
            AddMethodBody(@"
                int ool = (int)(i0 / (InArea + 1));
                int iil = i0 - (int)(ool * (InArea + 1));
                int widx = ool * (InArea + 1) + (iil);
                for (int b = 0; b < BatchCount; b++)
                {
                    int sidx = b * OutArea + ool;
                    int iidx = b * InArea + iil;
                    dWeight[widx] += Sigma[sidx] * (iil == 0 ? 1 : Input[iidx - 1]);
                }
                dWeight[widx] /= BatchCount;
");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Process.GpgpuSource
{
    class gp_affinefowd : Components.GPGPU.Function.SourceCode
    {
        protected override void ParameterConfigration()
        {
            AddParameter("Input", ObjectType.Array, ElementType.FLOAT);
            AddParameter("Output", ObjectType.Array, ElementType.FLOAT);
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
                    int oidx = i0 * OutArea + i1;
                    int widx = i1 * (InArea + 1);
                    Output[oidx] = Weight[widx];
                    for (int ii = 0; ii < InArea; ii++)
                    {
                        int iidx = i0 * InArea + ii;
                        widx = i1 * (InArea + 1) + (ii + 1);
                        Output[oidx] += Input[iidx] * Weight[widx];
                    }
");
        }
    }
}

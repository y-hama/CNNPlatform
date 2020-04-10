using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Process.GpgpuSource
{
    class gp_plfowd : Components.GPGPU.Function.SourceCode
    {
        protected override void ParameterConfigration()
        {
            AddParameter("Input", ObjectType.Array, ElementType.FLOAT);
            AddParameter("Output", ObjectType.Array, ElementType.FLOAT);
            AddParameter("Map", ObjectType.Array, ElementType.FLOAT);

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
            AddParameter("CompressSize", ObjectType.Value, ElementType.INT);
            AddParameter("ExpandSize", ObjectType.Value, ElementType.INT);
        }

        protected override void CreateSource()
        {
            GlobalID(3);
            AddMethodBody(@"
                        int locy = (int)(i2 / (InWidth / CompressSize));
                        int locx = i2 - locy * (InWidth / CompressSize);
                        double clr = -1;

                        for (int ii = 0; ii < CompressSize; ii++)
                        {
                            for (int ij = 0; ij < CompressSize; ij++)
                            {
                                int iidx = i0 * InArea + i1 * InSize + (locy * CompressSize + ij) * InWidth + (locx * CompressSize + ii);
                                if (clr < Input[iidx])
                                {
                                    clr = Input[iidx];
                                }
                            }
                        }
                        for (int ii = 0; ii < CompressSize; ii++)
                        {
                            for (int ij = 0; ij < CompressSize; ij++)
                            {
                                int iidx = i0 * InArea + i1 * InSize + (locy * CompressSize + ij) * InWidth + (locx * CompressSize + ii);
                                if (clr == Input[iidx])
                                {
                                    Map[iidx] = 1;
                                }
                                else { Map[iidx] = 0; }
                            }
                        }

                        for (int oi = 0; oi < ExpandSize; oi++)
                        {
                            for (int oj = 0; oj < ExpandSize; oj++)
                            {
                                int oidx = i0 * OutArea + i1 * OutSize + (locy * ExpandSize + oj) * OutWidth + (locx * ExpandSize + oi);
                                Output[oidx] = clr;
                            }
                        }
");
        }
    }
}

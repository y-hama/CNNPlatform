using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Process.GpgpuSource
{
    class gp_plback : Components.GPGPU.Function.SourceCode
    {
        protected override void ParameterConfigration()
        {
            AddParameter("Sigma", ObjectType.Array, ElementType.FLOAT);
            AddParameter("Propagator", ObjectType.Array, ElementType.FLOAT);
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
                        int locy = (int)(i2 / (OutWidth / ExpandSize));
                        int locx = i2 - locy * (OutWidth / ExpandSize);
                        double clr = -255;

                        for (int ii = 0; ii < ExpandSize; ii++)
                        {
                            for (int ij = 0; ij < ExpandSize; ij++)
                            {
                                int oidx = i0 * OutArea + i1 * OutSize + (locy * ExpandSize + ij) * OutWidth + (locx * ExpandSize + ii);
                                if (clr < Sigma[oidx])
                                {
                                    clr = Sigma[oidx];
                                }
                            }
                        }

                        for (int ii = 0; ii < CompressSize; ii++)
                        {
                            for (int ij = 0; ij < CompressSize; ij++)
                            {
                                int iidx = i0 * InArea + i1 * InSize + (locy * CompressSize + ij) * InWidth + (locx * CompressSize + ii);
                                if (Map[iidx] != 0)
                                {
                                    Propagator[iidx] = clr;
                                }
                                else { Propagator[iidx] = 0; }
                            }
                        }
");
        }
    }
}

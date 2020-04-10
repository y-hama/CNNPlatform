using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Process.GpgpuSource
{
    class gp_convback_prop : Components.GPGPU.Function.SourceCode
    {
        protected override void ParameterConfigration()
        {
            AddParameter("Sigma", ObjectType.Array, ElementType.FLOAT);
            AddParameter("WeightKernel", ObjectType.Array, ElementType.FLOAT);
            AddParameter("Propagator", ObjectType.Array, ElementType.FLOAT);

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
            GlobalID(3);
            AddMethodBody(@"
                        int y = (int)(i2 / InWidth);
                        int x = i2 - y * InWidth;
                        int pidx = i0 * InArea + i1 * InSize + i2;
                        Propagator[pidx] = 0;
                        for (int k = 0; k < KernelLength; k++)
                        {
                            int t = k / KernelArea;
                            int s = k - t * KernelArea;
                            int ox = (int)(((float)x / OutScale) - KernelExpand * (s - KernelSize));
                            int oy = (int)(((float)y / OutScale) - KernelExpand * (t - KernelSize));
                            if (ox >= 0 && oy >= 0 && ox < OutWidth && oy < OutHeight)
                            {
                                for (int och = 0; och < OutputChannels; och++)
                                {
                                    int oidx = i0 * OutArea + och * OutSize + oy * OutWidth + ox;
                                    int kidx = i1 * (OutputChannels * KernelLength) + och * KernelLength + k;
                                    Propagator[pidx] += Sigma[oidx] * WeightKernel[kidx];
                                }
                            }
                        }
");
        }
    }
}

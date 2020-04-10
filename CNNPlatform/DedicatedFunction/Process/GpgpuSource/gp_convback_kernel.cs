using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Process.GpgpuSource
{
    class gp_convback_kernel : Components.GPGPU.Function.SourceCode
    {
        protected override void ParameterConfigration()
        {
            AddParameter("Input", ObjectType.Array, ElementType.FLOAT);
            AddParameter("Sigma", ObjectType.Array, ElementType.FLOAT);
            AddParameter("dKernel", ObjectType.Array, ElementType.FLOAT);

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
                        int t = (int)(i2 / KernelArea);
                        int s = i2 - t * (KernelArea);
                        int _s = s - KernelSize, _t = t - KernelSize;
                        int kidx = i0 * (OutputChannels * KernelLength) + i1 * KernelLength + i2;
                        dKernel[kidx] = 0;
                        for (int b = 0; b < BatchCount; b++)
                        {
                            for (int i = 0; i < OutSize; i++)
                            {
                                int oy = (int)(i / OutWidth);
                                int ox = (int)(i - oy * OutWidth);
                                int oidx = b * OutArea + i1 * OutSize + i;

                                int _ix = (int)(((float)ox * OutScale) + _s * KernelExpand);
                                int _iy = (int)(((float)oy * OutScale) + _t * KernelExpand);
                                if (_ix >= 0 && _iy >= 0 && _ix < InWidth && _iy < InHeight)
                                {
                                    int _idx = b * InArea + i0 * InSize + _iy * InWidth + _ix;
                                    dKernel[kidx] += Input[_idx] * Sigma[oidx];
                                }
                            }
                        }
");
        }

    }
}

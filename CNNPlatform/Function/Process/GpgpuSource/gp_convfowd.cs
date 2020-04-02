using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Function.Process.GpgpuSource
{
    class gp_convfowd : Components.GPGPU.Function.SourceCode
    {
        protected override void ParameterConfigration()
        {
            AddParameter("Input", ObjectType.Array, ElementType.FLOAT);
            AddParameter("Output", ObjectType.Array, ElementType.FLOAT);
            AddParameter("WeightBias", ObjectType.Array, ElementType.FLOAT);
            AddParameter("WeightKernel", ObjectType.Array, ElementType.FLOAT);

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
                        float output = 0;
                        int y = (int)(i2 / OutWidth);
                        int x = i2 - y * OutWidth;
                        int otx = i0 * OutArea + i1 * OutSize + i2;

                        for (int k = 0; k < KernelLength; k++)
                        {
                            int t = k / KernelArea;
                            int s = k - t * KernelArea;
                            int ix = (int)(((float)x / OutScale) + KernelExpand * (s - KernelSize));
                            int iy = (int)(((float)y / OutScale) + KernelExpand * (t - KernelSize));
                            if (ix >= 0 && ix < InWidth && iy >= 0 && iy < InHeight)
                            {
                                for (int ich = 0; ich < InputChannels; ich++)
                                {
                                    int itx = i0 * InArea + ich * InSize + iy * InWidth + ix;
                                    int ktx = ich * (OutputChannels * KernelLength) + i1 * KernelLength + k;
                                    int btx = ich * OutputChannels + i1;
                                    output += Input[itx] * WeightKernel[ktx] + WeightBias[btx];
                                }
                            }
                        }
                        Output[otx] = output;
");
        }
    }
}

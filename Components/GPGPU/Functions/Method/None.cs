using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Functions.Method
{
    class None : FunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new Source.None());
        }

        protected override void CpuFunction(ComputeVariable variable)
        {
            Real[] input = variable[0].Instance.Array.Data;
            Real[] output = variable[1].Instance.Array.Data;

            int len = variable[0].Infomation.ArrayLength;
            int area = 10000;

            for (int i0 = 0; i0 < len; i0++)
            {
                int min, max;
                if (i0 - area > 0) { min = i0 - area; } else { min = 0; }
                if (i0 + area < len) { max = i0 + area; } else { max = len - 1; }
                output[i0] = 0;

                for (int i = min; i < max; i++)
                {
                    output[i0] += (input[i]);
                }
                output[i0] /= (max - min);
            }
        }

        protected override void GpuFunction(ComputeVariable variable)
        {
            int len = variable[0].Infomation.ArrayLength;
            int area = 10000;

            using (Cloo.ComputeBuffer<Real> _input = ConvertBuffer(variable[0].Instance))
            using (Cloo.ComputeBuffer<Real> _output = ConvertBuffer(variable[1].Instance))
            {
                SetParameter(_input);
                SetParameter(_output);
                SetParameter(len, ValueMode.INT);
                SetParameter(area, ValueMode.INT);
                Execute(len);
                ReadBuffer(_output, ref variable[1].Instance.Array.Data);
            }
        }
    }
}

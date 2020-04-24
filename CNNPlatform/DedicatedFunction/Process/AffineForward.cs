using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace CNNPlatform.DedicatedFunction.Process
{
    class AffineForward : DedicatedFunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new GpgpuSource.gp_affinefowd());
        }

        #region 
        private int BatchCount;

        private int InWidth;
        private int InHeight;
        private int InputChannels;

        private int OutWidth;
        private int OutHeight;
        private int OutputChannels;

        private int InSize;
        private int InArea;
        private int InTotal;

        private int OutSize;
        private int OutArea;
        private int OutTotal;

        private Components.Real[] Weight;

        private Components.Real[] Input;
        private Components.Real[] Output;
        #endregion


        protected override void ConvertVariable(ComputeVariable _variable)
        {
            var variable = _variable as Variable.AffineVariable;

            BatchCount = variable.BatchCount;

            InWidth = variable.InWidth;
            InHeight = variable.InHeight;
            InputChannels = variable.InputChannels;

            OutWidth = variable.OutWidth;
            OutHeight = variable.OutHeight;
            OutputChannels = variable.OutputChannels;

            InSize = variable.InSize;
            InArea = variable.InArea;
            InTotal = variable.InTotal;

            OutSize = variable.OutSize;
            OutArea = variable.OutArea;
            OutTotal = variable.OutTotal;

            Weight = variable.Weight.Data;

            Input = variable.Input.Data;
            Output = variable.Output.Data;
        }

        protected override void CpuFunction()
        {
            Parallel(0, BatchCount, i0 =>
            {
                Parallel(0, OutArea, i1 =>
                {
                    int oidx = i0 * OutArea + i1;
                    int widx = i1 * (InArea + 1);
                    Output[oidx] = Weight[widx];
                    for (int ii = 0; ii < InArea; ii++)
                    {
                        int iidx = i0 * InArea + ii;
                        widx = i1 * (InArea + 1) + (ii + 1);
                        Output[oidx] += Input[iidx] * Weight[widx];
                    }
                });
            });
        }

        protected override void GpuFunction()
        {
            using (var _input = ConvertBuffer(Input))
            using (var _output = ConvertBuffer(Output))
            using (var _weight = ConvertBuffer(Weight))
            {
                SetParameter(_input);
                SetParameter(_output);
                SetParameter(_weight);

                SetParameter(BatchCount, ValueMode.INT);
                SetParameter(InWidth, ValueMode.INT);
                SetParameter(InHeight, ValueMode.INT);
                SetParameter(InputChannels, ValueMode.INT);
                SetParameter(OutWidth, ValueMode.INT);
                SetParameter(OutHeight, ValueMode.INT);
                SetParameter(OutputChannels, ValueMode.INT);
                SetParameter(InSize, ValueMode.INT);
                SetParameter(InArea, ValueMode.INT);
                SetParameter(InTotal, ValueMode.INT);
                SetParameter(OutSize, ValueMode.INT);
                SetParameter(OutArea, ValueMode.INT);
                SetParameter(OutTotal, ValueMode.INT);

                Execute(BatchCount, OutArea);
                ReadBuffer(_output, ref Output);
            }
        }
    }
}

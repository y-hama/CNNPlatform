using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace CNNPlatform.DedicatedFunction.Process
{
    class PoolingForward : DedicatedFunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new GpgpuSource.gp_plfowd());
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

        public int CompressSize;
        public int ExpandSize;

        private Components.Real[] Input;
        private Components.Real[] Output;

        private Components.Real[] Map;
        #endregion

        protected override void ConvertVariable(ComputeVariable _variable)
        {
            var variable = _variable as Variable.PoolingVariable;

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

            CompressSize = variable.CompressSize;
            ExpandSize = variable.ExpandSize;

            Input = variable.Input.Data;
            Output = variable.Output.Data;
            Map = variable.Map.Data;
        }

        protected override void CpuFunction()
        {
            Parallel(0, BatchCount, i0 =>
            {
                Parallel(0, InputChannels, i1 =>
                {
                    Parallel(0, InSize / (CompressSize * CompressSize), i2 =>
                    {
                        int locy = (int)(i2 / (InWidth / CompressSize));
                        int locx = i2 - locy * (InWidth / CompressSize);
                        double clr = 0;
                        double cnt = 0;

                        for (int ii = 0; ii < CompressSize; ii++)
                        {
                            for (int ij = 0; ij < CompressSize; ij++)
                            {
                                int iidx = i0 * InArea + i1 * InSize + (locy * CompressSize + ij) * InWidth + (locx * CompressSize + ii);
                                clr += Input[iidx];
                                cnt = cnt + 1;
                            }
                        }
                        clr /= cnt;

                        for (int ii = 0; ii < CompressSize; ii++)
                        {
                            for (int ij = 0; ij < CompressSize; ij++)
                            {
                                int iidx = i0 * InArea + i1 * InSize + (locy * CompressSize + ij) * InWidth + (locx * CompressSize + ii);
                                Map[iidx] = 1;
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
                    });
                });
            });
        }

        protected override void GpuFunction()
        {
            using (var _input = ConvertBuffer(Input))
            using (var _output = ConvertBuffer(Output))
            using (var _map = ConvertBuffer(Map))
            {
                SetParameter(_input);
                SetParameter(_output);
                SetParameter(_map);

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
                SetParameter(CompressSize, ValueMode.INT);
                SetParameter(ExpandSize, ValueMode.INT);

                Execute(BatchCount, InputChannels, InSize / (CompressSize * CompressSize));
                ReadBuffer(_output, ref Output);
                ReadBuffer(_map, ref Map);
            }
        }
    }
}

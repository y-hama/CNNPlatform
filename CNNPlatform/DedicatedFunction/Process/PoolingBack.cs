using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace CNNPlatform.DedicatedFunction.Process
{
    class PoolingBack : Components.GPGPU.Function.ParameterizedFunctionBase
    {
        protected override void CreateGpuSource()
        {
            AddSource(new GpgpuSource.gp_plback());
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

        private Components.Real[] Sigma;
        private Components.Real[] Propagator;

        private Components.Real[] Map;

        private Components.Real[] Error;
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

            Sigma = variable.Sigma.Data;
            Propagator = variable.Propagator.Data;
            Map = variable.Map.Data;

            Error = variable.Error;
        }

        protected override void CpuFunction()
        {
            Parallel(0, BatchCount, i0 =>
            {
                Parallel(0, OutputChannels, i1 =>
                {
                    Parallel(0, OutSize / (ExpandSize * ExpandSize), i2 =>
                    {
                        int locy = (int)(i2 / (OutWidth / ExpandSize));
                        int locx = i2 - locy * (OutWidth / ExpandSize);
                        double clr = 0;
                        double cnt = 0;

                        for (int ii = 0; ii < ExpandSize; ii++)
                        {
                            for (int ij = 0; ij < ExpandSize; ij++)
                            {
                                int oidx = i0 * OutArea + i1 * OutSize + (locy * ExpandSize + ij) * OutWidth + (locx * ExpandSize + ii);
                                clr += Sigma[oidx];
                                cnt = cnt + 1;
                            }
                        }
                        clr /= cnt;

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
                    });
                });
            });
        }

        protected override void GpuFunction()
        {
            using (var _sigma = ConvertBuffer(Sigma))
            using (var _propagator = ConvertBuffer(Propagator))
            using (var _map = ConvertBuffer(Map))
            {
                SetParameter(_sigma);
                SetParameter(_propagator);
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

                Execute(BatchCount, OutputChannels, OutSize / (ExpandSize * ExpandSize));
                ReadBuffer(_propagator, ref Propagator);
            }
        }

        protected override bool UpdateConditionCheck()
        {
            return true;
        }

        public override void Update()
        {
            (Variable as DedicatedFunction.Variable.VariableBase).CalcurationError(ref Error);
        }
    }
}

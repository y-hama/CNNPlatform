using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.GPGPU;

namespace CNNPlatform.DedicatedFunction.Process
{
    class ReshapeForward : DedicatedFunctionBase
    {
        protected override void CreateGpuSource()
        {
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

        private Components.Real[] Input;
        private Components.Real[] Output;
        #endregion

        protected override void ConvertVariable(ComputeVariable _variable)
        {
            var variable = _variable as Variable.ReshapeVariable;

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

            Input = variable.Input.Data;
            Output = variable.Output.Data;
        }

        protected override void CpuFunction()
        {
            Array.Copy(Input, 0, Output, 0, Input.Length);
        }

        protected override void GpuFunction()
        {
            throw new NotImplementedException();
        }
    }
}

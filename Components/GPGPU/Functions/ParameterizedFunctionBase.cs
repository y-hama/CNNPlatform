using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Functions
{
    abstract class ParameterizedFunctionBase : FunctionBase
    {
        protected int BatchCounter { get; set; }

        #region Abstruct
        public abstract void CreateTemporaryBuffer(ComputeVariable variable);
        public abstract void Update(int count, ComputeVariable variable);

        protected abstract bool UpdateConditionCheck(ComputeVariable parameter);
        #endregion

        protected override void UpdateWithCondition(bool ForceUpdate, ComputeVariable variable)
        {
            if (ForceUpdate || UpdateConditionCheck(variable))
            {
                Update(BatchCounter, variable);
            }
        }
    }
}

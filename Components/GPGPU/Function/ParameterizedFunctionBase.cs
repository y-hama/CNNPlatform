using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function
{
    public abstract class ParameterizedFunctionBase : FunctionBase
    {
        #region Abstruct
        public abstract void Update(bool doUpdateCalculation);

        protected abstract bool UpdateConditionCheck(ref bool doUpdateCalculation);
        #endregion

        protected override void UpdateWithCondition()
        {
            bool doUpdateCalculation = false;
            if (UpdateConditionCheck(ref doUpdateCalculation))
            {
                Update(doUpdateCalculation);
            }
        }
    }
}

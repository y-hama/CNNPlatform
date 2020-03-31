using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function
{
    public abstract class ParameterizedFunctionBase : FunctionBase
    {
        #region Abstruct
        public abstract void Update();

        protected abstract bool UpdateConditionCheck();
        #endregion

        protected override void UpdateWithCondition()
        {
            if (UpdateConditionCheck())
            {
                Update();
            }
        }
    }
}

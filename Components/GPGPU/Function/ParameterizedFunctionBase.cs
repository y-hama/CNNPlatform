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

        protected void CalcurationError(ref Real[] _error, Real[] sigma)
        {
            var error = _error;
            var ave = sigma.Average(x => x);
            error[0] = 0;
            error[1] = 0;
            Parallel(0, sigma.Length, i0 =>
            {
                error[0] += sigma[i0] * sigma[i0];
                error[1] += (sigma[i0] - ave) * (sigma[i0] - ave);
            });
            //error[0] /= sigma.Length;
            error[1] /= sigma.Length;
            error[1] = Math.Sqrt(error[1]);
        }

        protected override void UpdateWithCondition()
        {
            if (UpdateConditionCheck())
            {
                Update();
            }
        }
    }
}

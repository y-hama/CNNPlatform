using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Functions
{
    class FunctionObjectBase
    {
        #region SingleArgs
        protected float exp(float x)
        {
            return (float)Math.Exp(x);
        }

        protected float abs(float x)
        {
            return Math.Abs(x);
        }

        protected float sin(float x)
        {
            return (float)Math.Sin(x);
        }

        protected float cos(float x)
        {
            return (float)Math.Cos(x);
        }

        protected float tan(float x)
        {
            return (float)Math.Tan(x);
        }
        #endregion

        #region DoubleArgs
        protected float max(float x, float y)
        {
            return Math.Max(x, y);
        }

        protected float min(float x, float y)
        {
            return Math.Min(x, y);
        }
        #endregion
    }
}

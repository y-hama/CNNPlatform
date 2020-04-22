using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function
{
    public class FunctionObjectBase
    {
        #region Iteration
        public void Parallel(int start, int end, Action<int> func)
        {
            Tasks.ForParallel(start, end, func);
        }
        #endregion

        #region SingleArgs
        protected float sqrt(float x)
        {
            return (float)Math.Sqrt(x);
        }

        protected float exp(float x)
        {
            return (float)Math.Exp(x);
        }

        protected float log(float x)
        {
            return (float)Math.Log(x);
        }

        protected float abs(float x)
        {
            return Math.Abs(x);
        }

        #region 
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

        #region
        protected float asin(float x)
        {
            return (float)Math.Asin(x);
        }

        protected float acos(float x)
        {
            return (float)Math.Acos(x);
        }

        protected float atan(float x)
        {
            return (float)Math.Atan(x);
        }
        #endregion

        #region
        protected float sinh(float x)
        {
            return (float)Math.Sinh(x);
        }

        protected float cosh(float x)
        {
            return (float)Math.Cosh(x);
        }

        protected float tanh(float x)
        {
            return (float)Math.Tanh(x);
        }
        #endregion

        protected float sign(float x)
        {
            return (float)Math.Sign(x);
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

        protected float pow(float x, float y)
        {
            return (float)Math.Pow(x, y);
        }
        #endregion
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Process.Optimizer
{
    abstract class OptimizerBase
    {

        protected bool Initialized { get; set; } = false;
        public static Components.Real Iteration { get; set; } = 0;

        public abstract double Update(ref Components.Real[] _w, Components.Real[] diff, bool doUpdate, double rho = 0);

        public static OptimizerBase CreateInstance(Utility.Types.Optimizer type)
        {
            OptimizerBase instance = null;
            switch (type)
            {
                case Utility.Types.Optimizer.Adam:
                    instance = new Adam();
                    break;
                case Utility.Types.Optimizer.AdaSelf:
                    instance = new AdaSelf();
                    break;
                default:
                    break;
            }
            return instance;
        }
    }
}

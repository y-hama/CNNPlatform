using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Utility
{
    public class Types
    {
        public enum DirectionPattern
        {
            Through,
            TurnBack,
        }

        internal enum Optimizer
        {
            Adam,
            AdaSelf,
        }

        internal enum Activator
        {
            Sigmoid,
            ReLU,
            Mish,
        }
    }
}

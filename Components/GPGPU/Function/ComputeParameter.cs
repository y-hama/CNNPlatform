using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cloo;

namespace Components.GPGPU.Function
{
    public class ComputeParameter
    {

        public Real this[int idx]
        {
            get { return Array[idx]; }
            set { Array[idx] = value; }
        }

        public string Name { get; set; }
        public Real[] Array { get; set; }
        public int Length { get { return Array.Length; } }

        public ComputeMemoryFlags MemoryMode { get; private set; }
        public Components.State.MemoryModeSet MemoryModeBase { get; private set; }
        public bool SaveFlag { get; set; }

        public ComputeParameter(string name, Real[] data, Components.State.MemoryModeSet mode)
        {
            Name = name;
            Array = data;
            MemoryModeBase = mode;
            switch (mode)
            {
                case Components.State.MemoryModeSet.ReadOnly:
                    MemoryMode = ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer;
                    break;
                case Components.State.MemoryModeSet.WriteOnly:
                    MemoryMode = ComputeMemoryFlags.WriteOnly | ComputeMemoryFlags.CopyHostPointer;
                    break;
                case Components.State.MemoryModeSet.Parameter:
                    MemoryMode = ComputeMemoryFlags.None;
                    break;
                default:
                    break;
            }
        }
    }
}

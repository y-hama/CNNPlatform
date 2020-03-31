using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cloo;

namespace Components.GPGPU.Functions
{
    public class ComputeParameter
    {
        public enum MemoryModeSet
        {
            ReadOnly,
            WriteOnly,
            Parameter,
        }

        public string Name { get; set; }
        public RNdArray Array { get; set; }
        public int Length { get { return Array.Length; } }

        public ComputeMemoryFlags MemoryMode { get; private set; }
        public MemoryModeSet MemoryModeBase { get; private set; }
        public bool SaveFlag { get; set; }

        public void Update(RNdArray ndArray)
        {
            Array = RNdArray.Convert(ndArray.Data, ndArray.Shape);
        }

        public ComputeParameter(string name, RNdArray data, MemoryModeSet mode)
        {
            Name = name;
            Array = data;
            MemoryModeBase = mode;
            switch (mode)
            {
                case MemoryModeSet.ReadOnly:
                    MemoryMode = ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer;
                    break;
                case MemoryModeSet.WriteOnly:
                    MemoryMode = ComputeMemoryFlags.WriteOnly | ComputeMemoryFlags.CopyHostPointer;
                    break;
                case MemoryModeSet.Parameter:
                    MemoryMode = ComputeMemoryFlags.None;
                    break;
                default:
                    break;
            }
        }
    }
}

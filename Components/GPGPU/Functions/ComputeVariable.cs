using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Functions
{
    class ComputeVariable
    {
        public class ParameterSet
        {
            public ComputeParameter Instance { get; set; }
            public Infomations Infomation { get; set; }
            public class Infomations
            {
                public int ArrayLength { get; private set; }
                public int Channels { get; private set; }
                public int Width { get; private set; }
                public int Height { get; private set; }
                public int AreaSize { get; private set; }
                public int CenterX { get; private set; }
                public int CenterY { get; private set; }

                public Type Type { get; private set; }
                public Infomations(RNdObject obj)
                {
                    Type = obj.GetType();
                    TypeofRNdArray(obj);
                    TypeofRNdMatrix(obj);
                }

                private void TypeofRNdArray(RNdObject obj)
                {
                    if (Type != typeof(RNdArray)) { return; }
                    var array = (RNdArray)obj;
                    ArrayLength = array.Length;
                    AreaSize = ArrayLength;
                    Channels = 1;
                    Width = 1;
                    Height = ArrayLength;
                    CenterX = Width / 2;
                    CenterY = Height / 2;
                }

                private void TypeofRNdMatrix(RNdObject obj)
                {
                    if (Type != typeof(RNdMatrix)) { return; }
                    var matrix = (RNdMatrix)obj;
                    AreaSize = matrix.AreaSize;
                    Channels = matrix.Channels;
                    Width = matrix.Width;
                    Height = matrix.Height;
                    ArrayLength = Channels * AreaSize;
                    CenterX = Width / 2;
                    CenterY = Height / 2;
                }
            }
        }

        public ComputeVariable()
        {
            Parameter = new List<ParameterSet>();
        }

        public List<ParameterSet> Parameter { get; set; }

        public ParameterSet this[int index]
        {
            get { return Parameter[index]; }
            set { Parameter[index] = value; }
        }
        
        public void Add(string name, RNdArray array, ComputeParameter.MemoryModeSet mode)
        {
            Parameter.Add(new ParameterSet()
            {
                Instance = new ComputeParameter(name, array, mode),
                Infomation = new ParameterSet.Infomations(array),
            });
        }
    }
}

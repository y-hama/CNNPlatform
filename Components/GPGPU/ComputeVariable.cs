using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU
{
    public abstract class ComputeVariable
    {
        #region InnerClass
        public class ParameterSet
        {
            public Function.ComputeParameter Instance { get; set; }
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
                    ParamOfRNdArray(obj);
                    ParamOfRNdMatrix(obj);
                }

                private void ParamOfRNdArray(RNdObject obj)
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

                private void ParamOfRNdMatrix(RNdObject obj)
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

        public class ValueSet
        {
            public string Name { get; private set; }
            public Real Value { get; set; }
            public ValueSet(string name) { Name = name; Value = 0; }
            public ValueSet(string name, Real value) { Name = name; Value = value; }
        }
        #endregion

        #region Parameter
        internal List<ValueSet> Argument { get; set; }
        internal List<ParameterSet> Parameter { get; set; }
        #endregion

        public ComputeVariable()
        {
            Argument = new List<ValueSet>();
            Parameter = new List<ParameterSet>();
        }

        public ParameterSet this[int index]
        {
            get { return Parameter[index]; }
            set { Parameter[index] = value; }
        }

        public ValueSet this[string valuename]
        {
            get
            {
                for (int i = 0; i < Argument.Count; i++)
                {
                    if (Argument[i].Name == valuename) { return Argument[i]; }
                }
                return null;
            }
            set
            {
                for (int i = 0; i < Argument.Count; i++)
                {
                    if (Argument[i].Name == valuename) { Argument[i].Value = value.Value; }
                }
            }
        }

        public void Add(string name, RNdObject array, Components.State.MemoryModeSet mode)
        {
            Parameter.Add(new ParameterSet()
            {
                Instance = new Function.ComputeParameter(name, array.Data, mode),
                Infomation = new ParameterSet.Infomations(array),
            });
        }

        protected abstract void ConfirmField(object shared);
        public ComputeVariable Confirm(object shared)
        {
            ConfirmField(shared);
            return this;
        }

        public abstract void UpdateParameter(object parameter);
        
    }
}

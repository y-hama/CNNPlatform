using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components.GPGPU.Function
{
    public abstract class SourceCode : FunctionObjectBase
    {
        protected enum ObjectType
        {
            Value,
            Array,
        }
        protected enum ElementType
        {
            INT,
            FLOAT,
        }
        protected enum ReturnType
        {
            INT,
            FLOAT,
            VOID,
        }
        private class ParameterSet
        {
            public string Name { get; set; }
            public ObjectType ObjectType { get; set; }
            public ElementType ElementType { get; set; }

            private const string ArgumentFormat = @"{0} {1}";
            public string Argument
            {
                get
                {
                    string cd = string.Empty;
                    string tp = string.Empty;
                    switch (ElementType)
                    {
                        case ElementType.INT:
                            tp = "int";
                            break;
                        case ElementType.FLOAT:
                            tp = "float";
                            break;
                        default:
                            break;
                    }
                    switch (ObjectType)
                    {
                        case ObjectType.Array:
                            tp = "__global " + tp + "*";
                            break;
                        case ObjectType.Value:
                            break;
                        default:
                            break;
                    }
                    cd = string.Format(ArgumentFormat, tp, Name);
                    return cd;
                }
            }
        }

        #region Property
        private List<ParameterSet> pList = new List<ParameterSet>();
        private const string HeaderFormat = @"{0} {1} {2}({3})";
        protected const string GET_GLOBAL_ID_FORMAT = @"int i{0} = get_global_id({1});";
        private const int MAX_GLOBAL_ID_COUNT = 3;
        private string argumentstring { get; set; }
        private string bodystring { get; set; }
        private string sourcestring { get; set; }

        public string Source
        {
            get
            {
                if (sourcestring == null || sourcestring == string.Empty) { CreateSourceInterface(); }
                return sourcestring.Replace("\r", "").Replace("\n", "");
            }
        }
        #endregion

        public SourceCode()
        {
            ParameterConfigration();
        }

        private void CreateSourceInterface()
        {
            CreateSource();
            CreateArguments();
            if (Name == "SpikeNeuronInit_Source")
            {

            }
            string access = IsLocalFunction ? "" : "__kernel";
            string type = IsLocalFunction ? Return.ToString().ToLower() : "void";
            sourcestring = string.Format(HeaderFormat,
                          access,
                          type,
                          Name,
                          argumentstring)
                          + "{" +
                          bodystring
                          + "}";
        }

        private void CreateArguments()
        {
            argumentstring = string.Empty;
            for (int i = 0; i < pList.Count; i++)
            {
                argumentstring += pList[i].Argument;
                if (i != pList.Count - 1)
                {
                    argumentstring += ",";
                }
            }
        }

        #region Abstruct/Virtual
        public virtual string Name { get { return this.GetType().Name; } }
        protected virtual bool IsLocalFunction { get { return false; } }
        protected virtual ReturnType Return { get { return ReturnType.VOID; } }
        protected abstract void ParameterConfigration();
        protected abstract void CreateSource();
        #endregion

        #region ProtectedMethod
        protected void GlobalID(int count)
        {
            count = Math.Min(count, MAX_GLOBAL_ID_COUNT);
            string temporary = string.Empty;
            for (int i = 0; i < count; i++)
            {
                temporary += string.Format(GET_GLOBAL_ID_FORMAT, i, i);
            }
            bodystring = temporary + bodystring;
        }

        protected void AddParameter(string name, ObjectType otype, ElementType etype)
        {
            pList.Add(new ParameterSet() { Name = name, ObjectType = otype, ElementType = etype });
        }

        protected void AddMethodBody(string lines)
        {
            bodystring += lines.Replace("\r", "").Replace("\n", "");
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cloo;

namespace Components.GPGPU.Function
{
    public abstract class FunctionBase : FunctionObjectBase
    {
        #region InnerClass
        protected enum ParamMode
        {
            Memory,
            Value,
        }
        protected enum ValueMode
        {
            INT,
            FLOAT,
        }
        protected class GpuParamSet
        {
            public object Instance { get; set; }
            public ParamMode ParamMode { get; private set; }
            public ValueMode ValueMode { get; private set; }
            public GpuParamSet(ComputeBuffer<Real> instance, ParamMode mode = FunctionBase.ParamMode.Memory)
            {
                ParamMode = mode;
                Instance = instance;
            }
            public GpuParamSet(object instance, ValueMode vmode, ParamMode mode = FunctionBase.ParamMode.Value)
            {
                ParamMode = mode;
                Instance = instance;
                ValueMode = vmode;
            }
        }

        protected class ComputeBufferSet : IDisposable
        {
            public ComputeBuffer<Real> Buffer { get; set; }
            public ComputeBufferSet(ComputeParameter parmeter, List<ComputeContext> Context, int sellectionIndex)
            {
                Buffer = new ComputeBuffer<Real>(Context[sellectionIndex], parmeter.MemoryMode, parmeter.Array);
            }

            public void Dispose()
            {
                Buffer.Dispose();
            }
        }
        #endregion

        #region Constructor
        public FunctionBase()
        {
            CreateGpuSource();
            FunctionConfiguration();
        }
        #endregion

        #region Property
        protected List<SourceCode> GpuSource { get; set; }
        private List<List<GpuParamSet>> GpuParameter { get; set; }

        public bool IsGpuProcess
        {
            get
            {
                if (!Core.Instance.UseGPU) { return false; }
                else if (GpuSource == null) { return false; }
                else if (GpuSource.Count == 0) { return false; }
                else { return true; }
            }
        }

        public TimeSpan StepElapsedSpan { get; private set; }

        protected Function ProcessFunction;
        private List<ComputeContext> Context { get; set; }
        private List<ComputeKernel> Kernel { get; set; }
        private List<ComputeCommandQueue> Queue { get; set; }
        private int sellectionIndex { get; set; }
        protected int Sellection { set { sellectionIndex = value; } }
        protected bool SwitchSellection(string methodname)
        {
            int idx = GpuSource.IndexOf(GpuSource.Find(x => x.Name == methodname));
            if (idx >= 0) { sellectionIndex = idx; return true; }
            else { return false; }
        }

        private List<ComputeVariable.ParameterSet> _cParam { get; set; }

        private int BufferNNameIndex { get; set; } = 0;
        #endregion

        #region Abstruct/Vitrual
        protected abstract void CreateGpuSource();

        protected abstract void ConvertVariable(ComputeVariable _variable);

        protected delegate void Function();
        protected abstract void CpuFunction();
        protected abstract void GpuFunction();

        protected virtual void UpdateWithCondition() { }
        #endregion

        #region PrivateMethod
        #endregion

        #region ProtectedMethod
        protected void AddSource(SourceCode code)
        {
            if (GpuSource == null) { GpuSource = new List<SourceCode>(); }
            GpuSource.Add(code);
        }

        protected ComputeBufferSet ConvertBuffer(Real[] data, string name = "", State.MemoryModeSet mode = Components.State.MemoryModeSet.WriteOnly)
        {
            string _name = string.Empty;
            if (name != string.Empty) { _name = name; }
            else { BufferNNameIndex++; _name = "buf" + BufferNNameIndex.ToString(); }
            return new ComputeBufferSet(
                new ComputeParameter(_name, data, mode),
                Context, sellectionIndex);
        }

        protected void ClearGpuParameter()
        {
            if (GpuParameter != null)
            {
                BufferNNameIndex = 0;
                for (int i = 0; i < GpuParameter.Count; i++)
                {
                    if (GpuParameter[i] != null)
                    {
                        GpuParameter[i].Clear();
                    }
                }
            }
        }

        protected void SetBuffer(ComputeParameter buffer, Real[] data)
        {
            buffer.Array = (Real[])data.Clone();
        }

        protected void SetParameter(ComputeBufferSet instance, ParamMode mode = FunctionBase.ParamMode.Memory)
        {
            GpuParameter[sellectionIndex].Add(new GpuParamSet(instance.Buffer, mode));
        }
        protected void SetParameter(object instance, ValueMode vmode, ParamMode mode = FunctionBase.ParamMode.Value)
        {
            GpuParameter[sellectionIndex].Add(new GpuParamSet(instance, vmode, mode));
        }

        protected void ReadBuffer(ComputeBufferSet mem, ref Real[] buffer)
        {
            Queue[sellectionIndex].ReadFromBuffer(mem.Buffer, ref buffer, true, null);
        }

        protected void Execute(params long[] globalworksize)
        {
            for (int i = 0; i < GpuParameter[sellectionIndex].Count; i++)
            {
                switch (GpuParameter[sellectionIndex][i].ParamMode)
                {
                    case ParamMode.Memory:
                        Kernel[sellectionIndex].SetMemoryArgument(i, (ComputeBuffer<Real>)GpuParameter[sellectionIndex][i].Instance);
                        break;
                    case ParamMode.Value:
                        switch (GpuParameter[sellectionIndex][i].ValueMode)
                        {
                            case ValueMode.INT:
                                Kernel[sellectionIndex].SetValueArgument(i, (int)GpuParameter[sellectionIndex][i].Instance);
                                break;
                            case ValueMode.FLOAT:
                                Kernel[sellectionIndex].SetValueArgument(i, (float)GpuParameter[sellectionIndex][i].Instance);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            Queue[sellectionIndex].Execute(Kernel[sellectionIndex], null, globalworksize, null, null);
            Queue[sellectionIndex].Finish();
        }
        #endregion

        #region PublicMethod
        protected void FunctionConfiguration()
        {
            if (IsGpuProcess)
            {
                Context = new List<ComputeContext>();
                Kernel = new List<ComputeKernel>();
                Queue = new List<ComputeCommandQueue>();
                GpuParameter = new List<List<GpuParamSet>>();
                var option = GPGPU.Core.Instance.GetOption(GpuSource);
                foreach (var item in option)
                {
                    Context.Add(item.Context);
                    Kernel.Add(item.Kernel);
                    Queue.Add(item.Queue);
                }
                for (int i = 0; i < option.Count; i++)
                {
                    GpuParameter.Add(new List<GpuParamSet>());
                }
                ProcessFunction = GpuFunction;
            }
            else
            {
                ProcessFunction = CpuFunction;
            }
        }

        public List<Tuple<string, string>> GetSourceList()
        {
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            if (IsGpuProcess)
            {
                foreach (var item in GpuSource)
                {
                    list.Add(new Tuple<string, string>(item.Name, item.Source));
                }
            }
            return list;
        }

        public void Do(ComputeVariable variable)
        {
            DateTime start = DateTime.Now;
            try
            {
                _cParam = variable.Parameter;
                ConvertVariable(variable);
                ProcessFunction();
                UpdateWithCondition();

                ClearGpuParameter();
            }
            catch (Exception ex)
            {
                State.ExceptionState(ex);
            }
            StepElapsedSpan = (DateTime.Now - start);
        }
        #endregion
    }
}

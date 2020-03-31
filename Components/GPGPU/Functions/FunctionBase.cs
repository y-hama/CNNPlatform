using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cloo;

namespace Components.GPGPU.Functions
{
    abstract class FunctionBase : FunctionObjectBase
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
        #endregion

        #region Constructor
        public FunctionBase()
        {
            CreateGpuSource();
        }
        #endregion

        #region Property
        private List<SourceCode> GpuSource { get; set; }
        private List<List<GpuParamSet>> GpuParameter { get; set; }

        public bool IsGpuProcess
        {
            get
            {
                if (GpuSource == null) { return false; }
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
        #endregion

        #region Abstruct/Vitrual
        protected abstract void CreateGpuSource();

        protected delegate void Function(ComputeVariable variable);
        protected abstract void CpuFunction(ComputeVariable variable);
        protected abstract void GpuFunction(ComputeVariable variable);

        protected virtual void UpdateWithCondition(bool ForceUpdate, ComputeVariable variable) { }
        #endregion

        #region PrivateMethod
        #endregion

        #region ProtectedMethod
        protected void AddSource(SourceCode code)
        {
            if (GpuSource == null) { GpuSource = new List<SourceCode>(); }
            GpuSource.Add(code);
        }

        protected ComputeBuffer<Real> ConvertBuffer(ComputeParameter parmeter)
        {
            return new ComputeBuffer<Real>(Context[sellectionIndex], parmeter.MemoryMode, parmeter.Array.Data);
        }

        protected void ClearGpuParameter()
        {
            if (GpuParameter != null)
            {
                for (int i = 0; i < GpuParameter.Count; i++)
                {
                    if (GpuParameter[i] != null)
                    {
                        GpuParameter[i].Clear();
                    }
                }
            }
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
        public void FunctionConfiguration()
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

        protected void SetBuffer(ComputeParameter buffer, Real[] data)
        {
            buffer.Array.Data = (Real[])data.Clone();
        }

        protected void SetParameter(ComputeBuffer<Real> instance, ParamMode mode = FunctionBase.ParamMode.Memory)
        {
            GpuParameter[sellectionIndex].Add(new GpuParamSet(instance, mode));
        }
        protected void SetParameter(object instance, ValueMode vmode, ParamMode mode = FunctionBase.ParamMode.Value)
        {
            GpuParameter[sellectionIndex].Add(new GpuParamSet(instance, vmode, mode));
        }

        protected void ReadBuffer(ComputeBuffer<Real> mem, ref Real[] buffer)
        {
            Queue[sellectionIndex].ReadFromBuffer(mem, ref buffer, true, null);
        }

        public void Do(bool ForceUpdate, ComputeVariable variable)
        {
            DateTime start = DateTime.Now;
            try
            {
                _cParam = variable.Parameter;
                ProcessFunction(variable);
                UpdateWithCondition(ForceUpdate, variable);

                ClearGpuParameter();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            StepElapsedSpan = (DateTime.Now - start);
        }
        #endregion
    }
}

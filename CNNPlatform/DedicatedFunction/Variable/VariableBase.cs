using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.DedicatedFunction.Variable
{
    abstract class VariableBase : Components.GPGPU.ComputeVariable
    {
        public string GetSizeStatus
        {
            get
            {
                return string.Format("(c:{0}, w:{1}, h:{2}) -> (c:{3}, w:{4}, h:{5})",
                    InputChannels, InWidth, InHeight, OutputChannels, OutWidth, OutHeight);
            }
        }

        public string GetCaption
        {
            get
            {
                return string.Format("{0}", this.GetType().Name.Substring(0, 2));
            }
        }

        public bool UpdateRequest { get; set; } = false;

        public int BatchCount { get; set; }

        public int InWidth { get; set; }
        public int InHeight { get; set; }
        public int InputChannels { get; set; }

        public int OutWidth { get; set; } = 0;
        public int OutHeight { get; set; } = 0;
        public int OutputChannels { get; set; }

        public int InSize { get { return InWidth * InHeight; } }
        public int InArea { get { return InputChannels * InSize; } }
        public int InTotal { get { return BatchCount * InArea; } }

        public int OutSize { get { return OutWidth * OutHeight; } }
        public int OutArea { get { return OutputChannels * OutSize; } }
        public int OutTotal { get { return BatchCount * OutArea; } }


        public Components.RNdMatrix Input;
        public Components.RNdMatrix Output;

        public Components.RNdMatrix Sigma;
        public Components.RNdMatrix Propagator;

        protected bool ObjectDecoded { get; set; } = false;

        public Components.Real[] Error { get; private set; } = new Components.Real[8];

        protected abstract void ConfirmField(object shared);
        public VariableBase Confirm(object shared)
        {
            ConfirmField(shared);
            Input = new Components.RNdMatrix(BatchCount, InputChannels, InWidth, InHeight);
            Output = new Components.RNdMatrix(BatchCount, OutputChannels, OutWidth, OutHeight);
            Sigma = new Components.RNdMatrix(BatchCount, OutputChannels, OutWidth, OutHeight);
            Propagator = new Components.RNdMatrix(BatchCount, InputChannels, InWidth, InHeight);
            return this;
        }

        public abstract string GetStatus { get; }

        public abstract void UpdateParameter(object parameter);
        public abstract void OverwriteParameter(ref object parameter);

        protected abstract void EncodeParameterCore(ref string res);
        public string EncodeParameter()
        {
            string res = string.Empty;
            res += InWidth.ToString() + " ";
            res += InHeight.ToString() + " ";
            res += InputChannels.ToString() + " ";
            res += OutWidth.ToString() + " ";
            res += OutHeight.ToString() + " ";
            res += OutputChannels.ToString() + " ";
            EncodeParameterCore(ref res);
            return res;
        }
        public abstract string EncodeOption();
        public abstract void SaveObject(System.IO.DirectoryInfo location);

        protected abstract void DecodeParameterCore(object[] values);
        protected abstract void DecodeOption(List<object> values);

        public VariableBase Decode(string location, string text, int batchcount)
        {
            string[] split = text.Split(new string[] { ";>" }, StringSplitOptions.RemoveEmptyEntries);

            List<string> variablebaseparam = new List<string>(split[0].Split(' '));
            variablebaseparam.RemoveAll(x => x == "\n");
            int it;
            float f;

            #region BaseParameter
            BatchCount = batchcount;
            if (int.TryParse(variablebaseparam[0], out it))
            {
                InWidth = it;
            }
            if (int.TryParse(variablebaseparam[1], out it))
            {
                InHeight = it;
            }
            if (int.TryParse(variablebaseparam[2], out it))
            {
                InputChannels = it;
            }
            if (int.TryParse(variablebaseparam[3], out it))
            {
                OutWidth = it;
            }
            if (int.TryParse(variablebaseparam[4], out it))
            {
                OutHeight = it;
            }
            if (int.TryParse(variablebaseparam[5], out it))
            {
                OutputChannels = it;
            }
            #endregion

            #region ExternalParameter
            int offset = 6;
            object[] ext = new object[variablebaseparam.Count - offset];
            for (int i = offset; i < variablebaseparam.Count; i++)
            {
                if (float.TryParse(variablebaseparam[i], out f))
                {
                    ext[i - offset] = f;
                }
                else
                {
                    ext[i - offset] = variablebaseparam[i];
                }
            }
            DecodeParameterCore(ext);
            #endregion

            var hash = split[1].Replace("\r", "").Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<object> loadbuffer = new List<object>();
            var dinfo = new System.IO.DirectoryInfo(location);
            for (int i = 0; i < hash.Length; i++)
            {
                loadbuffer.Add((new Components.RNdMatrix()).Load(dinfo, hash[i]));
            }
            DecodeOption(loadbuffer);
            ObjectDecoded = true;
            return this;
        }

        public abstract void CoreClone(ref VariableBase _clone);

        public virtual void CalcurationError(ref Components.Real[] _error)
        {
            var error = _error;
            double err = 0, sd = 0;

            var ave = Sigma.Data.Average(x => x);

            Tasks.ForParallel(0, Sigma.Length, i0 =>
            {
                err += Sigma.Data[i0] * Sigma.Data[i0];
                sd += (Sigma.Data[i0] - ave) * (Sigma.Data[i0] - ave);
            });
            err /= Sigma.Data.Length;
            sd /= Sigma.Data.Length;
            sd = Math.Sqrt(error[1]);

            error[0] = err;
            error[1] = sd;
        }

        public VariableBase Clone()
        {
            var clone = (VariableBase)Activator.CreateInstance(this.GetType());
            clone.BatchCount = BatchCount;
            clone.InWidth = InWidth;
            clone.InHeight = InHeight;
            clone.InputChannels = InputChannels;
            clone.OutWidth = OutWidth;
            clone.OutHeight = OutHeight;
            clone.OutputChannels = OutputChannels;
            CoreClone(ref clone);
            return clone;
        }
    }
}

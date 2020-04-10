using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Layer
{
    class LayerBase
    {
        public DedicatedFunction.Variable.VariableBase Variable { get; set; }

        public Components.GPGPU.Function.FunctionBase ForwardFunction { get; set; }
        public Components.GPGPU.Function.FunctionBase BackFunction { get; set; }

        public string ParameterStatus { get { return Variable.GetStatus; } }

        public string Encode()
        {
            string res = "!!>\n";
            res += this.GetType().ToString() + "\n";
            res += ForwardFunction.GetType().ToString() + "\n";
            res += BackFunction.GetType().ToString() + "\n";
            res += Variable.GetType().ToString() + "\n";
            res += "!!!>\n";
            res += ":>" + Variable.EncodeParameter() + "\n";
            res += ";>\n" + Variable.EncodeOption() + "\n";

            return res + "\n";
        }

        public LayerBase Decode(string text)
        {
            return null;
        }
    }
}

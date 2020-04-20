using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Layer
{
    class LayerBase
    {
        public enum DirectionPattern
        {
            Through,
            TurnBack,
        }

        public DedicatedFunction.Variable.VariableBase Variable { get; set; }

        public Components.GPGPU.Function.FunctionBase ForwardFunction { get; set; }
        public Components.GPGPU.Function.FunctionBase BackFunction { get; set; }

        public DirectionPattern Direction { get; set; } = DirectionPattern.Through;

        public string ParameterStatus { get { return Variable.GetStatus + " " + Variable.GetSizeStatus; } }

        public string Encode()
        {
            string res = "!!>\n";
            res += this.GetType().ToString() + "\n";
            res += Direction.ToString() + "\n";
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

        public LayerBase Clone()
        {
            var clone = (LayerBase)Activator.CreateInstance(this.GetType());
            clone.Direction = Direction;
            clone.ForwardFunction = (Components.GPGPU.Function.FunctionBase)Activator.CreateInstance(ForwardFunction.GetType());
            clone.BackFunction = (Components.GPGPU.Function.FunctionBase)Activator.CreateInstance(BackFunction.GetType());
            clone.Variable = Variable.Clone();
            return clone;
        }
    }
}

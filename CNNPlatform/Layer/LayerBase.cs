using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Layer
{
    abstract class LayerBase
    {
        public DedicatedFunction.Variable.VariableBase Variable { get; set; }

        public Components.GPGPU.Function.FunctionBase ForwardFunction { get; set; }
        public Components.GPGPU.Function.FunctionBase BackFunction { get; set; }

        public int Block { get; set; } = -1;

        public string ParameterStatus { get { return Variable.GetStatus + " " + Variable.GetSizeStatus; } }

        protected LayerBase(bool createFunctions)
        {
            if (createFunctions)
            {
                FunctionCreator();
            }
        }

        public LayerBase Confirm()
        {
            ForwardFunction.OptionCreater(Variable);
            BackFunction.OptionCreater(Variable);
            return this;
        }

        protected abstract void FunctionCreator();

        public void RefreshError()
        {
            for (int i = 0; i < Variable.Error.Length; i++)
            {
                Variable.Error[i] = 0;
            }
        }

        public string Encode()
        {
            string res = "!!!!!!!!!!>";
            res += this.GetType().ToString() + "\n";
            res += Block.ToString() + "\n";
            res += Variable.GetType().ToString() + "\n";
            res += "!!!!!>";
            res += Variable.EncodeParameter() + "\n";
            res += ";>" + Variable.EncodeOption() + "\n";

            return res + "\n";
        }

        public void Encode(ref Components.Locker.TagFileController.TagSegment container)
        {
            var tag = container.AddTag(this.GetType().ToString());
            tag.AddValue("Block", Block);
            tag.AddValue("VariableType", Variable.GetType().ToString());

            var vartag = tag.AddTag("Variable");
            Variable.EncodeParameter(ref vartag);
        }

        public static LayerBase Decode(string location, string text, Utility.Shared.ModelParameter instance, int batchcount)
        {
            var split = text.Split(new string[] { "!!!!!>" }, StringSplitOptions.RemoveEmptyEntries);
            LayerBase layer;
            var layerbaseparam = split[0];
            #region
            var lbparams = layerbaseparam.Split('\n');
            layer = (LayerBase)Activator.CreateInstance(Type.GetType(lbparams[0]), new object[] { true });
            int block;
            if (int.TryParse(lbparams[1], out block))
            {
                layer.Block = block;
            }
            #endregion

            var variableparam = split[1];
            var variable = (DedicatedFunction.Variable.VariableBase)Activator.CreateInstance(Type.GetType(lbparams[2]));
            layer.Variable = variable.Decode(location, variableparam, batchcount).Confirm(instance);

            return layer;
        }

        public LayerBase Clone()
        {
            var clone = (LayerBase)Activator.CreateInstance(this.GetType(), new object[] { false });
            clone.Block = Block;
            clone.Variable = Variable.Clone();
            return clone;
        }
    }
}

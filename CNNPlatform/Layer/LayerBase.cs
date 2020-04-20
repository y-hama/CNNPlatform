﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Layer
{
    abstract class LayerBase
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

        protected LayerBase(bool createFunctions)
        {
            if (createFunctions)
            {
                FunctionCreator();
            }
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
            res += Direction.ToString() + "\n";
            res += Variable.GetType().ToString() + "\n";
            res += "!!!!!>";
            res += Variable.EncodeParameter() + "\n";
            res += ";>" + Variable.EncodeOption() + "\n";

            return res + "\n";
        }

        public static LayerBase Decode(string location, string text, Utility.Shared.ModelParameter instance, int batchcount)
        {
            var split = text.Split(new string[] { "!!!!!>" }, StringSplitOptions.RemoveEmptyEntries);
            LayerBase layer;
            var layerbaseparam = split[0];
            #region
            var lbparams = layerbaseparam.Split('\n');
            layer = (LayerBase)Activator.CreateInstance(Type.GetType(lbparams[0]), new object[] { true });
            layer.Direction = (DirectionPattern)Enum.Parse(typeof(DirectionPattern), lbparams[1]);
            #endregion

            var variableparam = split[1];
            var variable = (DedicatedFunction.Variable.VariableBase)Activator.CreateInstance(Type.GetType(lbparams[2]));
            layer.Variable = variable.Decode(location, variableparam, batchcount).Confirm(instance);

            return layer;
        }

        public LayerBase Clone()
        {
            var clone = (LayerBase)Activator.CreateInstance(this.GetType(), new object[] { false });
            clone.Direction = Direction;
            clone.Variable = Variable.Clone();
            return clone;
        }
    }
}

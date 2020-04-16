using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CNNPlatform.Utility;
using CNNPlatform.Utility.Shared;

namespace CNNPlatform.Model
{
    class Creater
    {
        private Creater() { }
        public static Creater Core { get; private set; } = new Creater();

        public ModelParameter Instance { get; set; } = null;

        public Model TestModel()
        {
            var model = new CNNPlatform.Model.Model(Instance, 8, 3, 64, 64);
            model.AddConvolution(6, 1, 1, Types.Optimizer.AdaSelf);
            model.AddPooling(2, 1);
            model.AddConvolution(12, 1, 1, Types.Optimizer.AdaSelf);
            model.AddPooling(2, 1);
            model.AddConvolution(24, 1, 1, Types.Optimizer.AdaSelf);
            model.AddActivation(Types.Activator.Mish);
            model.AddPooling(2, 1);
            model.AddConvolution(48, 1, 1, Types.Optimizer.AdaSelf);
            model.AddPooling(2, 1);
            model.AddConvolution(96, 1, 1, Types.Optimizer.AdaSelf);
            model.AddAffine(1000, Types.Optimizer.AdaSelf);
            model.AddActivation(Types.Activator.ReLU);
            model.AddAffine(300, Types.Optimizer.AdaSelf);
            model.AddActivation(Types.Activator.ReLU);

            model.AddReverse();
            return model;
        }
    }
}

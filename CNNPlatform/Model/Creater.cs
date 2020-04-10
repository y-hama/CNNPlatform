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

        public Model TestModel(int batchcount = 1)
        {
            var model = new CNNPlatform.Model.Model(Instance, batchcount, 80, 60, 3);

            //model.AddConvolution(6, 1, 1, Types.Optimizer.AdaSelf);
            //model.AddPooling(4, 1);
            //model.AddConvolution(12, 1, 1, Types.Optimizer.AdaSelf);
            //model.AddPooling(2, 1);

            //model.AddConvolution(12, 1, 1, Types.Optimizer.AdaSelf);

            model.AddConvolution(3, 1, 2, Types.Optimizer.AdaSelf, 4);
            //model.AddActivation(Types.Activator.Mish);

            model.AddConvolution(3, 1, 2, Types.Optimizer.AdaSelf);
            model.AddConvolution(3, 1, 1, Types.Optimizer.AdaSelf, 0.25);
            //model.AddActivation(Types.Activator.Mish);
            return model;
        }
    }
}

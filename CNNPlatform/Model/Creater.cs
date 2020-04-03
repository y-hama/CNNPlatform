using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var model = new CNNPlatform.Model.Model(batchcount, 320, 240, 3);
            model.AddConvolution(Instance, outch: 6, kernelsize: 2, expand: 1);
            model.AddConvolution(Instance, outch: 3, kernelsize: 1, expand: 2);

            return model;
        }
    }
}

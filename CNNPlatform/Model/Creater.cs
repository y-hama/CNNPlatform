using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Model
{
    class Creater
    {
        private Creater() { }
        public static Creater Core { get; private set; } = new Creater();

        public SharedObject Instance { get; set; } = null;

        public Model TestModel(int batchcount = 1)
        {
            var model = new CNNPlatform.Model.Model();
            model.Layer.Add(new Layer.Convolution()
            {
                Variable = new Function.Variable.ConvolutionValiable()
                {
                    BatchCount = batchcount,
                    InputChannels = 3,
                    InWidth = 160,
                    InHeight = 120,

                    OutputChannels = 3,
                    OutScale = 1,

                    KernelSize = 1,
                    KernelExpand = 2,
                    Rho = 0.00001,
                }.Confirm(Instance),
            });
            return model;
        }
    }
}

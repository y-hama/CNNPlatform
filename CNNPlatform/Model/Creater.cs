﻿using System;
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

        public int BatchCount { get; set; } = 4;

        public Model __BasicImageCreater()
        {
            var model = new CNNPlatform.Model.Model(Instance, BatchCount, 3, 4, 4);

            model.AddAffine(256, Types.Optimizer.Adam);
            model.AddReshape(2, 2);
            model.AddPooling(1, 2);
            model.AddConvolution(48, 1, 1, Types.Optimizer.Adam);
            model.AddPooling(1, 2);
            model.AddConvolution(32, 1, 1, Types.Optimizer.Adam);
            model.AddConvolution(32, 1, 1, Types.Optimizer.Adam);
            model.AddPooling(1, 2);
            model.AddConvolution(16, 1, 1, Types.Optimizer.Adam);
            model.AddPooling(1, 2);
            model.AddConvolution(16, 1, 1, Types.Optimizer.Adam);
            model.AddPooling(1, 2);
            model.AddConvolution(3, 1, 1, Types.Optimizer.Adam);

            //model.AddReverse();
            return model;
        }
        public Model BasicImageCreater()
        {
            var model = new CNNPlatform.Model.Model(Instance, BatchCount, 3, 128, 128);

            model.AddConvolution(16, 1, 1, Types.Optimizer.Adam);
            model.AddPooling(2, 1);
            model.AddConvolution(32, 1, 1, Types.Optimizer.Adam);
            model.AddConvolution(32, 1, 1, Types.Optimizer.Adam);
            model.AddPooling(2, 1);
            model.AddConvolution(48, 1, 1, Types.Optimizer.Adam);
            model.AddPooling(2, 1);
            model.AddConvolution(64, 1, 1, Types.Optimizer.Adam);
            model.AddPooling(2, 1);
            model.AddConvolution(64, 1, 1, Types.Optimizer.Adam);
            model.AddAffine(512, Types.Optimizer.Adam);
            model.AddAffine(256, Types.Optimizer.Adam);
            model.AddAffine(64, Types.Optimizer.Adam);

            model.AddReverse();
            return model;
        }

        public Model test()
        {
            var model = new CNNPlatform.Model.Model(Instance, BatchCount, 3, 8, 8);

            model.AddAffine(100);

            return model;
        }
    }
}

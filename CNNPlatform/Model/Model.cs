using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Model
{
    class Model
    {
        public List<Layer.LayerBase> Layer { get; set; } = new List<CNNPlatform.Layer.LayerBase>();
    }
}

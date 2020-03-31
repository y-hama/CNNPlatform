using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Layer
{
    class Convolution : LayerBase
    {
        public Convolution()
        {
            ForwardFunction = new Function.Process.ConvolutionForward();
            BackFunction = new Function.Process.ConvolutionBack();
        }
    }
}

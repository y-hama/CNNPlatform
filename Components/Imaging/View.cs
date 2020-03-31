using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Runtime.InteropServices;

namespace Components.Imaging
{
    public class View
    {
        public static void Show(RNdMatrix mat, string title = "window")
        {
            Mat[] frame;
            Converter.RNdMatrixToMat(mat, out frame);

            Cv2.ImShow(title, frame[0]);
            Cv2.WaitKey(1);
        }
    }
}

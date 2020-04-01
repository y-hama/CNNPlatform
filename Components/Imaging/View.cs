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
        public static void Show(RNdMatrix mat, string title = "window", int locx = -1, int locy = -1)
        {
            Mat[] frame;
            Converter.RNdMatrixToMat(mat, out frame);

            Cv2.ImShow(title, frame[0]);
            if (locx >= 0 && locy >= 0)
            {
                Cv2.MoveWindow(title, locx, locy);
            }
            Cv2.WaitKey(1);
        }
    }
}

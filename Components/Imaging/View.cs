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
        private static Mat FrameConverter(RNdMatrix[] mats, int resultwidth = -1, int resultheight = -1)
        {
            int count = mats.Length;
            int tilec = (int)Math.Ceiling(Math.Sqrt(count));
            int width = mats.Select(x => x.Width).Max();
            int height = mats.Select(x => x.Height).Max();

            Mat frame = new Mat(new Size(width * tilec, height * tilec), MatType.CV_8UC3, new Scalar(0));

            for (int i = 0; i < count; i++)
            {
                int yi = i / tilec;
                int xi = i - yi * tilec;
                Mat[] tmps;
                Converter.RNdMatrixToMat(mats[i], out tmps);
                Mat tmpf = tmps[0].Clone();
                if (tmpf.Channels() != 3)
                {
                    Cv2.CvtColor(tmpf, tmpf, ColorConversionCodes.GRAY2BGR);
                }
                int offsetx = (width - mats[i].Width) / 2;
                int offsety = (height - mats[i].Height) / 2;
                frame[new Rect(new Point(xi * width + offsetx, yi * height + offsety), new Size(mats[i].Width, mats[i].Height))] = tmpf;
            }

            int w, h;
            w = resultwidth > 0 ? resultwidth : width;
            h = resultheight > 0 ? resultheight : height;
            frame = frame.Resize(new Size(w, h));
            return frame;
        }

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

        public static void Show(RNdMatrix[] mats, string title = "window", int locx = -1, int locy = -1)
        {
            var frame = FrameConverter(mats);
            Cv2.ImShow(title, frame);
            if (locx >= 0 && locy >= 0)
            {
                Cv2.MoveWindow(title, locx, locy);
            }
            Cv2.WaitKey(1);
        }

        public static RNdMatrix ConvertToShow(RNdMatrix[] mats, int width, int height)
        {
            RNdMatrix mat;
            Imaging.Converter.MatToRNdMatrix(new Mat[] { FrameConverter(mats, width, height) }, out mat);
            return mat;
        }
    }
}

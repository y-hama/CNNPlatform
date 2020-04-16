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
            int height = mats.Select(x => x.Width > 1 ? x.Height : 0).Max();

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
                if (tmpf.Height > height)
                {
                    Cv2.Resize(tmpf, tmpf, new Size(tmpf.Width, height), 0, 0, InterpolationFlags.Area);
                }
                int offsetx = (width - tmpf.Width) / 2;
                int offsety = (height - tmpf.Height) / 2;
                frame[new Rect(new Point(xi * width + offsetx, yi * height + offsety), new Size(tmpf.Width, tmpf.Height))] = tmpf;
            }

            int w, h;
            w = resultwidth > 0 ? resultwidth : width;
            h = resultheight > 0 ? resultheight : height;
            frame = frame.Resize(new Size(w, h), 0, 0, InterpolationFlags.Area);
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

        public static void Show(Real[] data, int[] shape, string title = "window", int locx = -1, int locy = -1)
        {
            var mat = new Components.RNdMatrix(shape) { Data = data };
            Show(mat, title, locx, locy);
        }

        public static RNdMatrix ConvertToResultImage(RNdMatrix[] mats, int width = 0, int height = 0)
        {
            RNdMatrix mat;
            Imaging.Converter.MatToRNdMatrix(new Mat[] { FrameConverter(mats, width, height) }, out mat);
            return mat;
        }

        public static RNdMatrix ConvertToProcessImage(List<Components.RNdMatrix> source, double scale = 1)
        {
            int arraywidth = 10;
            int w = 0, h = 0;
            foreach (var item in source)
            {
                w += item.Width == 1 ? arraywidth : (int)(scale * item.Width);
                if (item.Width > 1 && item.Height * item.Channels > h)
                { h = (int)(scale * item.Height * item.Channels); }
            }
            Mat frame = new Mat(new Size(w, h), MatType.CV_8UC3, new Scalar(16, 16, 16));

            int arrayheight = 0;
            foreach (var item in source)
            {
                if (item.Width == 1)
                {
                    if (arrayheight < item.Height * scale)
                    {
                        arrayheight = (int)(item.Height * scale);
                    }
                }
            }
            double arrayscale = (double)h / arrayheight;

            int startw = 0, wt = 0, ht = 0;
            for (int i = 0; i < source.Count; i++)
            {
                var item = source[i];
                wt = (item.Width == 1 ? arraywidth : (int)(scale * item.Width));
                Mat tmp;
                Converter.RNdMatrixToVMat(item, out tmp);
                if (scale != 1)
                {
                    Cv2.Resize(tmp, tmp, new Size(), scale, scale, InterpolationFlags.Area);
                }
                if (tmp.Width != wt)
                {
                    Cv2.Resize(tmp, tmp, new Size(wt, tmp.Height * arrayscale), 0, 0, InterpolationFlags.Area);
                }
                ht = 0;
                if (tmp.Height < frame.Height)
                {
                    ht = (frame.Height - tmp.Height) / 2;
                }
                frame[new Rect(new Point(startw, ht), tmp.Size())] = tmp;
                startw += tmp.Width;
            }

            RNdMatrix mat;
            Converter.MatToRNdMatrix(new Mat[] { frame }, out mat);
            return mat;
        }
    }
}

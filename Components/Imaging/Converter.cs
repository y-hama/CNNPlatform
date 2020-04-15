using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OpenCvSharp;

namespace Components.Imaging
{
    class Converter
    {
        /// <summary>
        /// framesの画像群は全て同サイズ、同チャネル、同Depthであることが前提
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="mat"></param>
        public static void MatToRNdMatrix(Mat[] frames, out RNdMatrix mat)
        {
            // mat[b,c,w,h] = b * () + c * () + w * () + hとなるように並び替え
            if (frames.Count() > 0)
            {
                int batchcount = frames.Count();
                int ch = frames[0].Channels();
                mat = new Components.RNdMatrix(batchcount, ch, frames[0].Width, frames[0].Height);
                int size = (int)frames[0].Total();
                int total = size * frames[0].Channels();
                unsafe
                {
                    byte[] get = new byte[mat.Data.Length];
                    for (int i = 0; i < batchcount; i++)
                    {
                        var cframe = frames[i].Split();
                        for (int c = 0; c < ch; c++)
                        {
                            Marshal.Copy(cframe[c].Data, get, i * total + c * size, size);
                        }
                    }
                    mat.Data = get.Select(x => (Real)((double)x / byte.MaxValue)).ToArray();
                }
            }
            else { mat = null; }
        }
        public static void RNdMatrixToMat(RNdMatrix mat, out Mat[] frames)
        {
            // MatToRNdMatrixの逆変換
            var baseframe = new Mat(mat.Height, mat.Width, MatType.MakeType(MatType.CV_8U, mat.Channels), new Scalar(byte.MinValue));
            var framelist = new List<Mat>();

            int batchsize = mat.BatchSize;
            int ch = mat.Channels;
            int size = mat.Width * mat.Height;
            int total = size * ch;

            var get = mat.Data.Select(x => (byte)(Math.Max(0, Math.Min(1, x)) * byte.MaxValue)).ToArray();

            for (int i = 0; i < batchsize; i++)
            {
                Mat[] cframe = new Mat[ch];
                for (int c = 0; c < ch; c++)
                {
                    cframe[c] = new Mat(mat.Height, mat.Width, MatType.CV_8UC1, new Scalar(byte.MinValue));
                    Marshal.Copy(get, i * total + c * size, cframe[c].Data, size);
                }

                var frame = new Mat();
                Cv2.Merge(cframe.ToArray(), frame);
                framelist.Add(frame);
            }

            frames = framelist.ToArray();
        }

        public static void RNdMatrixToVMat(RNdMatrix mat, out Mat frames)
        {
            int batchsize = mat.BatchSize;
            int ch = mat.Channels;
            int w = mat.Width, h = mat.Height;
            int size = w * h;
            int total = size * ch;
            frames = new Mat(new Size(w, h * ch), MatType.CV_8UC3, new Scalar(byte.MinValue));

            var framelist = new List<Mat>();
            byte[] get_p, get_m;
            var s_p = mat.Data.Select(x => ((x > 0 ? x : 0))).ToArray();
            var s_m = mat.Data.Select(x => ((x < 0 ? -x : 0))).ToArray();

            var max_p = s_p.Max();
            var max_m = s_m.Max();
            var max = Math.Max(max_p, max_m);
            get_p = s_p.Select(x => (byte)((x / max) * byte.MaxValue)).ToArray();
            get_m = s_m.Select(x => (byte)((x / max) * byte.MaxValue)).ToArray();
            //if (max_p > 1 || max_m > 1)
            //{
            //    var max = Math.Max(max_p, max_m);
            //    get_p = s_p.Select(x => (byte)((x / max) * byte.MaxValue)).ToArray();
            //    get_m = s_m.Select(x => (byte)((x / max) * byte.MaxValue)).ToArray();
            //}
            //else
            //{
            //    get_p = s_p.Select(x => (byte)(x * byte.MaxValue)).ToArray();
            //    get_m = s_m.Select(x => (byte)(x * byte.MaxValue)).ToArray();
            //}

            for (int c = 0; c < ch; c++)
            {
                var cframe_t = new Mat(mat.Height, mat.Width, MatType.CV_8UC1, new Scalar(byte.MinValue));
                var cframe_r = cframe_t.Clone();
                var cframe_g = cframe_t.Clone();
                var cframe_b = cframe_t.Clone();

                Marshal.Copy(get_p, c * size, cframe_r.Data, size);
                Marshal.Copy(get_m, c * size, cframe_b.Data, size);

                var cframe = new Mat(mat.Height, mat.Width, MatType.CV_8UC3);
                Cv2.Merge(new Mat[] { cframe_b, cframe_g, cframe_r }, cframe);
                frames[new Rect(new Point(0, c * mat.Height), cframe.Size())] = cframe;
            }
        }
    }
}

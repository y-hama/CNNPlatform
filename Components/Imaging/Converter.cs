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
    }
}

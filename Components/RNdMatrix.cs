using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

//using OpenCvSharp;

namespace Components
{
    [Serializable]
    public class RNdMatrix : RNdObject
    {
        #region Constructor
        private RNdMatrix() { }
        public RNdMatrix(int batchsize, int ch, int wth, int hgt)
        {
            this.Shape = new int[4];
            Shape[0] = batchsize;
            Shape[1] = ch;
            Shape[2] = wth;
            Shape[3] = hgt;

            this.Data = new Real[this.Length];
        }
        public RNdMatrix(int[] shape)
        {
            this.Shape = shape;

            this.Data = new Real[this.Length];
        }
        public RNdMatrix(RNdArray[][] array)
        {
            this.Shape = new int[4];
            Shape[0] = array.Length;
            Shape[1] = array[0].Length;
            Shape[2] = 1;
            Shape[3] = array[0][0].Length;

            this.Data = new Real[this.Length];
            for (int b = 0; b < BatchSize; b++)
            {
                for (int c = 0; c < Channels; c++)
                {
                    System.Array.Copy(array[b][c].Data, 0, this.Data, b * Channels * AreaSize + c * AreaSize, AreaSize);
                }
            }
        }

        //public RNdMatrix(Mat frame)
        //{
        //    this.Shape = new int[4];
        //    Shape[0] = 1;
        //    Shape[1] = frame.Channels();
        //    Shape[2] = frame.Width;
        //    Shape[3] = frame.Height;
        //    Data = MatToArray(frame);
        //}

        //public RNdMatrix(Mat[] frames)
        //{
        //    if (frames == null) { return; }
        //    if (frames.Length == 0) { return; }

        //    Size size = frames[0].Size();
        //    this.Shape = new int[4];
        //    Shape[0] = frames.Length;
        //    Shape[1] = frames[0].Channels();
        //    Shape[2] = size.Width; Shape[3] = size.Height;

        //    Real[][] temporary = new Real[BatchSize][];
        //    for (int b = 0; b < BatchSize; b++)
        //    {
        //        for (int i = 0; i < frames.Length; i++)
        //        {
        //            if (frames[b].Size() != size) { Cv2.Resize(frames[b], frames[b], size); }
        //            temporary[b] = MatToArray(frames[b]);
        //        }
        //    }

        //    Data = new Real[Length];
        //    int pos = 0;
        //    for (int i = 0; i < frames.Length; i++)
        //    {
        //        Array.Copy(temporary[i], 0, Data, pos, temporary[i].Length);
        //        pos += temporary[i].Length;
        //    }
        //}

        //public RNdMatrix(Mat[][] framesArray)
        //{
        //    if (framesArray == null) { return; }
        //    if (framesArray.Length == 0) { return; }

        //    Size size = framesArray[0][0].Size();
        //    this.Shape = new int[4];
        //    Shape[0] = framesArray.Length;
        //    Shape[1] = framesArray[0][0].Channels();
        //    Shape[2] = size.Width; Shape[3] = size.Height;

        //    Real[][][] temporary = new Real[BatchSize][][];
        //    for (int b = 0; b < BatchSize; b++)
        //    {
        //        temporary[b] = new Real[framesArray.Length][];
        //        for (int i = 0; i < framesArray.Length; i++)
        //        {
        //            if (framesArray[b][i].Size() != size) { Cv2.Resize(framesArray[b][i], framesArray[b][i], size); }
        //            temporary[b][i] = MatToArray(framesArray[b][i]);
        //        }
        //    }
        //    Data = new Real[Length];
        //    int pos = 0;
        //    for (int i = 0; i < framesArray.Length; i++)
        //    {
        //        Array.Copy(temporary[i], 0, Data, pos, AreaSize);
        //        pos += temporary[i].Length;
        //    }
        //}
        #endregion

        #region PrivateMethod
        //private Real[] MatToArray(Mat frame)
        //{
        //    int areaSize = frame.Width * frame.Height;
        //    Real[] temporary = new Real[frame.Channels() * frame.Width * frame.Height];
        //    int pos = 0;
        //    Mat[] chs;
        //    Cv2.Split(frame, out chs);
        //    for (int i = 0; i < frame.Channels(); i++)
        //    {
        //        var bytearray = new byte[areaSize]; var realarray = new Real[areaSize];
        //        Marshal.Copy(chs[i].Data, bytearray, 0, frame.Width * frame.Height);
        //        realarray = bytearray.Select(x => (Real)((float)x * 1.0 / 255.0)).ToArray();
        //        Array.Copy(realarray, 0, temporary, pos, areaSize);
        //        pos += areaSize;
        //    }
        //    return temporary;
        //}
        //private Mat ArrayToMat(int batchindex)
        //{
        //    Mat frame = new Mat();
        //    Mat[] chs = new Mat[Channels];
        //    Real[] rla = new Real[AreaSize];
        //    byte[] bta = new byte[AreaSize];
        //    for (int i = 0; i < Channels; i++)
        //    {
        //        chs[i] = Mat.Zeros(new Size(Width, Height), MatType.CV_8UC1);
        //        Array.Copy(Data, batchindex * Channels * AreaSize + i * AreaSize, rla, 0, AreaSize);
        //        bta = rla.Select(x => (byte)(byte.MaxValue * (Math.Abs(x) > 1 ? 1 : Math.Abs(x)))).ToArray();
        //        Marshal.Copy(bta, 0, chs[i].Data, AreaSize);
        //    }
        //    Cv2.Merge(chs, frame);
        //    return frame;
        //}
        #endregion

        public Real this[int b, int c, int x, int y]
        {
            get { return Data[b * Channels * AreaSize + c * AreaSize + y * Width + x]; }
            set { Data[b * Channels * AreaSize + c * AreaSize + y * Width + x] = value; }
        }

        public Real this[int b, int c, int i]
        {
            get { return Data[b * Channels * AreaSize + c * AreaSize + i]; }
            set { Data[b * Channels * AreaSize + c * AreaSize + i] = value; }
        }

        public override RNdObject Clone()
        {
            var matrix = new RNdMatrix() { Data = (Real[])Data.Clone(), Shape = (int[])Shape.Clone() };
            return matrix;
        }

        public override RNdObject Abs()
        {
            var matrix = new RNdMatrix() { Data = ((Real[])Data.Clone()).Select(x => (Real)Math.Abs(x)).ToArray(), Shape = (int[])Shape.Clone() };
            return matrix;
        }

        //public override void Show(string name, int batchindex = 0)
        //{
        //    Cv2.ImShow(name, ArrayToMat(batchindex));
        //    Cv2.WaitKey(1);
        //}

        public static RNdMatrix operator -(RNdMatrix o1, RNdMatrix o2)
        {
            return (RNdMatrix)((RNdObject)o1 - (RNdObject)o2);
        }

        public static RNdMatrix operator +(RNdMatrix o1, RNdMatrix o2)
        {
            return (RNdMatrix)((RNdObject)o1 + (RNdObject)o2);
        }

        public static RNdMatrix operator *(RNdMatrix o1, RNdMatrix o2)
        {
            return (RNdMatrix)((RNdObject)o1 * (RNdObject)o2);
        }

        public static RNdMatrix operator /(RNdMatrix o1, RNdMatrix o2)
        {
            return (RNdMatrix)((RNdObject)o1 / (RNdObject)o2);
        }
    }
}

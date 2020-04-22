using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

//using OpenCvSharp;

namespace Components
{
    [Serializable]
    public class RNdMatrix : RNdObject
    {
        #region Constructor
        public RNdMatrix() { }
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
        internal RNdMatrix(RNdArray[][] array)
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
        #endregion

        public override void Save(DirectoryInfo location)
        {
            string name = location.FullName + @"\" + Hash;
            Locker.FileConverter<RNdMatrix>.Save(this, name, FileType);
        }

        public override RNdObject Load(DirectoryInfo location, string hash)
        {
            string name = location.FullName + @"\" + hash;
            return Locker.FileConverter<RNdMatrix>.Load(name, FileType);
        }

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
            var cl = new Real[Data.Length];
            Array.Copy(Data, 0, cl, 0, Data.Length);
            var matrix = new RNdMatrix() { Hash = this.Hash, Data = cl, Shape = (int[])Shape.Clone() };
            return matrix;
        }

        public override RNdObject Abs()
        {
            var matrix = new RNdMatrix() { Data = ((Real[])Data.Clone()).Select(x => (Real)Math.Abs(x)).ToArray(), Shape = (int[])Shape.Clone() };
            return matrix;
        }

        public void CopyTo(RNdMatrix dist)
        {
            if (IsSimilarity(this, dist))
            {
                Array.Copy(Data, 0, dist.Data, 0, Data.Length);
            }
        }

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

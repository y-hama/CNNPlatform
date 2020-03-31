using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace Components
{
    [Serializable]
    public class RNdArray : RNdObject
    {
        #region Constructor
        private RNdArray() { }
        public RNdArray(int length)
        {
            this.Shape = new int[4];
            this.Shape[0] = 1;
            this.Shape[1] = 1;
            this.Shape[2] = 1;
            this.Shape[3] = length;

            Data = new Real[Length];
        }
        public RNdArray(Array data)
        {
            this.Data = Real.GetArray(data);

            this.Shape = new int[4];
            this.Shape[0] = 1;
            this.Shape[1] = 1;
            this.Shape[2] = 1;
            this.Shape[3] = data.Length;
        }
        public RNdArray(Array[] data)
        {
            Real[][] temporary = new Real[data.Length][];
            int len = 0;
            for (int i = 0; i < data.Length; i++)
            {
                temporary[i] = Real.GetArray(data[i]);
                len += data[i].Length;
            }
            this.Data = new Real[len];
            int pos = 0;
            for (int i = 0; i < data.Length; i++)
            {
                Array.Copy(temporary[i], 0, this.Data, pos, temporary[i].Length);
                pos += temporary[i].Length;
            }
            this.Shape = new int[3];
            this.Shape[0] = data.Length;
            this.Shape[1] = 1;
            this.Shape[2] = 1;
            this.Shape[3] = data[0].Length;
        }
        #endregion

        public override RNdObject Clone()
        {
            var array = new RNdArray() { Data = (Real[])Data.Clone(), Shape = (int[])Shape.Clone() };
            return array;
        }

        public Real this[int idx]
        {
            get { return Data[idx]; }
            set { Data[idx] = value; }
        }

        public Real this[int ch, int idx]
        {
            get { return Data[ch * this.AreaSize + idx]; }
            set { Data[ch * this.AreaSize + idx] = value; }
        }

        public void CopyTo(RNdArray array)
        {
            if (IsSimilarity(this, array))
            {
                for (int i = 0; i < Length; i++)
                {
                    array[i] = this[i];
                }
            }
        }

        public void CopyBy(Real[] array)
        {
            if (this.Length == array.Length)
            {
                for (int i = 0; i < Length; i++)
                {
                    this[i] = array[i];
                }
            }
        }
        public void CopyBy(double[] array)
        {
            if (this.Length == array.Length)
            {
                for (int i = 0; i < Length; i++)
                {
                    this[i] = array[i];
                }
            }
        }

        //public override void Show(string name, int batchindex = 0)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

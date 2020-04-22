using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Components
{
    [Serializable]
    public abstract class RNdObject
    {
        public string Hash { get; set; }
        public string GetHash() { return Hash; }

        public Real[] Data;

        public static RNdObject Zeros()
        {
            return new RNdMatrix(0, 0, 0, 0);
        }

        public RNdObject()
        {
            Hash = Locker.HashCreator.GetString(64);
            Locker.FileConverter<RNdObject>.AddXMLSerializerType(GetType());
        }

        public int[] Shape { get; set; }
        public int Length { get { int l = 1; foreach (var item in Shape) { l *= item; } return l; } }
        public int BatchSize { get { return Shape[0]; } }
        public int Channels { get { return Shape[1]; } }
        public int Width { get { return Shape[2]; } }
        public int Height { get { return Shape[3]; } }
        public int AreaCount { get { return Shape[0] * Shape[1]; } }
        public int AreaSize { get { return Shape[2] * Shape[3]; } }

        public static bool IsSimilarity(RNdObject o1, RNdObject o2)
        {
            for (int i = 0; i < o1.Shape.Length; i++)
            {
                if (o1.Shape[i] != o2.Shape[i])
                {
                    return false;
                }
            }
            return true;
        }

        public abstract RNdObject Abs();

        public abstract RNdObject Clone();

        protected Locker.FileType FileType { get; set; } = Locker.FileType.XML;
        public abstract void Save(System.IO.DirectoryInfo location);

        public abstract RNdObject Load(System.IO.DirectoryInfo location, string hash);

        public static RNdObject operator -(RNdObject o1, RNdObject o2)
        {
            if ((o1.GetType()) == (o2.GetType()))
            {
                if (RNdObject.IsSimilarity(o1, o2))
                {
                    var x = o1.Clone();
                    for (int i = 0; i < o1.Length; i++)
                    {
                        x.Data[i] = o1.Data[i] - o2.Data[i];
                    }
                    return x;
                }
                else { return null; }
            }
            else { return null; }
        }

        public static RNdObject operator +(RNdObject o1, RNdObject o2)
        {
            if ((o1.GetType()) == (o2.GetType()))
            {
                if (RNdObject.IsSimilarity(o1, o2))
                {
                    var x = o1.Clone();
                    for (int i = 0; i < o1.Length; i++)
                    {
                        x.Data[i] = o1.Data[i] + o2.Data[i];
                    }
                    return x;
                }
                else { return null; }
            }
            else { return null; }
        }

        public static RNdObject operator *(RNdObject o1, RNdObject o2)
        {
            if ((o1.GetType()) == (o2.GetType()))
            {
                if (RNdObject.IsSimilarity(o1, o2))
                {
                    var x = o1.Clone();
                    for (int i = 0; i < o1.Length; i++)
                    {
                        x.Data[i] = o1.Data[i] * o2.Data[i];
                    }
                    return x;
                }
                else { return null; }
            }
            else { return null; }
        }

        public static RNdObject operator /(RNdObject o1, RNdObject o2)
        {
            if ((o1.GetType()) == (o2.GetType()))
            {
                if (RNdObject.IsSimilarity(o1, o2))
                {
                    var x = o1.Clone();
                    for (int i = 0; i < o1.Length; i++)
                    {
                        x.Data[i] = o1.Data[i] / o2.Data[i];
                    }
                    return x;
                }
                else { return null; }
            }
            else { return null; }
        }
    }
}

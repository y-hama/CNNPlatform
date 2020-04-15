﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Runtime.InteropServices;

namespace Components.Imaging
{
    public class FileLoader
    {
        public static FileLoader Instance { get; } = new FileLoader();
        private FileLoader()
        {
        }

        private List<string> ImageExtensions = new List<string>()
        {
            ".bmp",
            ".jpg",
            ".png",
        };

        private System.IO.DirectoryInfo SourceDirectory { get; set; } = null;
        private System.IO.DirectoryInfo ResultDirectory { get; set; } = null;

        private class ObjectIndex
        {
            public class Image
            {
                private Mat containerinstance { get; set; }
                public Mat Container
                {
                    get
                    {
                        if (containerinstance == null)
                        { return (containerinstance = new Mat(Infomation.FullName)); }
                        else
                        { return containerinstance; }
                    }
                    set { containerinstance = value; }
                }
                public System.IO.FileInfo Infomation { get; set; }

                public Image(Mat source) { Container = source; }
                public Image(System.IO.FileInfo info) { Infomation = info; }
            }
            public List<Image> Images { get; set; } = new List<Image>();
            public ObjectIndex(Image source)
            {
                Images.Add(source);
            }
        }
        private Queue<ObjectIndex> Index { get; set; } = new Queue<ObjectIndex>();

        private string HeadImage { get; set; }

        public void SetSourceLocation(System.IO.DirectoryInfo info)
        {
            if (!info.Exists) { throw new Exception(); }
            SourceDirectory = info;
        }

        public void SetResultLocation(System.IO.DirectoryInfo info)
        {
            if (!info.Exists) { throw new Exception(); }
            ResultDirectory = info;
        }

        private Mat CreateNoizeImage(int inw, int inh)
        {
            Mat frame = new Mat(new Size(inw, inh), MatType.CV_8UC3, new Scalar(0));
            byte[] array = new byte[frame.Channels() * frame.Size().Width * frame.Size().Height];
            double edge = State.RandomSource.Next(1, 4);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (byte)(Math.Max(0, Math.Min(1, Math.Abs(State.GetRandomValue(0, 0.1, edge)))) * byte.MaxValue);
            }
            Marshal.Copy(array, 0, frame.Data, array.Length);
            return frame;
        }

        private List<ObjectIndex.Image> GetIndex(out bool check, int inw, int inh)
        {
            if (Index.Count > 0)
            {
                check = false;
            }
            else
            {
                List<ObjectIndex> list = new List<ObjectIndex>();
                #region Load SourceImage
                if (SourceDirectory == null)
                {
                    list.Add(new ObjectIndex(new ObjectIndex.Image(CreateNoizeImage(inw, inh))));
                }
                else
                {
                    var files = SourceDirectory.GetFiles();
                    int fcnt = files.Length, count = 0;
                    var sellection = new List<int>();
                    while (count != fcnt)
                    {
                        var idx = State.RandomSource.Next(fcnt);
                        while (sellection.Contains(idx))
                        {
                            idx = State.RandomSource.Next(fcnt);
                        }
                        sellection.Add(idx);
                        list.Add(new ObjectIndex(new ObjectIndex.Image(files[idx])));
                        count++;
                    }
                }
                #endregion
                #region Load ResultImage
                if (ResultDirectory == null)
                {
                    #region CreateRandom
                    foreach (var item in list)
                    {
                        if (item.Images[0].Infomation == null)
                        {
                            item.Images.Add(new ObjectIndex.Image(CreateNoizeImage(inw, inh)));
                        }
                        else
                        {
                            item.Images.Add(new ObjectIndex.Image(item.Images[0].Infomation));
                        }
                    }
                    #endregion
                }
                else
                {
                    var rlist = new List<System.IO.FileInfo>(ResultDirectory.GetFiles());
                    if (rlist.Count == 0)
                    {
                        #region CreateRandom
                        foreach (var item in list)
                        {
                            item.Images.Add(new ObjectIndex.Image(CreateNoizeImage(inw, inh)));
                        }
                        #endregion
                    }
                    else
                    {
                        foreach (var item in list)
                        {
                            if (item.Images[0].Infomation == null)
                            {
                                #region LoadRandom
                                item.Images.Add(new ObjectIndex.Image(rlist[State.RandomSource.Next(rlist.Count)]));
                                #endregion
                            }
                            else
                            {
                                int idx = rlist.FindIndex(x => x.Name == item.Images[0].Infomation.Name);
                                if (idx >= 0)
                                {
                                    item.Images.Add(new ObjectIndex.Image(rlist[idx]));
                                }
                                else
                                {
                                    #region LoadRandom
                                    item.Images.Add(new ObjectIndex.Image(rlist[State.RandomSource.Next(rlist.Count)]));
                                    #endregion
                                }
                            }
                        }
                    }
                }
                #endregion
                foreach (var item in list)
                {
                    Index.Enqueue(item);
                }
                check = true;
            }
            return Index.Dequeue().Images;
        }

        public bool LoadImage(int inw, int inh, int outw, int outh, int batchcount, int inchannels, int outchannels, out RNdMatrix smat, out RNdMatrix tmat)
        {
            bool ret = false;
            smat = tmat = null;

            List<Mat> sframes = new List<Mat>();
            List<Mat> tframes = new List<Mat>();

            for (int i = 0; i < batchcount; i++)
            {
                bool check;
                var index = GetIndex(out check, inw, inh);
                ret |= check;
                var mat1 = (index[0].Container);
                var mat2 = (index[1].Container);
                var sframe = mat1.Clone();
                if (inchannels != sframe.Channels())
                {
                    if (inchannels == 1) { Cv2.CvtColor(sframe, sframe, ColorConversionCodes.BGR2GRAY); }
                    else if (inchannels == 3) { Cv2.CvtColor(sframe, sframe, ColorConversionCodes.GRAY2BGR); }
                    else { throw new Exception(); }
                }
                sframe = sframe.Resize(new Size(inw, inh), 0, 0, InterpolationFlags.Area);
                sframes.Add(sframe.Clone());

                var tframe = mat2.Clone();
                if (outchannels != tframe.Channels())
                {
                    if (outchannels == 1) { Cv2.CvtColor(tframe, tframe, ColorConversionCodes.BGR2GRAY); }
                    else if (outchannels == 3) { Cv2.CvtColor(tframe, tframe, ColorConversionCodes.GRAY2BGR); }
                    else { throw new Exception(); }
                }
                tframe = Effect(tframe.Resize(new Size(outw, outh), 0, 0, InterpolationFlags.Area));
                tframe = Effect(tframe);
                tframes.Add(tframe.Clone());
            }

            Converter.MatToRNdMatrix(sframes.ToArray(), out smat);
            Converter.MatToRNdMatrix(tframes.ToArray(), out tmat);
            return ret;
        }

        private Mat Effect(Mat source)
        {
            Mat frame = source.Clone();

            //var frames = frame.Split();
            //for (int i = 0; i < frames.Length; i++)
            //{
            //    Cv2.Laplacian(frames[i], frames[i], frames[i].Type());
            //}
            //Cv2.Merge(frames, frame);

            return frame;
        }

        private Mat Offset(Mat source)
        {
            Mat frame = source.Clone();

            return frame;
        }
    }
}

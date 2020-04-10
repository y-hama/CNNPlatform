using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

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
        };

        private System.IO.DirectoryInfo Directory { get; set; }

        public void SetLocation(System.IO.DirectoryInfo info)
        {
            if (!info.Exists) { throw new Exception(); }
            Directory = info;
        }

        private Queue<string> IndexA { get; set; } = new Queue<string>();
        private Queue<string> IndexB { get; set; } = new Queue<string>();
        private string HeadImage { get; set; }

        private string[] GetIndex(out bool check)
        {
            string idx1 = string.Empty;
            string idx2 = string.Empty;
            if (IndexA.Count > 0)
            {
                check = false;
            }
            else
            {
                int fcnt = 0;
                var flist = new List<System.IO.FileInfo>(Directory.GetFiles()).FindAll(x => ImageExtensions.Contains(x.Extension.ToLower()));
                HeadImage = flist[0].FullName;
                while (fcnt != flist.Count)
                {
                    var idx = State.RandomSource.Next(flist.Count);
                    while (IndexA.Contains(flist[idx].FullName))
                    {
                        idx = State.RandomSource.Next(flist.Count);
                    }
                    IndexA.Enqueue(flist[idx].FullName);

                    idx = State.RandomSource.Next(flist.Count);
                    while (IndexB.Contains(flist[idx].FullName))
                    {
                        idx = State.RandomSource.Next(flist.Count);
                    }
                    IndexB.Enqueue(flist[idx].FullName);
                    fcnt++;
                }
                check = true;

            }
            idx1 = IndexA.Dequeue();
            idx2 = IndexB.Dequeue();
            return new string[] { idx1, idx1 };
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
                var index = GetIndex(out check);
                ret |= check;
                var mat1 = new Mat(index[0]);
                var mat2 = new Mat(index[1]);
                var sframe = mat1.Clone();
                if (inchannels != sframe.Channels())
                {
                    if (inchannels == 1) { Cv2.CvtColor(sframe, sframe, ColorConversionCodes.BGR2GRAY); }
                    else if (inchannels == 3) { Cv2.CvtColor(sframe, sframe, ColorConversionCodes.GRAY2BGR); }
                    else { throw new Exception(); }
                }
                sframe = sframe.Resize(new Size(inw, inh));
                sframes.Add(sframe.Clone());

                var tframe = mat2.Clone();
                if (outchannels != tframe.Channels())
                {
                    if (outchannels == 1) { Cv2.CvtColor(tframe, tframe, ColorConversionCodes.BGR2GRAY); }
                    else if (outchannels == 3) { Cv2.CvtColor(tframe, tframe, ColorConversionCodes.GRAY2BGR); }
                    else { throw new Exception(); }
                }
                tframe = Effect(tframe.Resize(new Size(outw, outh)));
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

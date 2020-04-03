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

        public void LoadImage(int inw, int inh, int outw, int outh, int batchcount, int inchannels, int outchannels, out RNdMatrix smat, out RNdMatrix tmat)
        {
            smat = tmat = null;

            List<Mat> sframes = new List<Mat>();
            List<Mat> tframes = new List<Mat>();
            var flist = new List<System.IO.FileInfo>(Directory.GetFiles()).FindAll(x => ImageExtensions.Contains(x.Extension.ToLower()));

            for (int i = 0; i < batchcount; i++)
            {
                var index = State.RandomSource.Next(flist.Count);
                var mat = new Mat(flist[index].FullName);
                var sframe = mat.Clone();
                if (inchannels != sframe.Channels())
                {
                    if (inchannels == 1) { Cv2.CvtColor(sframe, sframe, ColorConversionCodes.BGR2GRAY); }
                    else if (inchannels == 3) { Cv2.CvtColor(sframe, sframe, ColorConversionCodes.GRAY2BGR); }
                    else { throw new Exception(); }
                }
                sframe = sframe.Resize(new Size(inw, inh));
                sframes.Add(sframe.Clone());

                var tframe = mat.Clone();
                if (outchannels != tframe.Channels())
                {
                    if (outchannels == 1) { Cv2.CvtColor(tframe, tframe, ColorConversionCodes.BGR2GRAY); }
                    else if (outchannels == 3) { Cv2.CvtColor(tframe, tframe, ColorConversionCodes.GRAY2BGR); }
                    else { throw new Exception(); }
                }
                tframe = Effect(tframe.Resize(new Size(outw, outh)));
                tframes.Add(tframe.Clone());
            }

            Converter.MatToRNdMatrix(sframes.ToArray(), out smat);
            Converter.MatToRNdMatrix(tframes.ToArray(), out tmat);
        }

        private Mat Effect(Mat source)
        {
            Mat frame = source.Clone();

            //double rho = 0.75;
            //Cv2.Laplacian(source, frame, source.Type());
            //Cv2.AddWeighted(source, rho, frame, (1 - rho), 1, frame);

            return frame;
        }
    }
}

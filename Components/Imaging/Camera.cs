﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace Components.Imaging
{
    public class Camera
    {
        public static Camera Instance { get; } = new Camera();
        private Camera()
        {
            capture = new VideoCapture(0);
            if (!capture.IsOpened()) { throw new Exception(); }
            //var codec = @"MJPG";
            //capture.Set(CaptureProperty.FourCC, VideoWriter.FourCC(codec[0], codec[1], codec[2], codec[3]));
        }

        private const string VIDEOFILE_NAME = "_videoframe.mp4";

        private Mat PrevFrame { get; set; }
        private Mat PrevDiffFrame { get; set; }
        private RotatedRect PrevRect { get; set; }


        private VideoCapture capture { get; set; }
        private object ___slock { get; set; } = new object();
        private Mat _SourceFrame = null;
        private Mat SourceFrame
        {
            get
            {
                lock (___slock)
                {
                    return _SourceFrame;
                }
            }
            set
            {
                lock (___slock)
                {
                    _SourceFrame = value;
                }
            }
        }
        private object ___tlock { get; set; } = new object();
        private Mat _TeacherFrame = null;
        private Mat TeacherFrame
        {
            get
            {
                lock (___tlock)
                {
                    return _TeacherFrame;
                }
            }
            set
            {
                lock (___tlock)
                {
                    _TeacherFrame = value;
                }
            }
        }
        private bool startupcomplete { get; set; } = false;

        private int Delay { get; set; }
        private bool Terminate { get; set; } = false;

        public void StartCapture(int delay = 0)
        {
            Delay = delay;
            SourceFrame = new Mat();
            TeacherFrame = new Mat();
            new System.Threading.Thread(() =>
            {
                while (!Terminate)
                {
                    if (capture.Read(SourceFrame))
                    {
                        //if (delay == 0)
                        //{
                        //    TeacherFrame = SourceFrame.Clone();
                        //}
                        //else
                        //{
                        //    System.Threading.Thread.Sleep(delay);
                        //    if (!capture.Read(TeacherFrame))
                        //    {
                        //        throw new Exception();
                        //    }
                        //}
                    }
                    else { throw new Exception(); }
                    System.Threading.Thread.Sleep(1);
                    startupcomplete = true;
                }
            }).Start();
        }
        public void StopCapture() { Terminate = true; }

        public void SaveVideo(int TimeLength)
        {
            List<Mat> frames = new List<Mat>();
            var size = new Size(640, 480);

            var start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < TimeLength / 2)
            {
                var mat = new Mat();
                if (capture.Read(mat))
                {
                    mat = mat.Resize(size);
                    frames.Add(mat);
                    System.Threading.Thread.Sleep(1);
                }
            }

            var frame_rate = frames.Count;
            var fmt = VideoWriter.FourCC('m', 'p', '4', 'v');
            var writer = new VideoWriter(VIDEOFILE_NAME, fmt, frame_rate, size);
            for (int i = 0; i < frames.Count; i++)
            {
                writer.Write(frames[i].Clone());
            }
            for (int i = frames.Count - 1; i >= 0; i--)
            {
                writer.Write(frames[i].Clone());
            }
            writer.Release();
        }

        public bool LoadCapture(int batchcount, int inchannels, int inw, int inh, int outchannels, int outw, int outh, bool doFlipX, bool doFlipY, double doRot, bool doAffine, out RNdMatrix smat, out RNdMatrix tmat)
        {
            bool ret = true;

            List<Mat> sframes = new List<Mat>();
            List<Mat> tframes = new List<Mat>();

            for (int i = 0; i < batchcount; i++)
            {
                Mat mat = new Mat(), mat1, mat2;
                while (!capture.Read(mat)) { }
                mat1 = mat.Clone();
                mat2 = mat.Clone();

                bool flipx = false;
                bool flipy = false;
                int oflux = 0, ofluy = 0;
                int ofrdx = 0, ofrdy = 0;
                double scl = 2.5;
                double rotangle = 0;
                Point lu = new Point(), rd = new Point();

                if (doFlipX)
                {
                    flipx = (State.RandomSource.NextDouble() > 0.5 ? true : false);
                }
                if (doFlipY)
                {
                    flipy = (State.RandomSource.NextDouble() > 0.5 ? true : false);
                }
                rotangle = (State.RandomSource.NextDouble() * 2 - 1) * doRot;
                if (doAffine)
                {
                    if (inw > 1 && outw > 1 && inh > 1 && outh > 1)
                    {
                        oflux = State.RandomSource.Next(Math.Max(0, (int)(Math.Min(inw, outw) / (scl) - 1)));
                        ofluy = State.RandomSource.Next(Math.Max(0, (int)(Math.Min(inh, outh) / (scl) - 1)));
                        ofrdx = State.RandomSource.Next(Math.Max(0, (int)(Math.Min(inw, outw) / (scl) - 1)));
                        ofrdy = State.RandomSource.Next(Math.Max(0, (int)(Math.Min(inh, outh) / (scl) - 1)));
                    }
                }

                lu = new Point(oflux, ofluy);
                rd = new Point(ofrdx, ofrdy);

                var sframe = mat1.Clone();
                if (inchannels != sframe.Channels())
                {
                    if (inchannels == 1) { Cv2.CvtColor(sframe, sframe, ColorConversionCodes.BGR2GRAY); }
                    else if (inchannels == 3) { Cv2.CvtColor(sframe, sframe, ColorConversionCodes.GRAY2BGR); }
                    else { throw new Exception(); }
                }
                sframe = sframe.Resize(new Size(inw, inh), 0, 0, InterpolationFlags.Area);
                sframe = EffectProcess.Offset(sframe, lu, rd, flipx, flipy, rotangle);
                sframes.Add(sframe.Clone());

                var tframe = mat2.Clone();
                if (outchannels != tframe.Channels())
                {
                    if (outchannels == 1) { Cv2.CvtColor(tframe, tframe, ColorConversionCodes.BGR2GRAY); }
                    else if (outchannels == 3) { Cv2.CvtColor(tframe, tframe, ColorConversionCodes.GRAY2BGR); }
                    else { throw new Exception(); }
                }
                tframe = EffectProcess.Effect(tframe.Resize(new Size(outw, outh), 0, 0, InterpolationFlags.Area));
                tframe = EffectProcess.Offset(tframe, lu, rd, flipx, flipy, rotangle);
                tframes.Add(tframe.Clone());
            }

            Converter.MatToRNdMatrix(sframes.ToArray(), out smat);
            Converter.MatToRNdMatrix(tframes.ToArray(), out tmat);
            return ret;
        }

        public void GetDiffFrame(int width, int height, out double[] buf)
        {
            buf = new double[width * height];
            var frame = new Mat();
            var diff = new Mat();
            var rotatedrect = new RotatedRect();
            if (capture.Read(frame))
            {
                frame = frame.Resize(new Size(width, height));
                Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2GRAY);
                if (PrevFrame != null)
                {
                    Cv2.Absdiff(frame, PrevFrame, diff);
                    double weight = 1;
                    Mat[] contours;
                    for (int r = 0; r < 2; r++)
                    {
                        Cv2.Threshold(diff, diff, byte.MaxValue / 8, byte.MaxValue, ThresholdTypes.Otsu);

                        var nonzerocnt = Cv2.CountNonZero(diff);
                        weight = (0.25 - ((double)nonzerocnt) / (width * height)) / (0.25);
                        weight = weight < 0 ? 0 : weight;

                        if (weight > 0.5)
                        {
                            Mat h = new Mat();
                            Cv2.FindContours(diff, out contours, new Mat(), RetrievalModes.External, ContourApproximationModes.ApproxTC89KCOS);

                            diff = new Mat(new Size(width, height), MatType.CV_8UC1, new Scalar(0));
                            if (contours.Length > 0)
                            {
                                var areaave = contours.Average(x => Cv2.ContourArea(x));
                                for (int i = 0; i < contours.Length; i++)
                                {
                                    if (Cv2.ContourArea(contours[i]) > areaave)
                                    {
                                        Cv2.DrawContours(diff, contours, i, new Scalar(byte.MaxValue), -1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            diff = new Mat(new Size(width, height), MatType.CV_8UC1, new Scalar(0));
                        }

                    }
                    Point[][] contourspoint;
                    HierarchyIndex[] hierarchyIndexes;
                    Cv2.FindContours(diff.Clone(), out contourspoint, out hierarchyIndexes, RetrievalModes.External, ContourApproximationModes.ApproxTC89KCOS);
                    if (contourspoint.Length > 0)
                    {
                        var points = new List<Point>();
                        for (int idx_cnt = 0; idx_cnt < contourspoint.GetLength(0); ++idx_cnt)
                        {
                            if (hierarchyIndexes[idx_cnt].Parent != -1) { continue; }
                            points.AddRange(contourspoint[idx_cnt]);
                        }
                        if (points.Count > 5)
                        {
                            diff = new Mat(new Size(width, height), MatType.CV_8UC1, new Scalar(0));
                            rotatedrect = Cv2.FitEllipse(points);
                            float rho = 0.25f;
                            rotatedrect.Angle = (rho * rotatedrect.Angle + (1 - rho) * PrevRect.Angle);
                            rotatedrect.Size.Width = (rho * rotatedrect.Size.Width + (1 - rho) * PrevRect.Size.Width);
                            rotatedrect.Size.Height = (rho * rotatedrect.Size.Height + (1 - rho) * PrevRect.Size.Height);
                            Cv2.Ellipse(diff, rotatedrect, new Scalar(byte.MaxValue), -1);
                        }
                    }

                    double w = 0.8;
                    Cv2.AddWeighted(PrevDiffFrame, w, diff, 1 - w, 0, diff);

                    Mat result = diff.Clone();
                    //Cv2.Threshold(diff, result, byte.MaxValue / 8, byte.MaxValue, ThresholdTypes.Binary);

                    Cv2.Dilate(result, result, new Mat(), new Point(-1, -1), 8);

                    //frame.CopyTo(result, result);

                    unsafe
                    {
                        byte* rslt = (byte*)result.Data;
                        byte* f = (byte*)frame.Data;
                        for (int i = 0; i < width * height; i++)
                        {
                            double r = (double)rslt[i] / byte.MaxValue;
                            if (r > 0.25)
                            {
                                buf[i] = ((double)f[i] / byte.MaxValue) + 0.25;
                            }
                        }
                    }
                }
                if (PrevFrame == null)
                {
                    PrevFrame = frame.Clone();
                    PrevDiffFrame = new Mat(PrevFrame.Size(), PrevFrame.Type(), new Scalar(0));
                    PrevRect = new RotatedRect();
                }
                else
                {
                    double weight = 0.5;
                    Cv2.AddWeighted(PrevFrame, weight, frame, 1 - weight, 0, PrevFrame);
                    PrevDiffFrame = diff.Clone();
                    PrevRect = rotatedrect;
                }
            }
        }
    }
}

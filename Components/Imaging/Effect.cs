﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;

namespace Components.Imaging
{
    static class EffectProcess
    {
        public static Mat Effect(Mat source)
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

        public static Mat Offset(Mat source, Point leftup, Point rightdown, bool flipx, bool flipy, double rotangle)
        {
            if (source.Width <= 1 || source.Height <= 1)
            {
                return source;
            }
            else
            {
                Mat frame = source.Clone();
                if (flipx && flipy)
                {
                    frame = frame.Flip(FlipMode.XY);
                }
                else
                {
                    if (flipx)
                    {
                        frame = frame.Flip(FlipMode.X);
                    }
                    if (flipy)
                    {
                        frame = frame.Flip(FlipMode.Y);
                    }
                }

                var center = new Point2f(frame.Width / 2, frame.Height / 2);
                Mat rMat = Cv2.GetRotationMatrix2D(center, rotangle, 1);
                Cv2.WarpAffine(frame, frame, rMat, new Size(frame.Cols, frame.Rows));

                var rect = new Rect(leftup, new Size(frame.Width - (leftup.X + rightdown.X), frame.Height - (leftup.Y + rightdown.Y)));
                frame = frame.Clone()[rect];
                return frame.Resize(source.Size(), 0, 0, InterpolationFlags.Area);
            }
        }
    }
}

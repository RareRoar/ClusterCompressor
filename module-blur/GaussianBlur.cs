using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GaussianBlur
{
    public class GaussianBlurer
    {
        public static double[,] ApplyGaussianBlur(int lenght, double weight)
        {
            double[,] kernel = new double[lenght, lenght];
            double kernelSum = 0;
            int foff = (lenght - 1) / 2;
            double distance = 0;
            double constant = 1d / (2 * Math.PI * weight * weight);
            for (int y = -foff; y <= foff; y++)
            {
                for (int x = -foff; x <= foff; x++)
                {
                    distance = ((y * y) + (x * x)) / (2 * weight * weight);
                    kernel[y + foff, x + foff] = constant * Math.Exp(-distance);
                    kernelSum += kernel[y + foff, x + foff];
                }
            }
            for (int y = 0; y < lenght; y++)
            {
                for (int x = 0; x < lenght; x++)
                {
                    kernel[y, x] = kernel[y, x] * 1d / kernelSum;
                }
            }
            return kernel;
        }

        public static Bitmap Convolve(Bitmap srcImage, double[,] kernel)
        {
            int width = srcImage.Width;
            int height = srcImage.Height;
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytes = srcData.Stride * srcData.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            srcImage.UnlockBits(srcData);
            int colorChannels = 3;
            double[] rgb = new double[colorChannels];
            int foff = (kernel.GetLength(0) - 1) / 2;
            int kcenter = 0;
            int kpixel = 0;
            for (int y = foff; y < height - foff; y++)
            {
                for (int x = foff; x < width - foff; x++)
                {
                    for (int c = 0; c < colorChannels; c++)
                    {
                        rgb[c] = 0.0;
                    }
                    kcenter = y * srcData.Stride + x * 4;
                    for (int fy = -foff; fy <= foff; fy++)
                    {
                        for (int fx = -foff; fx <= foff; fx++)
                        {
                            kpixel = kcenter + fy * srcData.Stride + fx * 4;
                            for (int c = 0; c < colorChannels; c++)
                            {
                                rgb[c] += (double)(buffer[kpixel + c]) * kernel[fy + foff, fx + foff];
                            }
                        }
                    }
                    for (int c = 0; c < colorChannels; c++)
                    {
                        if (rgb[c] > 255)
                        {
                            rgb[c] = 255;
                        }
                        else if (rgb[c] < 0)
                        {
                            rgb[c] = 0;
                        }
                    }
                    for (int c = 0; c < colorChannels; c++)
                    {
                        result[kcenter + c] = (byte)rgb[c];
                    }
                    result[kcenter + 3] = 255;
                }
            }
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, resultData.Scan0, bytes);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }

        public static void gaussBlur_4(int[] scl, int[] tcl, int w, int h, int r)
        {
            var bxs = boxesForGauss(r, 3);
            boxBlur_4(scl, tcl, w, h, (bxs[0] - 1) / 2);
            boxBlur_4(tcl, scl, w, h, (bxs[1] - 1) / 2);
            boxBlur_4(scl, tcl, w, h, (bxs[2] - 1) / 2);
        }

        public static int[] boxesForGauss(double sigma, int n)  // standard deviation, number of boxes
        {
            double wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);  // Ideal averaging filter width 
            int wl = (int)Math.Floor(wIdeal);
            if (wl % 2 == 0) 
                wl--;
            int wu = wl + 2;

            double mIdeal = (12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
            int m = (int)Math.Round(mIdeal);
            // var sigmaActual = Math.sqrt( (m*wl*wl + (n-m)*wu*wu - n)/12 );

            int[] sizes = new int[n];
            for (int i = 0; i < n; i++)
            {
                sizes[i] = (i < m) ? wl : wu;
            }
            return sizes;
        }
        public static void boxBlur_4(int[] scl, int[] tcl, int w, int h, int r)
        {
            for (var i = 0; i < scl.Length; i++) tcl[i] = scl[i];
            boxBlurH_4(tcl, scl, w, h, r);
            boxBlurT_4(scl, tcl, w, h, r);
        }
        public static void boxBlurH_4(int[] scl, int[] tcl, int w, int h, int r)
        {
            double iarr = 1 / (r + r + 1);
            for (var i = 0; i < h; i++)
            {
                int ti = i * w, li = ti, ri = ti + r;
                int fv = scl[ti], lv = scl[ti + w - 1], val = (r + 1) * fv;
                for (var j = 0; j < r; j++) val += scl[ti + j];
                for (var j = 0; j <= r; j++) { 
                    val += scl[ri++] - fv;
                    tcl[ti++] = (int)Math.Round(val * iarr);
                }
                for (var j = r + 1; j < w - r; j++) { 
                    val += scl[ri++] - scl[li++];
                    tcl[ti++] = (int)Math.Round(val * iarr);
                }
                for (var j = w - r; j < w; j++) { 
                    val += lv - scl[li++];
                    tcl[ti++] = (int)Math.Round(val * iarr);
                }
            }
        }
        public static void boxBlurT_4(int[] scl, int[] tcl, int w, int h, int r)
        {
            double iarr = 1 / (r + r + 1);
            for (var i = 0; i < w; i++)
            {
                int ti = i, li = ti, ri = ti + r * w;
                int fv = scl[ti], lv = scl[ti + w * (h - 1)], val = (r + 1) * fv;
                for (var j = 0; j < r; j++)
                    val += scl[ti + j * w];
                for (var j = 0; j <= r; j++)
                {
                    val += scl[ri] - fv;
                    tcl[ti] = (int)Math.Round(iarr * val);
                    ri += w; ti += w;
                }
                for (var j = r + 1; j < h - r; j++)
                {
                    val += scl[ri] - scl[li];
                    tcl[ti] = (int)Math.Round(val * iarr);
                    li += w; ri += w; ti += w;
                }
                for (var j = h - r; j < h; j++)
                {
                    val += lv - scl[li];
                    tcl[ti] = (int)Math.Round(val * iarr);
                    li += w; ti += w;
                }
            }
        }



    }
}

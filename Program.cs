using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GaussianBlur;
using KMeansPP;
using BWH;
using RLE;
using System.IO;
using System.Reflection;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using BWH.Tools;
using PredictionCompression;
using System.Diagnostics;

namespace ClusterCompressor
{
    public static class Program
    {
        private static readonly string[] samples_ = { "Resources\\Lenna.bmp", "Resources\\War.bmp",
            "Resources\\Rainbow.bmp", "Resources\\Noise.bmp" };
        private static readonly string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        private static readonly Stopwatch stopwatch = new Stopwatch();


        public static byte[] BitmapToByteArray(Bitmap srcImage)
        {
            int width = srcImage.Width;
            int height = srcImage.Height;
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytes = srcData.Stride * srcData.Height;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            srcImage.UnlockBits(srcData);
            return buffer;
        }

        public static Bitmap ByteArrayToBitmap(byte[] bytes, int width, int height)
        {
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bytes, 0, resultData.Scan0, bytes.Length);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }

        private static void Clusterize_(ref Bitmap bitmap, int blockSize, int clusterColors)
        {

            int width = bitmap.Width;
            int height = bitmap.Height;

            int dw = blockSize;
            int dh = dw;
            int wCount = width / dw + (width % dw > 0 ? 1 : 0);
            int hCount = height / dh + (height % dh > 0 ? 1 : 0);


            for (int di = 0; di < hCount; di++)
            {
                for (int dj = 0; dj < wCount; dj++)
                {
                    int w, h;

                    if (di < hCount - 1 || height % dh == 0)
                    {
                        h = dh;
                    }
                    else
                    {
                        h = height % dh;
                    }
                    if (dj < wCount - 1 || width % dw == 0)
                    {
                        w = dw;
                    }
                    else
                    {
                        w = width % dw;
                    }
                    ByteVector[] pixels = new ByteVector[w * h];
                    for (int i = 0; i < h; i++)
                    {
                        for (int j = 0; j < w; j++)
                        {
                            var pixel = bitmap.GetPixel(dj * dw + j, di * dh + i);
                            var rawVector = new byte[] {
                        pixel.R, pixel.G, pixel.B };
                            pixels[i * w + j] = new ByteVector(dj * dw + j, di * dh + i, rawVector);
                        }
                    }

                    KMeansPPClusterizer<ByteVector> kmpp = new KMeansPPClusterizer<ByteVector>(clusterColors, pixels, Metrics.Euclidian);

                    var clusters = kmpp.GetClusters();
                    if (clusters is null)
                    {
                        Console.WriteLine("Fail");
                        Console.ReadKey();
                        return;
                    }
                    Color[] newPixels = new Color[w * h];
                    foreach (var key in clusters.Keys)
                    {
                        var clusterList = clusters[key];
                        int sumR = 0;
                        int sumG = 0;
                        int sumB = 0;
                        foreach (var pixel in clusterList)
                        {
                            sumR += (int)pixel.CoordinatesArray[0];
                            sumG += (int)pixel.CoordinatesArray[1];
                            sumB += (int)pixel.CoordinatesArray[2];
                        }
                        sumR = (int)((double)sumR / clusterList.Count);
                        sumG = (int)((double)sumG / clusterList.Count);
                        sumB = (int)((double)sumB / clusterList.Count);
                        foreach (var pixel in clusterList)
                        {
                            pixel.CoordinatesArray[0] = sumR;
                            pixel.CoordinatesArray[1] = sumG;
                            pixel.CoordinatesArray[2] = sumB;

                            bitmap.SetPixel(pixel.X, pixel.Y, Color.FromArgb(sumR, sumG, sumB));
                        }
                    }
                }
            }
        }



        private static double PNSR(Bitmap P, Bitmap Q)
        {
            int w = P.Width;
            int h = P.Height;

            double sum = 0;
            Color pixelMax = Color.FromArgb(0, 0, 0);
            double absMax = 0;
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Color pixel1 = P.GetPixel(i, j);
                    Color pixel2 = Q.GetPixel(i, j);
                    sum += (Math.Pow(pixel1.R - pixel2.R, 2) +
                        Math.Pow(pixel1.G - pixel2.G, 2) +
                        Math.Pow(pixel1.B - pixel2.B, 2));
                
                    absMax = Math.Sqrt(Math.Pow(pixelMax.R, 2) +
                        Math.Pow(pixelMax.G, 2) +
                        Math.Pow(pixelMax.B, 2));

                    double absNew = Math.Sqrt(Math.Pow(pixel1.R, 2) +
                        Math.Pow(pixel1.G, 2) +
                        Math.Pow(pixel1.B, 2));

                    if (absMax < absNew)
                    {
                        pixelMax = pixel1;
                    }

                }
            }
            absMax = Math.Sqrt(Math.Pow(pixelMax.R, 2) +
                        Math.Pow(pixelMax.G, 2) +
                        Math.Pow(pixelMax.B, 2));

            return 20 * Math.Log10(absMax / Math.Sqrt(sum / (w * h)));
        }

        public static void Main(string[] args)
        {

            string samplePath = samples_[3];
            int blurMethod = 1;
            // int compressionMethod = 0;

            Bitmap bitmap = new Bitmap(projectDirectory + "\\" + samplePath);

            Form form = new Form();
            form.Text = "Image Viewer";
            form.Width = (bitmap.Width);
            form.Height = (bitmap.Height);
            PictureBox pictureBox = new PictureBox();

            Bitmap rawBitmap = bitmap;

            for (int severity = 0; severity < 2; severity++)
            {

                Console.WriteLine("Mercy flag is {0}", severity);
                if (severity == 1) {
                    stopwatch.Start();
                    Clusterize_(ref bitmap, 8, 8);
                    stopwatch.Stop();
                    Console.WriteLine("Clusterization: {0} ms", stopwatch.ElapsedMilliseconds);
                    stopwatch.Restart();

                    if (blurMethod == 0)
                        bitmap = GaussianBlurer.Convolve(bitmap, GaussianBlurer.ApplyGaussianBlur(2, 2));
                    else if (blurMethod == 1)
                        bitmap = (new SwiftBlur(bitmap)).Process(1);

                    stopwatch.Stop();
                    Console.WriteLine("Blur: {0} ms", stopwatch.ElapsedMilliseconds);
                }
                byte[] data = BitmapToByteArray(bitmap);
                for (int compressionMethod = 0; compressionMethod < 3; compressionMethod++)
                {
                    if (compressionMethod == 1)
                        compressionMethod++;
                    byte[] compressed = null;
                    byte[] decompressed = null;
                    stopwatch.Restart();
                    if (compressionMethod == 0)
                    {
                        Console.WriteLine("Huffman");
                        compressed = BWCompression.Compress(data);
                    }
                    else if (compressionMethod == 1)
                    {
                        Console.WriteLine("Prediction");
                        compressed = BurrowsWheelerTransform.Transform(data);
                        compressed = PredictionCompressor.Compress(compressed);
                    }
                    else if (compressionMethod == 2)
                    {
                        Console.WriteLine("RLE");
                        compressed = BurrowsWheelerTransform.Transform(data);
                        compressed = RLEncoder.Compress(compressed);
                    }



                    stopwatch.Stop();
                    Console.WriteLine("Compression: {0} ms", stopwatch.ElapsedMilliseconds);
                    stopwatch.Restart();
                    if (compressionMethod == 0)
                    {
                        decompressed = BWCompression.Decompress(compressed);
                    }
                    else if (compressionMethod == 1)
                    {

                        decompressed = PredictionCompressor.Compress(compressed);
                        decompressed = BurrowsWheelerTransform.InverseTransform(decompressed);
                    }
                    else if (compressionMethod == 2)
                    {

                        decompressed = RLEncoder.Decompress(compressed);
                        decompressed = BurrowsWheelerTransform.InverseTransform(decompressed);
                    }


                    stopwatch.Stop();
                    Console.WriteLine("Deompression: {0} ms", stopwatch.ElapsedMilliseconds);

                    bitmap = ByteArrayToBitmap(decompressed, bitmap.Width, bitmap.Height);


                    Console.Write("Compression coefficient = ");
                    Console.WriteLine((double)compressed.Length / data.Length);
                }
                if (severity == 1) { 
                    Console.Write("PNSR = ");
                    Console.WriteLine(PNSR(rawBitmap, bitmap));
                }
            }
            pictureBox.Image = bitmap;
            pictureBox.Dock = DockStyle.Fill;
            form.Controls.Add(pictureBox);
            
            bitmap.Save(projectDirectory + "\\result.bmp");
            Application.Run(form);
        }
    }
}

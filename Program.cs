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

namespace ClusterCompressor
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Bitmap bitmap = new Bitmap(@"C:\Users\Admin\source\repos\ClusterCompressor\Lenna.bmp");




            Form form = new Form();
            form.Text = "Image Viewer";
            form.Width = (int)(bitmap.Width * 1.2);
            form.Height = (int)(bitmap.Height * 1.2);
            PictureBox pictureBox = new PictureBox();

            
            int width = bitmap.Width;
            int height = bitmap.Height;

            int dw = 10;
            int dh = 10;
            int wCount = width / dw + (width % dw > 0 ? 1 : 0);
            int hCount = height / dh + (height % dh > 0 ? 1 : 0);
            //for (int di = 0; di < hCount; di++)
            //{
            //    for (int dj = 0; dj < wCount; dj++)
            //    {
            //        int w, h;

            //        if (di < hCount - 1 || height % dh == 0)
            //        {
            //            h = dh;
            //        }
            //        else
            //        {
            //            h = height % dh;
            //        }
            //        if (dj < wCount - 1 || width % dw == 0)
            //        {
            //            w = dw;
            //        }
            //        else
            //        {
            //            w = width % dw;
            //        }
            //        ByteVector[] pixels = new ByteVector[w * h];
            //        for (int i = 0; i < h; i++)
            //        {
            //            for (int j = 0; j < w; j++)
            //            {
            //                var pixel = bitmap.GetPixel(dj * dw + j, di * dh + i);
            //                var rawVector = new byte[] { 
            //            //pixel.A, 
            //            pixel.R, pixel.G, pixel.B };
            //                pixels[i * w + j] = new ByteVector(dj * dw + j, di * dh + i, rawVector);
            //            }
            //        }



            //        KMeansPPClusterizer<ByteVector> kmpp = new KMeansPPClusterizer<ByteVector>(5, pixels, Metrics.Euclidian);

            //        var clusters = kmpp.GetClusters();
            //        if (clusters is null)
            //        {
            //            Console.WriteLine("Fail");
            //            Console.ReadKey();
            //            return;
            //        }
            //        else
            //        {
            //            //Console.WriteLine("OK");
            //        }
            //        Color[] newPixels = new Color[w * h];
            //        foreach (var key in clusters.Keys)
            //        {
            //            var clusterList = clusters[key];
            //            int sumR = 0;
            //            int sumG = 0;
            //            int sumB = 0;
            //            foreach (var pixel in clusterList)
            //            {
            //                sumR += (int)pixel.CoordinatesArray[0];
            //                sumG += (int)pixel.CoordinatesArray[1];
            //                sumB += (int)pixel.CoordinatesArray[2];
            //            }
            //            sumR = (int)((double)sumR / clusterList.Count);
            //            sumG = (int)((double)sumG / clusterList.Count);
            //            sumB = (int)((double)sumB / clusterList.Count);
            //            foreach (var pixel in clusterList)
            //            {
            //                pixel.CoordinatesArray[0] = sumR;
            //                pixel.CoordinatesArray[1] = sumG;
            //                pixel.CoordinatesArray[2] = sumB;

            //                // !!!
            //                bitmap.SetPixel(pixel.X, pixel.Y, Color.FromArgb(sumR, sumG, sumB));
            //            }
            //        }
            //    }
            //}

            byte[] data = GaussianBlurer.BitmapToByteArray(bitmap);
            byte[] compressed = RLEncoder.compress(data); //BWCompression.Compress(data);
            byte[] decompressed = RLEncoder.decompress(compressed);//BWCompression.Decompress(compressed);
            bitmap = GaussianBlurer.ByteArrayToBitmap(decompressed, width, height);
            
            
            //pictureBox.Image = GaussianBlurer.Convolve(bitmap, GaussianBlurer.ApplyGaussianBlur(20, 20));
            //SwiftBlur sb = new SwiftBlur(bitmap);
            //pictureBox.Image = sb.Process(1);

            pictureBox.Image = bitmap;
            pictureBox.Dock = DockStyle.Fill;
            form.Controls.Add(pictureBox);
            pictureBox.Location = new Point(0, 0);
            Application.Run(form);
        }
    }
}

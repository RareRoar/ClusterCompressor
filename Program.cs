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

            //byte[] data = new byte[] { 0x01, 0x01, 0x01, 0x01, 0x02, 0x01, 0x01, 0x02 };//
            byte[] data = GaussianBlurer.BitmapToByteArray(bitmap);
            byte[] compressed = AltCompress(data);//RLEncoder.compress(data); //BWCompression.Compress(data);
                                                  //byte[] decompressed = AltDecompress(compressed);//RLEncoder.decompress(compressed);//BWCompression.Decompress(compressed);
                                                  //bitmap = GaussianBlurer.ByteArrayToBitmap(decompressed, width, height);
            //for (int i = 0; i < data.Length; i++)
            //{
            //    Console.Write(Convert.ToString(data[i], 2).PadLeft(8, '0'));
            //    Console.Write(" ");
            //}
            //Console.WriteLine();
            //for (int i =0; i < compressed.Length; i++)
            //{
            //    Console.Write(Convert.ToString(compressed[i], 2).PadLeft(8, '0'));
            //    Console.Write(" ");
            //}
            //Console.WriteLine();
            byte[] decompressed = AltDecompress(compressed);
            //for (int i = 0; i < decompressed.Length; i++)
            //{
            //    Console.Write(Convert.ToString(decompressed[i], 2).PadLeft(8, '0'));
            //    Console.Write(" ");
            //}
            //Console.WriteLine(decompressed.Length);

            //pictureBox.Image = GaussianBlurer.Convolve(bitmap, GaussianBlurer.ApplyGaussianBlur(20, 20));
            //SwiftBlur sb = new SwiftBlur(bitmap);
            //pictureBox.Image = sb.Process(1);
            bitmap = GaussianBlurer.ByteArrayToBitmap(decompressed, width, height);
            Console.WriteLine(data.Length);
            Console.WriteLine(compressed.Length);
            Console.WriteLine(decompressed.Length);

            for (int i = 0; i < data.Length; i++)
            {

                //Console.Write(i);
                //Console.Write(") ");
                //Console.Write(Convert.ToString(data[i], 2).PadLeft(8, '0'));
                //Console.Write(" ");
                //Console.Write(Convert.ToString(compressed[i], 2).PadLeft(8, '0'));
                //Console.Write(" ");
                //Console.WriteLine(Convert.ToString(decompressed[i], 2).PadLeft(8, '0'));
                if (data[i] != decompressed[i])
                {
                    Console.Write(i);
                    break;

                }
            }
            pictureBox.Image = bitmap;
            pictureBox.Dock = DockStyle.Fill;
            form.Controls.Add(pictureBox);
            pictureBox.Location = new Point(0, 0);
            Application.Run(form);
            //Console.ReadKey();
        }


        public static byte[] AltCompress(byte[] data)
        {
            byte[] result = new byte[data.Length];
            byte[] dict = new byte[0xFFFF+1];
            int offset = 0;
            result[0] = data[0];
            result[1] = data[1];
            int currentByte = 2;
            for (int i = 2; i < data.Length; i++)
            {
                if (dict[(data[i-2] << 8) + data[i - 1]] == data[i])
                {
                    //Console.WriteLine("Guessed");
                    //Console.WriteLine(currentByte);
                    //Console.WriteLine(string.Format($"Byte: {0}, offset: {1}", currentByte.ToString(), offset));
                    //Console.Write(Convert.ToString((0x01 << (8 - offset - 1)), 2).PadLeft(8, '0'));
                    result[currentByte] = (byte)(result[currentByte] | (0x01 << (8 - offset - 1)));
                    offset++;
                    if (offset == 8)
                    {
                        offset = 0;
                        currentByte++;
                    }
                }
                else
                {
                    //result[currentByte] = (byte)(result[currentByte] & (0x00 << (8 - offset))); 
                    offset++;
                    if (offset == 8)
                    {
                        offset = 0;
                        currentByte++;
                    }
                    dict[(data[i - 2] << 8) + data[i - 1]] = data[i];
                    result[currentByte] = (byte)(result[currentByte] | (data[i] >> offset));
                    currentByte++;
                    result[currentByte] = (byte)(result[currentByte] | (data[i] << (8 - offset)));
                }
            }
            byte[] temp = new byte[currentByte + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = result[i];
            }
            Console.WriteLine("done");
            return temp;
        }

        public static byte[] AltDecompress(byte[] data)
        {
            var result = new List<byte>();
            byte[] dict = new byte[0xFFFF + 1];
            int offset = 0;
            result.Add(data[0]);
            result.Add(data[1]);
            int currentByte = 2;
            //Console.WriteLine(result.Count());
            while (currentByte < data.Length)
            {
                int codeBit = (data[currentByte] >> (8 - offset - 1)) % 2;
                //Console.Write("Check bit: ");
                //Console.WriteLine(codeBit);
                if (codeBit == 1)
                {
                    //Console.Write("Written ");
                    int t = result[result.Count - 2];
                    t <<= 8;
                    t += result[result.Count - 1];
                    result.Add(dict[t]);
                    
                    //Console.WriteLine(Convert.ToString((byte)(dict[t]), 2).PadLeft(8, '0'));
                    // Console.WriteLine(result.Count.ToString() + ") 1added, cuurent" + currentByte.ToString());
                    offset++;
                    if (offset == 8)
                    {
                        offset = 0;
                        currentByte++;
                        //Console.WriteLine("boom");
                    }
                    if (currentByte == data.Length - 1)
                    {
                        continue;
                    }
                }
                else
                {
                    if (currentByte == data.Length - 1)
                    {
                        break;
                    }
                    offset++;
                    if (offset == 8)
                    {
                        offset = 0;
                        currentByte++;
                        //Console.WriteLine("boom");
                    }

                    int leftMask = 0b1;
                    if (offset == 8)//todo
                    {
                        leftMask = 0;
                    }
                    else
                    {
                        for (int j = 0; j < 8-offset-1; j++)
                        {
                            leftMask <<= 1;
                            leftMask++;
                        }
                    }
                    int rightMask = 0b1;
                    
                        for (int j = 0; j < offset - 1; j++)
                        {
                            rightMask <<= 1;
                            rightMask++;
                        }
                        rightMask <<= (8 - offset);
                    

                    
                    //Console.WriteLine(currentByte);
                    //Console.WriteLine(offset);
                    if (currentByte < data.Length - 1)
                    {
                        ////Console.WriteLine("Masks");
                        ////Console.Write(Convert.ToString(leftMask, 2).PadLeft(8, '0'));
                        ////Console.Write(" ");
                        ////Console.WriteLine(Convert.ToString(rightMask, 2).PadLeft(8, '0'));

                        ////Console.WriteLine("Data");
                        ////Console.Write(Convert.ToString((byte)((data[currentByte])), 2).PadLeft(8, '0'));
                        ////Console.Write(" ");
                        ////Console.WriteLine(Convert.ToString((byte)(data[currentByte + 1]), 2).PadLeft(8, '0'));

                        ////Console.WriteLine("Result");
                        ////Console.Write(Convert.ToString((byte)((data[currentByte] & leftMask) << offset), 2).PadLeft(8, '0'));
                        ////Console.Write(" ");

                        //Console.Write("Written too ");
                        result.Add((byte)(((data[currentByte] & leftMask) << offset) + ((data[currentByte + 1] & rightMask) >> (8 - offset))));


                        //Console.WriteLine(Convert.ToString((byte)(((data[currentByte] & leftMask) << offset) + (data[currentByte + 1] & rightMask) >> (8 - offset)), 2).PadLeft(8, '0'));
                        int t = result[result.Count - 3];
                        t <<= 8;
                        t += result[result.Count - 2];
                        dict[t] = result[result.Count-1];
                    }
                    else
                    {
                        if (currentByte < data.Length)
                        {
                            //Console.WriteLine("written");
                            //Console.WriteLine(Convert.ToString(leftMask, 2).PadLeft(8, '0'));
                            //Console.WriteLine(Convert.ToString((byte)(((data[currentByte] & leftMask) << offset)), 2).PadLeft(8, '0'));

                            result.Add((byte)(((data[currentByte] & leftMask) << offset)));
                            int t = result[result.Count - 3];
                            t <<= 8;
                            t += result[result.Count - 2];
                            dict[t] = result[result.Count-1];
                            
                        }
                    }


                    //Console.WriteLine(result.Count.ToString() + ") 0added, cuurent" + currentByte.ToString());
                    currentByte++;

                }           



            }
            //Console.WriteLine("undone");

            //Console.WriteLine(result.Count());
            return result.ToArray();
        }
    }
}

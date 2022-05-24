using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWH.Tools;

namespace BWH
{
    public class BWCompression
    {
        public static byte[] Compress(byte[] Data)
        {
            byte[] bw = BurrowsWheelerTransform.Transform(Data);
            byte[] mtf = MoveToFrontCoding.Encode(bw);
            byte[] hf = HuffmanCoding.Encode(mtf);
            return hf;
        }

        public static byte[] Decompress(byte[] data)
        {
            byte[] dhf = HuffmanCoding.Decode(data);
            byte[] imtf = MoveToFrontCoding.Decode(dhf);
            byte[] ibw = BurrowsWheelerTransform.InverseTransform(imtf);
            return ibw;
        }

        public static void _Main(string[] args)
        {
            var data = new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x02 };
            var newData = Compress(data);
            for (int i = 0; i < data.Length; i++)
            {
                Console.Write(newData[i].ToString() + " ");
            }
            Console.WriteLine();
            data = null;
            data = Decompress(newData);
            for (int i = 0; i < data.Length; i++)
            {
                Console.Write(data[i].ToString() + " ");
            }
            Console.ReadKey();
            return;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredictionCompression
{
    public static class PredictionCompressor
    {
        public static byte[] Compress(byte[] data)
        {
            byte[] result = new byte[data.Length];
            byte[] dict = new byte[0xFFFF + 1];
            int offset = 0;
            result[0] = data[0];
            result[1] = data[1];
            int currentByte = 2;
            for (int i = 2; i < data.Length; i++)
            {
                if (dict[(data[i - 2] << 8) + data[i - 1]] == data[i])
                {
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
                    offset++;
                    if (offset == 8)
                    {
                        offset = 0;
                        currentByte++;
                    }
                    dict[(data[i - 2] << 8) + data[i - 1]] = data[i];
                    result[currentByte] = (byte)(result[currentByte] | (data[i] >> offset));
                    currentByte++;
                    if (currentByte == data.Length)
                    {
                        currentByte--;
                        break;
                    }
                    result[currentByte] = (byte)(result[currentByte] | (data[i] << (8 - offset))); // надо ли -1?
                }
            }
            byte[] temp = new byte[currentByte + 1];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = result[i];
            }
            return temp;
        }

        public static byte[] Decompress(byte[] data)
        {
            var result = new List<byte>();
            byte[] dict = new byte[0xFFFF + 1];
            int offset = 0;
            result.Add(data[0]);
            result.Add(data[1]);
            int currentByte = 2;
            while (currentByte < data.Length)
            {
                int codeBit = (data[currentByte] >> (8 - offset - 1)) % 2;
                if (codeBit == 1)
                {
                    int t = result[result.Count - 2];
                    t <<= 8;
                    t += result[result.Count - 1];
                    result.Add(dict[t]);
                    offset++;
                    if (offset == 8)
                    {
                        offset = 0;
                        currentByte++;
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
                    }

                    int leftMask = 0b1;
                    if (offset == 8)
                    {
                        leftMask = 0;
                    }
                    else
                    {
                        for (int j = 0; j < 8 - offset - 1; j++)
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

                    if (currentByte < data.Length - 1)
                    {
                        result.Add((byte)(((data[currentByte] & leftMask) << offset) + ((data[currentByte + 1] & rightMask) >> (8 - offset))));
                        int t = result[result.Count - 3];
                        t <<= 8;
                        t += result[result.Count - 2];
                        dict[t] = result[result.Count - 1];
                    }
                    else
                    {
                        if (currentByte < data.Length)
                        {
                            result.Add((byte)(((data[currentByte] & leftMask) << offset)));
                            int t = result[result.Count - 3];
                            t <<= 8;
                            t += result[result.Count - 2];
                            dict[t] = result[result.Count - 1];

                        }
                    }
                    currentByte++;

                }

            }
            return result.ToArray();
        }

    }
}

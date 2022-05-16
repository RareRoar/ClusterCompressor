using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWH.External;

namespace BWH.Tools
{
    public static class BurrowsWheelerTransform
    {
        public static byte[] Transform(byte[] input)
        {
            byte[] output = new byte[input.Length + 4];
            short[] newInput = new short[input.Length + 1];

            for (int i = 0; i < input.Length; i++)
                newInput[i] = (Int16)(input[i] + 1);

            newInput[input.Length] = 0;
            int[] suffixArray = SuffixArray.Construct(newInput);
            int end = 0;
            int outputInd = 0;
            for (int i = 0; i < suffixArray.Length; i++)
            {
                if (suffixArray[i] == 0)
                {
                    end = i;
                    continue;
                }
                output[outputInd] = (byte)(newInput[suffixArray[i] - 1] - 1);
                outputInd++;
            }
            byte[] endByte = IntToByteArr(end);
            endByte.CopyTo(output, input.Length);
            return output;
        }
        public static byte[] InverseTransform(byte[] input)
        {
            int Length = input.Length - 4;
            int I = ByteArrToInt(input, input.Length - 4);
            int[] freq = new int[256];
            Array.Clear(freq, 0, freq.Length);
            // T1: Number of Preceding Symbols Matching Symbol in Current Position.
            int[] T1 = new int[Length];
            // T2: Number of Symbols Lexicographically Less Than Current Symbol
            int[] T2 = new int[256];
            Array.Clear(T2, 0, T2.Length);
            // Construct T1
            for (int i = 0; i < Length; i++)
            {
                T1[i] = freq[input[i]];
                freq[input[i]]++;
            }

            // Construct T2
            // Add $ special symbol in consideration to be less than any symbol
            T2[0] = 1;
            for (int i = 1; i < 256; i++)
            {
                T2[i] = T2[i - 1] + freq[i - 1];
            }

            byte[] output = new byte[Length];
            int nxt = 0;
            for (int i = Length - 1; i >= 0; i--)
            {
                output[i] = input[nxt];
                int a = T1[nxt];
                int b = T2[input[nxt]];
                nxt = a + b;
                // Add $ special symbol index in consideration
                if (nxt >= I)
                {
                    nxt--;
                }
            }
            return output;
        }
        private static byte[] IntToByteArr(int i)
        {
            return BitConverter.GetBytes(i);
        }
        private static int ByteArrToInt(byte[] input, int StartIndex)
        {
            return BitConverter.ToInt32(input, StartIndex);
        }
    }
}
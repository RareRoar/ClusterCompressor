using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWH.External
{
    public static class SuffixArray
    {
        public static int[] Construct(short[] str)
        {
            int[] suffixArray = new int[str.Length];
            int[] rankArray = new int[str.Length];
            int[] tempRankArray = new int[str.Length];

            for (int i = 0; i < str.Length; i++)
            {
                suffixArray[i] = i;
                rankArray[i] = str[i];
            }

            for (int k = 1; k < str.Length; k *= 2)
            {
                CountSort(suffixArray, rankArray, k); // sort SA[i] based on RA[SA[i]+k]
                CountSort(suffixArray, rankArray, 0);// sort SA[i] based on RA[SA[i]]

                int newRank = 0;
                tempRankArray[suffixArray[0]] = 0;

                for (int i = 1; i < rankArray.Length; i++)
                {

                    int curRank = rankArray[suffixArray[i]];
                    int prevRank = rankArray[suffixArray[i - 1]];
                    int curRankWithOffset = 0;
                    int prevRankWithOffset = 0;

                    if (suffixArray[i] + k < rankArray.Length)
                        curRankWithOffset = rankArray[suffixArray[i] + k];

                    if (suffixArray[i - 1] + k < rankArray.Length)
                        prevRankWithOffset = rankArray[suffixArray[i - 1] + k];

                    if (curRank != prevRank || curRankWithOffset != prevRankWithOffset)
                        newRank++;

                    tempRankArray[suffixArray[i]] = newRank;
                }

                Array.Copy(tempRankArray, rankArray, rankArray.Length);

            }
            return suffixArray;
        }

        // sort suffixArray based on rankArray
        // suffixArray[i] mapped to rankArray[i+rankArrayOffset]
        // time Complexity  : O(N)
        private static void CountSort(int[] suffixArray, int[] rankArray, int rankArrayOffset)
        {
            int freqLength = Math.Max(rankArray.Max()+1, rankArray.Length);
            int[] frequency = new int[freqLength];
            int[] cumulativeFrequency = new int[freqLength]; // used as a start index
            int[] tempSuffixArray = new int[suffixArray.Length];

            for (int i = 0; i < rankArray.Length; i++)
            {
                int val = 0;
                if (i + rankArrayOffset < rankArray.Length)
                    val = rankArray[i + rankArrayOffset];
                frequency[val]++;
            }

            for (int i = 1; i < freqLength; i++)
                cumulativeFrequency[i] = cumulativeFrequency[i - 1] + frequency[i - 1];

            for (int i = 0; i < suffixArray.Length; i++)
            {
                int newIndex;
                if (suffixArray[i] + rankArrayOffset < rankArray.Length)
                {
                    newIndex = cumulativeFrequency[rankArray[suffixArray[i] + rankArrayOffset]];
                    cumulativeFrequency[rankArray[suffixArray[i] + rankArrayOffset]]++;
                }
                else
                {
                    newIndex = cumulativeFrequency[0];
                    cumulativeFrequency[0]++;
                }
                tempSuffixArray[newIndex] = suffixArray[i];
            }
            Array.Copy(tempSuffixArray, suffixArray, suffixArray.Length);
        }

    }
}

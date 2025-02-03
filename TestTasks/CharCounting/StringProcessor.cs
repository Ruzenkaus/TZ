using System;
using System.Collections.Generic;
using System.Linq;

namespace TestTasks.VowelCounting
{
    public class StringProcessor
    {
        public (char symbol, int count)[] GetCharCount(string veryLongString, char[] countedChars)
        {
            Dictionary<char, int> map = new Dictionary<char, int>();
            foreach (char c in countedChars)
            {
                if (!map.ContainsKey(c))
                    map[c] = 0;
            }

            foreach (char c in veryLongString)
            {
                if (map.ContainsKey(c))
                {
                    map[c]++;
                }
            }

            return map.Select(kvp => (kvp.Key, kvp.Value)).ToArray();
        }
    }
}

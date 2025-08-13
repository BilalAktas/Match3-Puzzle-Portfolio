using System.Collections.Generic;
using UnityEngine;

namespace Portfolio.Match3.Helpers
{
    public class Helper
    {
        /// <summary>
        /// Randomly shuffles the items in a list.
        /// </summary>
        public static void ShuffleList<T>(List<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var randIndex = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[randIndex];
                list[randIndex] = temp;
            }
        }
    }
}
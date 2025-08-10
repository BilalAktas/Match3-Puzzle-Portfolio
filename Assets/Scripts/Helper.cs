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
            for (var i = 0; i < list.Count; i++)
            {
                var randIndex = Random.Range(i, list.Count);
                T temp = list[i];
                list[i] = list[randIndex];
                list[randIndex] = temp;
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace NetcodePlus
{
    public class ListTool : MonoBehaviour
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public static void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
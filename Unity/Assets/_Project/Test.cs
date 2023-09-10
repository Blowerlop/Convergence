using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project
{
    public class Test : MonoBehaviour
    {
        [ConsoleCommand("nonOptional", "")]
        public static void NonOptional(string a)
        {
            Debug.Log(a);
        }

        [ConsoleCommand("optional", "")]
        public static void Optional(int a, string b = "string optional")
        {
            Debug.Log(b);
        }
    }
}

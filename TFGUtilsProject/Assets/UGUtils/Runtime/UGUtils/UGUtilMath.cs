using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UGU.Runtime.Utils
{
    public static class UGUtilMath
    {
        public static string ToPercent(float value, int decimalPlaces = 2)
        {
            return value.ToString("P" + decimalPlaces);
        }
    }
}
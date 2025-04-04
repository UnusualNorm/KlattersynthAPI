using System;

namespace UnityEngine
{
    public struct Mathf
    {
        public static float Abs(float f) => Math.Abs(f);
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }
        public static float Cos(float f) => (float)Math.Cos(f);
        public static float Exp(float power) => (float)Math.Exp(power);
        public static int Min(int a, int b) => Math.Min(a, b);
        public static float Sin(float f) => (float)Math.Sin(f);
    }
}

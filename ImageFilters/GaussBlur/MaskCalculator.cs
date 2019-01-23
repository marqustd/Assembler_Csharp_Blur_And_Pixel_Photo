using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageFilters.GaussBlur
{
    public static class MaskCalculator
    {
        public static double[] CalculateMask(int maskSize, double gaussRadius)
        {
            var mask = new List<double>();
            var positionDiff = (maskSize - 1) / 2;

            for (var i = 0; i < maskSize; i++)
            {
                for (var j = 0; j < maskSize; j++)
                {
                    mask.Add(CalculateWeight(j - positionDiff, i - positionDiff, gaussRadius));
                }
            }

            var sum = mask.Sum();
            mask = mask.Select(m => m /= sum).ToList();
            return mask.ToArray();
        }

        private static double CalculateWeight(int x, int y, double gaussRadius)
        {
            var power = -(x * x + y * y) / (2 * Math.Pow(gaussRadius, 2));
            var weight = 1 / (2 * Math.PI * Math.Pow(gaussRadius, 2)) * Math.Pow(Math.E, power);
            return weight;
        }
    }
}
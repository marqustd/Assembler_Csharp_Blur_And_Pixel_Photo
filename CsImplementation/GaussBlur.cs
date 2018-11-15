using System;
using System.Drawing;
using System.Threading.Tasks;

namespace CsImplementation
{
    public interface IGaussBlurCs
    {
        Task<Bitmap> Blur(Bitmap source, double radius);
    }

    public class GaussBlurCs : IGaussBlurCs
    {
        private readonly int maskSize = 7;
        private double[,] mask;

        public async Task<Bitmap> Blur(Bitmap source, double radius)
        {
            mask = PrepareMask(radius);
            var toReturn = new Bitmap(source);
            var halfMask = maskSize / 2;


            for (var y = halfMask; y < source.Height; y += maskSize - 1)
            for (var x = halfMask; x < source.Width; x += maskSize - 1)
            for (var y2 = y - halfMask; y2 < y + halfMask && y2 < source.Height; y2++)
            for (var x2 = x - halfMask; x2 < x + halfMask && x2 < source.Width; x2++)
            {
            }

            return source;
        }

        private double[,] PrepareMask(double radius)
        {
            var newMask = new double[maskSize, maskSize];
            var center = maskSize / 2;
            var twoSigmaSqr = 2 * radius * radius;
            var twoPiSigmaSqr = Math.PI * twoSigmaSqr;

            for (var y = 0; y < maskSize; y++)
            for (var x = 0; x < maskSize; x++)
                newMask[x, y] = 1 / twoPiSigmaSqr * Math.Pow(Math.E,
                                    -((center - x) * (center - x) + (center - y) * (center - y)) / twoSigmaSqr);

            return newMask;
        }
    }
}
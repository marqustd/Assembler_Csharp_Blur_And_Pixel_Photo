using System;
using System.Drawing;

namespace GaussBlur
{
    public interface IGaussBlurCs
    {
        Bitmap Blur(Bitmap source, double radius);
    }

    public class GaussBlurCs : IGaussBlurCs
    {
        public Bitmap Blur(Bitmap source, double radius)
        {
            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    source.SetPixel(x, y, Color.Red);
                }
            }

            return source;
        }
    }
}
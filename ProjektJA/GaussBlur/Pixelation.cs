using System.Drawing;
using System.Threading.Tasks;

namespace CsImplementation
{
    public interface IPixelation
    {
        Task<Bitmap> Pixelate(Bitmap source, int radius);
    }

    public class Pixelation : IPixelation
    {
        public async Task<Bitmap> Pixelate(Bitmap source, int radius)
        {
            var toReturn = new Bitmap(source);
            var halfRadius = radius / 2;

            for (var y = halfRadius; y < source.Height; y += radius)
            {
                for (var x = halfRadius; x < source.Width; x += radius)
                {
                    var mainPixel = source.GetPixel(x, y);

                    for (var y2 = y - halfRadius; y2 < y + halfRadius && y2<source.Height; y2++)
                    {
                        for (var x2 = x - halfRadius; x2 < x + halfRadius && x2 < source.Width; x2++)
                        {
                            toReturn.SetPixel(x2, y2, mainPixel);
                        }
                    }
                }
            }

            return toReturn;
        }
    }
}
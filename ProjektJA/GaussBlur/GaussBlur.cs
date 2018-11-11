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
        public async Task<Bitmap> Blur(Bitmap source, double radius)
        {
            for (var x = 0; x < source.Width; x++)
            {
                for (var y = 0; y < source.Height; y++)
                {
                    source.SetPixel(x, y, Color.Red);
                }
            }

            return source;
        }
    }
}
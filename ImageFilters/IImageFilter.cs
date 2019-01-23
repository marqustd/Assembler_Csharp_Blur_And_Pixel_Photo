using System.Drawing;

namespace ImageFilters
{
    public interface IImageFilter
    {
        Bitmap FilterUnsafe(Bitmap source);
    }
}
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageFilters.Pixelate
{
    public class PixelationAsm : IImageFilter
    {
        private readonly int radius;

        public PixelationAsm(int radius)
        {
            this.radius = radius;
        }

        public unsafe Bitmap FilterUnsafe(Bitmap source)
        {
            var bitmap = new Bitmap(source);
            var dataArraySize = bitmap.Width * bitmap.Height;
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            var original = (int*) data.Scan0;

            var filtered = new int[dataArraySize];
            fixed (int* filteredPtr = filtered)
            {
                PixelateUnsafe(original, filteredPtr, source);
                Marshal.Copy(filtered, 0, (IntPtr) original, dataArraySize);
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        [DllImport("AsmImplementation.dll", EntryPoint = "pixelRow")]
        private static extern unsafe void PixelRowUnsafeAsm(int columnsToFilter, int row,
            int* original, int* pixelated,
            int rowCounter, int sourceWidth, int bound, int radius);

        private unsafe void PixelateUnsafe(int* original, int* pixelated, Bitmap source)
        {
            var bound = (radius - 1) / 2;
            var columnsToFilter = source.Width / radius;
            var rowsToFilter = source.Height - source.Height % radius;
            var rowCounter = 0;

            var row = 0;
            for (; row < rowsToFilter; row++) //rzedy ktore sie mieszcza
            {
                PixelRowUnsafeAsm(columnsToFilter, row, original, pixelated, rowCounter++, source.Width, bound, radius);
                if (!(rowCounter < radius))
                {
                    rowCounter = 0;
                }
            }

            rowCounter = bound + 1;
            for (; row < source.Height; row++) //reszta pikseli
            {
                PixelRowUnsafeAsm(columnsToFilter, row, original, pixelated, rowCounter++, source.Width, bound, radius);
            }
        }
    }
}
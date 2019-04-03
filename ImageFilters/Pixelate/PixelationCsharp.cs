using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageFilters.Pixelate
{
    public class PixelationCsharp : IImageFilter
    {
        private readonly int radius;

        public PixelationCsharp(int radius)
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

        private unsafe void PixelateUnsafe(int* original, int* pixelated, Bitmap source)
        {
            var bound = (radius - 1) / 2;
            var columnsToFilter = source.Width / radius;
            var rowsToFilter = source.Height - source.Height % radius;
            var rowCounter = 0;

            var row = 0;
            for (; row < rowsToFilter; row++) //rzedy ktore sie mieszcza
            {
                PixelRowUnsafe(original, pixelated, rowCounter++, columnsToFilter, row, source.Width, bound);
                if (!(rowCounter < radius))
                {
                    rowCounter = 0;
                }
            }

            rowCounter = bound + 1;
            for (; row < source.Height; row++) //reszta pikseli
            {
                PixelRowUnsafe(original, pixelated, rowCounter++, columnsToFilter, row, source.Width, bound);
            }
        }

        private unsafe void PixelRowUnsafe(int* original, int* pixelated, int rowCounter, int columnsToFilter,
            int rowIndex, int arrayWidth, int bound)
        {
            var mainPixelIndex = (rowIndex - rowCounter) * arrayWidth + bound +
                                 bound * arrayWidth; //Pierwszy piksel z ktorego bierzemy kolor
            var pixelIndex = rowIndex * arrayWidth; //pierwszy indeks do zmiany koloru
            var pixel = original[mainPixelIndex];
            for (var i = 0; i < columnsToFilter; i++) //przejscie po calym rzedzie
            {
                for (var j = 0; j < radius; j++) //przejscie po radius
                {
                    pixelated[pixelIndex + j] = pixel;
                }

                pixelIndex += radius;
                mainPixelIndex += radius;
                pixel = original[mainPixelIndex];
            }

            var pixelsLeft = arrayWidth - columnsToFilter * radius; //reszta
            mainPixelIndex -= bound; //pierwszy piksel przy brzegu
            pixel = original[mainPixelIndex];

            for (var j = 0; j < pixelsLeft; j++)
            {
                pixelated[pixelIndex + j] = pixel;
            }
        }
    }
}
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
            var dataArraySize = Consts.BytesInPixel * bitmap.Width * bitmap.Height;
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            var original = (byte*) data.Scan0;

            var filtered = new byte[dataArraySize];
            fixed (byte* filteredPtr = filtered)
            {
                PixelateUnsafe(original, filteredPtr, source);
                Marshal.Copy(filtered, 0, (IntPtr) original, dataArraySize);
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        private unsafe void PixelateUnsafe(byte* original, byte* pixelated, Bitmap source)
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

        private unsafe void PixelRowUnsafe(byte* original, byte* pixelated, int rowCounter, int columnsToFilter,
            int rowIndex, int arrayWidth, int bound)
        {
            var mainPixelIndex = Consts.BytesInPixel *
                                 ((rowIndex - rowCounter) * arrayWidth + bound +
                                  bound * arrayWidth); //Pierwszy piksel z ktorego bierzemy kolor
            var pixelIndex = rowIndex * arrayWidth * Consts.BytesInPixel; //pierwszy indeks do zmiany koloru
            var R = original[mainPixelIndex];
            var G = original[mainPixelIndex + 1];
            var B = original[mainPixelIndex + 2];
            var A = original[mainPixelIndex + 3];
            for (var i = 0; i < columnsToFilter; i++) //przejscie po calym rzedzie
            {
                for (var j = 0; j < radius; j++) //przejscie po radius
                {
                    pixelated[pixelIndex + j * Consts.BytesInPixel] = R;
                    pixelated[pixelIndex + j * Consts.BytesInPixel + 1] = G;
                    pixelated[pixelIndex + j * Consts.BytesInPixel + 2] = B;
                    pixelated[pixelIndex + j * Consts.BytesInPixel + 3] = A;
                }

                pixelIndex += radius * Consts.BytesInPixel;
                mainPixelIndex += radius * Consts.BytesInPixel;
                R = original[mainPixelIndex];
                G = original[mainPixelIndex + 1];
                B = original[mainPixelIndex + 2];
                A = original[mainPixelIndex + 3];
            }

            var pixelsLeft = arrayWidth - columnsToFilter * radius; //reszta
            mainPixelIndex -= bound * Consts.BytesInPixel; //pierwszy piksel przy brzegu
            R = original[mainPixelIndex];
            G = original[mainPixelIndex + 1];
            B = original[mainPixelIndex + 2];
            A = original[mainPixelIndex + 3];

            for (var j = 0; j < pixelsLeft; j++)
            {
                pixelated[pixelIndex + j * Consts.BytesInPixel] = R;
                pixelated[pixelIndex + j * Consts.BytesInPixel + 1] = G;
                pixelated[pixelIndex + j * Consts.BytesInPixel + 2] = B;
                pixelated[pixelIndex + j * Consts.BytesInPixel + 3] = A;
            }
        }
    }
}
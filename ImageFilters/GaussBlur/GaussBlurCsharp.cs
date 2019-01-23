using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageFilters.GaussBlur
{
    public class GaussBlurCsharp : IImageFilter
    {
        private const int BytesInPixel = 4;
        private readonly double[] mask;
        private readonly int maskSize;

        public GaussBlurCsharp(int maskSize, double gaussRadius)
        {
            this.maskSize = maskSize;
            mask = MaskCalculator.CalculateMask(maskSize, gaussRadius);
        }

        public unsafe Bitmap FilterUnsafe(Bitmap source)
        {
            var bitmap = new Bitmap(source);
            var dataArraySize = BytesInPixel * bitmap.Width * bitmap.Height;
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            var original = (byte*) data.Scan0;

            var filtered = new byte[dataArraySize];
            fixed (byte* filteredPtr = filtered)
            {
                BlurUnsafe(original, filteredPtr, source);
                Marshal.Copy(filtered, 0, (IntPtr) original, dataArraySize);
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        private unsafe void BlurUnsafe(byte* original, byte* blured, Bitmap source)
        {
            var boundPixelWidth = (maskSize - 1) / 2;
            var arraySize = source.Width * source.Height;
            var boundTopBottomArraySize = source.Width * boundPixelWidth;
            var bottomBoundStartIndex = (arraySize - boundTopBottomArraySize) * BytesInPixel;

            //  Set top and bottom bound
            for (var i = 0; i < boundTopBottomArraySize; i++)
            {
                //Top bound
                blured[i * BytesInPixel] = original[i * BytesInPixel];
                blured[i * BytesInPixel + 1] = original[i * BytesInPixel + 1];
                blured[i * BytesInPixel + 2] = original[i * BytesInPixel + 2];
                blured[i * BytesInPixel + 3] = original[i * BytesInPixel + 3];
                //Bottom bound
                blured[bottomBoundStartIndex + i * BytesInPixel] = original[bottomBoundStartIndex + i * BytesInPixel];
                blured[bottomBoundStartIndex + i * BytesInPixel + 1] =
                    original[bottomBoundStartIndex + i * BytesInPixel + 1];
                blured[bottomBoundStartIndex + i * BytesInPixel + 2] =
                    original[bottomBoundStartIndex + i * BytesInPixel + 2];
                blured[bottomBoundStartIndex + i * BytesInPixel + 3] =
                    original[bottomBoundStartIndex + i * BytesInPixel + 3];
            }

            var index = boundTopBottomArraySize * BytesInPixel;
            var realWidth = (source.Width - 2 * boundPixelWidth) * BytesInPixel;
            while (index < bottomBoundStartIndex)
            {
                //Set start of the row
                for (var i = 0; i < boundPixelWidth; i++)
                {
                    //R
                    blured[index + i * BytesInPixel] = original[index + i * BytesInPixel];
                    //G
                    blured[index + i * BytesInPixel + 1] = original[index + i * BytesInPixel + 1];
                    //B
                    blured[index + i * BytesInPixel + 2] = original[index + i * BytesInPixel + 2];
                    //A
                    blured[index + i * BytesInPixel + 3] = original[index + i * BytesInPixel + 3];
                }

                index += boundPixelWidth * BytesInPixel;

                //Apply gauss filter
                for (var i = 0; i < realWidth; i += BytesInPixel)
                {
                    BlurUnsafe(original, blured, source.Width, index + i);
                }

                index += realWidth;

                //Set end of the row
                for (var i = 0; i < boundPixelWidth; i++)
                {
                    blured[index + i * BytesInPixel] = original[index + i * BytesInPixel];
                    blured[index + i * BytesInPixel + 1] = original[index + i * BytesInPixel + 1];
                    blured[index + i * BytesInPixel + 2] = original[index + i * BytesInPixel + 2];
                    blured[index + i * BytesInPixel + 3] = original[index + i * BytesInPixel + 3];
                }

                index += boundPixelWidth * BytesInPixel;
            }
        }

        private unsafe void BlurUnsafe(byte* original, byte* blured, int arrayWidth, int index)
        {
            var positionDiff = (maskSize - 1) / 2;
            var R = 0d;
            var G = 0d;
            var B = 0d;
            var A = 0d;
            var maskCounter = 0;
            int indexR, indexG, indexB, indexA;
            for (var y = -positionDiff; y <= positionDiff; y++)
            {
                for (var x = -positionDiff; x <= positionDiff; x++)
                {
                    indexR = index + (x + y * arrayWidth) * BytesInPixel;
                    indexG = index + (x + y * arrayWidth) * BytesInPixel + 1;
                    indexB = index + (x + y * arrayWidth) * BytesInPixel + 2;
                    indexA = index + (x + y * arrayWidth) * BytesInPixel + 3;

                    R += original[indexR] * mask[maskCounter];
                    G += original[indexG] * mask[maskCounter];
                    B += original[indexB] * mask[maskCounter];
                    A += original[indexA] * mask[maskCounter];
                    maskCounter++;
                }
            }

            blured[index] = (byte) R;
            blured[index + 1] = (byte) G;
            blured[index + 2] = (byte) B;
            blured[index + 3] = (byte) A;
        }
    }
}
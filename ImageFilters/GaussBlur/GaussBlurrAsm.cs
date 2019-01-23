using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageFilters.GaussBlur
{
    public class GaussBlurrAsm : IImageFilter
    {
        private readonly double[] mask;
        private readonly int maskSize;

        public GaussBlurrAsm(int maskSize, double gaussRadius)
        {
            this.maskSize = maskSize;
            mask = MaskCalculator.CalculateMask(maskSize, gaussRadius);
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
                BlurUnsafe(original, filteredPtr, source);
                Marshal.Copy(filtered, 0, (IntPtr) original, dataArraySize);
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        [DllImport("AsmImplementation.dll", EntryPoint = "gauss")]
        private static extern unsafe void BlurUnsafeAsm(int index, int arrayWidth, byte* original, byte* filtered,
            double* mask, int maskSize);

        [DllImport("AsmImplementation.dll", EntryPoint = "border")]
        private static extern unsafe void SetTopBottomBorderAsm(byte* original, byte* filtered, int topBottomBorderSize,
            int bottomBoundStartIndex);


        private unsafe void BlurUnsafe(byte* original, byte* blured, Bitmap source)
        {
            var boundPixelWidth = (maskSize - 1) / 2;
            var arraySize = source.Width * source.Height;
            var boundTopBottomArraySizeInPixels = source.Width * boundPixelWidth;
            var boundTopBottomArraySizeInBytes = boundTopBottomArraySizeInPixels * Consts.BytesInPixel;
            var bottomBoundStartIndex = (arraySize - boundTopBottomArraySizeInPixels) * Consts.BytesInPixel;
            var arrayWidth = source.Width;
            int byteIndex;

            //  Set top and bottom bound
            SetTopBottomBorderAsm(original, blured, boundTopBottomArraySizeInBytes, bottomBoundStartIndex);

            var index = boundTopBottomArraySizeInBytes;
            var realWidth = (source.Width - 2 * boundPixelWidth) * Consts.BytesInPixel;
            while (index < bottomBoundStartIndex)
            {
                //Set start of the row
                for (var i = 0; i < boundPixelWidth; i++)
                {
                    byteIndex = i * Consts.BytesInPixel;
                    blured[index + byteIndex] = original[index + byteIndex];
                    blured[index + byteIndex + 1] = original[index + byteIndex + 1];
                    blured[index + byteIndex + 2] = original[index + byteIndex + 2];
                    blured[index + byteIndex + 3] = original[index + byteIndex + 3];
                }

                index += boundPixelWidth * Consts.BytesInPixel;

                fixed (double* maskPtr = mask)
                {
                    BlurUnsafeAsm(index, arrayWidth, original, blured, maskPtr, maskSize);
                }

                index += realWidth;

                for (var i = 0; i < boundPixelWidth; i++)
                {
                    byteIndex = i * Consts.BytesInPixel;
                    blured[index + byteIndex] = original[index + byteIndex];
                    blured[index + byteIndex + 1] = original[index + byteIndex + 1];
                    blured[index + byteIndex + 2] = original[index + byteIndex + 2];
                    blured[index + byteIndex + 3] = original[index + byteIndex + 3];
                }

                index += boundPixelWidth * Consts.BytesInPixel;
            }
        }
    }
}
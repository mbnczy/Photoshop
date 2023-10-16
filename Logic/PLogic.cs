using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Pipelines;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;

namespace Photoshop.Logic
{
	public class PLogic
	{

        private static PLogic instance = null;
        public static PLogic Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PLogic();
                }
                return instance;
            }
        }

        public Stack<byte[]> Images = new Stack<byte[]>();

        public PLogic()
        {

            System.Drawing.AsposeDrawing.License lic = new System.Drawing.AsposeDrawing.License();
            //lic.SetLicense("Aspose.Drawing.lic");
        }

        public bool Invert()
        {
            Bitmap b = ConvertByteArrayToBitmap(Images.Peek());

            // GDI+ still lies to us - the return format is BGR, NOT RGB. 
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - b.Width * 3;
                int nWidth = b.Width * 3;
                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        p[0] = (byte)(255 - p[0]);
                        ++p;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);


            Images.Push(ConvertBitmapToByteArray(b,ImageFormat.Jpeg));
            return true;
        }

        public bool GrayScale()
        {
            Bitmap b = ConvertByteArrayToBitmap(Images.Peek());

            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - b.Width * 3;

                byte red, green, blue;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];

                        p[0] = p[1] = p[2] = (byte)(.299 * red
                            + .587 * green
                            + .114 * blue);

                        p += 3;
                    }
                    p += nOffset;
                }
                b.UnlockBits(bmData);


                Images.Push(ConvertBitmapToByteArray(b, ImageFormat.Jpeg));
                return true;
            }
        }

        public bool Brightness(int brightness)
        {
            Bitmap b = ConvertByteArrayToBitmap(Images.Peek());

            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            int nWidth = b.Width * 3;

            

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - b.Width * 3;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        int nVal = (int)(p[0] + brightness);

                        if (nVal < 0) nVal = 0;
                        if (nVal > 255) nVal = 255;

                        p[0] = (byte)nVal;

                        ++p;
                    }
                    p += nOffset;
                }
                b.UnlockBits(bmData);


                Images.Push(ConvertBitmapToByteArray(b, ImageFormat.Jpeg));

                return true;
            }
            
        }

        public bool Contrast(int nContrast)
        {
            Bitmap b = ConvertByteArrayToBitmap(Images.Peek());

            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            int nWidth = b.Width * 3;


            if (nContrast < -100) return false;
            if (nContrast > 100) return false;

            double pixel = 0, contrast = (100.0 + nContrast) / 100.0;

            contrast *= contrast;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - b.Width * 3;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        int red = p[2];

                        pixel = red / 255.0;
                        pixel -= 0.5;
                        pixel *= contrast;
                        pixel += 0.5;
                        pixel *= 255;
                        if (pixel < 0) pixel = 0;
                        if (pixel > 255) pixel = 255;

                        p[2] = (byte)pixel;

                        ++p;
                    }
                    p += nOffset;
                }
                b.UnlockBits(bmData);

                Images.Push(ConvertBitmapToByteArray(b, ImageFormat.Jpeg));

                return true;
            }
        }

        public bool Logarithm(int nLog)
        {
            Bitmap b = ConvertByteArrayToBitmap(Images.Peek());

            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            int nWidth = b.Width * 3;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - b.Width * 3;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        //g(x, y) = c * log [1.0 + f(x, y)]
                        double pixelValue = p[0];
                        double transformedValue = nLog * Math.Log(1.0 + pixelValue);

                        byte newValue = (byte)Math.Min(255, Math.Max(0, transformedValue));

                        p[0] = newValue;
                        ++p;
                    }
                    p += nOffset;
                }
                b.UnlockBits(bmData);

                Images.Push(ConvertBitmapToByteArray(b, ImageFormat.Jpeg));

                return true;
            }
        }

        public bool Gamma(double red, double green, double blue)
        {
            Bitmap b = ConvertByteArrayToBitmap(Images.Peek());

            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            int nWidth = b.Width * 3;

            byte[] redGamma = new byte[256];
            byte[] greenGamma = new byte[256];
            byte[] blueGamma = new byte[256];

            for (int i = 0; i < 256; ++i)
            {
                redGamma[i] = (byte)Math.Min(255, (int)((255.0
                    * Math.Pow(i / 255.0, 1.0 / red)) + 0.5));
                greenGamma[i] = (byte)Math.Min(255, (int)((255.0
                    * Math.Pow(i / 255.0, 1.0 / green)) + 0.5));
                blueGamma[i] = (byte)Math.Min(255, (int)((255.0
                    * Math.Pow(i / 255.0, 1.0 / blue)) + 0.5));
            }

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - b.Width * 3;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        if (x % 3 == 0) // Red component
                            p[0] = blueGamma[p[0]];
                        else if (x % 3 == 1) // Green component
                            p[1] = redGamma[p[1]];
                        else if (x % 3 == 2) // Blue component
                            p[2] = greenGamma[p[2]];

                        ++p;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);


            Images.Push(ConvertBitmapToByteArray(b, ImageFormat.Jpeg));

            return true;
        }
        public bool Gamma2(double gammaRed, double gammaGreen, double gammaBlue)
        {
            Bitmap b = ConvertByteArrayToBitmap(Images.Peek());
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            int nWidth = b.Width * 3;
            // Define the gamma correction factors for each channel
            double gammaCorrectionRed = 1.0 / gammaRed;
            double gammaCorrectionGreen = 1.0 / gammaGreen;
            double gammaCorrectionBlue = 1.0 / gammaBlue;

            
            int bytesPerPixel = Image.GetPixelFormatSize(b.PixelFormat) / 8;

            unsafe
            {
                byte* ptr = (byte*)bmData.Scan0;

                for (int y = 0; y < b.Height; y++)
                {
                    for (int x = 0; x < b.Width; x++)
                    {
                        int offset = y * bmData.Stride + x * bytesPerPixel;

                        byte red = ptr[offset + 2];
                        byte green = ptr[offset + 1];
                        byte blue = ptr[offset];

                        byte newRed = (byte)(255 * Math.Pow(red / 255.0, gammaCorrectionRed));
                        byte newGreen = (byte)(255 * Math.Pow(green / 255.0, gammaCorrectionGreen));
                        byte newBlue = (byte)(255 * Math.Pow(blue / 255.0, gammaCorrectionBlue));

                        ptr[offset + 2] = newRed;
                        ptr[offset + 1] = newGreen;
                        ptr[offset] = newBlue;
                    }
                }
            }

            b.UnlockBits(bmData);

            Images.Push(ConvertBitmapToByteArray(b, ImageFormat.Jpeg));
            return true;
        }

        public int[] CreateHistogramWPtr254()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());
            int[] histogram = new int[256]; // 256 possible intensity values

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int stride = bitmapData.Stride;
            unsafe
            {
                byte* scan0 = (byte*)bitmapData.Scan0.ToPointer();

                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte* row = scan0 + (y * stride);
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int intensity = (int)(0.299 * row[x * 3 + 2] + 0.587 * row[x * 3 + 1] + 0.114 * row[x * 3]);
                        if (intensity != 255)
                        {
                            histogram[intensity]++;
                        }
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);

            return histogram;
        }
        public int[] CreateHistogramWPtr()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());
            int[] histogram = new int[256]; // 256 possible intensity values

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int stride = bitmapData.Stride;
            unsafe
            {
                byte* scan0 = (byte*)bitmapData.Scan0.ToPointer();

                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte* row = scan0 + (y * stride);
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int intensity = (int)(0.299 * row[x * 3 + 2] + 0.587 * row[x * 3 + 1] + 0.114 * row[x * 3]);
                        histogram[intensity]++;
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);

            return histogram;
        }
        public int[] CreateHistogram()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());
            int[] histogram = new int[256]; // 256 possible intensity values

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    int intensity = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                    //int intensity = (int)(pixel.R + pixel.G + pixel.B);
                    histogram[intensity]++;
                }
            }

            return histogram;
        }
        public int[] CreateHistogramWColor254()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());
            int[] histogram = new int[256]; // 256 possible intensity values

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    int intensity = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                    //int intensity = (int)(pixel.R + pixel.G + pixel.B);
                    if (intensity!=255)
                    {
                        histogram[intensity]++;
                    }
                }
            }

            return histogram;
        }
        public int[] Bad_hist()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());

            int totalPixels = bitmap.Height * bitmap.Width;
            int[] histogram = new int[256];
            BitmapData imageData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte[] pixels = new byte[totalPixels];
            Marshal.Copy(imageData.Scan0, pixels, 0, totalPixels);
            bitmap.UnlockBits(imageData);

            for (int i = 0; i < totalPixels; i++)
            {
                histogram[pixels[i]]++;
            }

            return histogram;
        }
        public bool LSD_Filter()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());

            int totalPixels = bitmap.Height * bitmap.Width;

            int[] histogram = new int[256];
            BitmapData imageData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte[] pixels = new byte[totalPixels];
            Marshal.Copy(imageData.Scan0, pixels, 0, totalPixels);
            bitmap.UnlockBits(imageData);

            for (int i = 0; i < totalPixels; i++)
            {
                histogram[pixels[i]]++;
            }

            int[] cdf = new int[256];
            cdf[0] = histogram[0];
            for (int i = 1; i < 256; i++)
            {
                cdf[i] = cdf[i - 1] + histogram[i];
            }

            BitmapData outputData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            byte[] outputPixels = new byte[totalPixels];

            for (int i = 0; i < totalPixels; i++)
            {
                outputPixels[i] = (byte)((cdf[pixels[i]] * 255) / totalPixels);
            }

            Marshal.Copy(outputPixels, 0, outputData.Scan0, totalPixels);

            bitmap.UnlockBits(outputData);
            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));

            return true;
        }
        public bool HistogramEqualization()
        {
            int[] histogram_ = CreateHistogram();


            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());

            int totalPixels = bitmap.Height * bitmap.Width;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, bitmap.PixelFormat);
            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;

                // Calculate the cumulative distribution function (CDF)
                int[] cdf = new int[256];
                cdf[0] = histogram_[0];
                for (int i = 1; i < 256; i++)
                {
                    cdf[i] = cdf[i - 1] + histogram_[i];
                }

                // Equalize the image using pointers
                for (int i = 0; i < totalPixels*3; i++)
                {
                    ptr[i] = (byte)((cdf[ptr[i]] * 255) / totalPixels);
                }
            }

            bitmap.UnlockBits(bitmapData);
            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));

            return true;
        }

        public bool ApplyAverageFilter(int filterSize)
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());
            int halfSize = filterSize / 2;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer();

                int stride = bitmapData.Stride;

                for (int y = halfSize; y < bitmap.Height - halfSize; y++)
                {
                    for (int x = halfSize; x < bitmap.Width - halfSize; x++)
                    {
                        int totalRed = 0, totalGreen = 0, totalBlue = 0;

                        for (int j = -halfSize; j <= halfSize; j++)
                        {
                            for (int i = -halfSize; i <= halfSize; i++)
                            {
                                byte* pixelPtr = ptr + (y + j) * stride + (x + i) * 3;

                                totalRed += pixelPtr[2];
                                totalGreen += pixelPtr[1];
                                totalBlue += pixelPtr[0];
                            }
                        }

                        int divisor = filterSize * filterSize;
                        int newRed = totalRed / divisor;
                        int newGreen = totalGreen / divisor;
                        int newBlue = totalBlue / divisor;

                        byte* outputPixelPtr = ptr + y * stride + x * 3;
                        outputPixelPtr[2] = (byte)newRed;
                        outputPixelPtr[1] = (byte)newGreen;
                        outputPixelPtr[0] = (byte)newBlue;
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);
            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));
            return true;
        }

        public bool ApplyGaussianFilter(int size, double sigma)
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());
            int width = bitmap.Width;
            int height = bitmap.Height;
            Bitmap outputImage = new Bitmap(width, height);

            double[,] kernel = new double[size, size];
            double sum = 0.0;
            int radius = size / 2;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    double exponent = -(x * x + y * y) / (2.0 * sigma * sigma);
                    kernel[y + radius, x + radius] = Math.Exp(exponent) / (2 * Math.PI * sigma * sigma);
                    sum += kernel[y + radius, x + radius];
                }
            }

            // Normalize the kernel
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[y, x] /= sum;
                }
            }
            
            //Apply the Gaussian filter
            int kernelCenter = size / 2;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double r = 0, g = 0, b = 0;

                    for (int i = 0; i < size; i++)
                    {
                        for (int j = 0; j < size; j++)
                        {
                            int px = x + i - kernelCenter;
                            int py = y + j - kernelCenter;

                            if (px >= 0 && px < width && py >= 0 && py < height)
                            {
                                Color pixel = bitmap.GetPixel(px, py);
                                double weight = kernel[i, j];

                                r += pixel.R * weight;
                                g += pixel.G * weight;
                                b += pixel.B * weight;
                            }

                        }
                    }
                    r = Math.Min(255, Math.Max(0, r));
                    g = Math.Min(255, Math.Max(0, g));
                    b = Math.Min(255, Math.Max(0, b));


                    Color filteredColor = Color.FromArgb((int)r, (int)g, (int)b);
                    outputImage.SetPixel(x, y, filteredColor);
                }
            }
            Images.Push(ConvertBitmapToByteArray(outputImage, ImageFormat.Jpeg));

            return true;
        }
        public bool ApplyGaussianFilter2(int size, double weight)
        {
            //kernel
            double[,] kernel = new double[size, size];
            double kernelSum = 0;
            double dist = 0;
            int halfsize = (size - 1) / 2;
            double constant = 1d / (2 * Math.PI * weight * weight);
            for (int y = -halfsize; y <= halfsize; y++)
            {
                for (int x = -halfsize; x <= halfsize; x++)
                {
                    dist = ((y * y) + (x * x)) / (2 * weight * weight);
                    kernel[y + halfsize, x + halfsize] = constant * Math.Exp(-dist);
                    kernelSum += kernel[y + halfsize, x + halfsize];
                }
            }
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[y, x] = kernel[y, x] * 1d / kernelSum;
                }
            }

            //apply filter
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());
            int width = bitmap.Width;
            int height = bitmap.Height;
            BitmapData srcData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            int bytes = srcData.Stride * srcData.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];

            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            bitmap.UnlockBits(srcData);

            double[] rgb = new double[3];
            int kcenter = 0;
            int kpixel = 0;
            for (int y = halfsize; y < height - halfsize; y++)
            {
                for (int x = halfsize; x < width - halfsize; x++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        rgb[c] = 0.0;
                    }
                    kcenter = y * srcData.Stride + x * 4;
                    for (int fy = -halfsize; fy <= halfsize; fy++)
                    {
                        for (int fx = -halfsize; fx <= halfsize; fx++)
                        {
                            kpixel = kcenter + fy * srcData.Stride + fx * 4;
                            for (int c = 0; c < 3; c++)
                            {
                                rgb[c] += (double)(buffer[kpixel + c]) * kernel[fy + halfsize, fx + halfsize];
                            }
                        }
                    }
                    for (int c = 0; c < 3; c++)
                    {
                        if (rgb[c] > 255)
                        {
                            rgb[c] = 255;
                        }
                        else if (rgb[c] < 0)
                        {
                            rgb[c] = 0;
                        }
                    }
                    for (int c = 0; c < 3; c++)
                    {
                        result[kcenter + c] = (byte)rgb[c];
                    }
                    result[kcenter + 3] = 255;
                }
            }
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, resultData.Scan0, bytes);
            resultImage.UnlockBits(resultData);

            Images.Push(ConvertBitmapToByteArray(resultImage, ImageFormat.Jpeg));

            return true;
        }

        public bool ApplySobelEdgeDetection()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());

            int width = bitmap.Width;
            int height = bitmap.Height;

            int[,] sobelX = new int[,]
            {
                { 1, 0, -1 },
                { 2, 0, -2 },
                { 1, 0, -1 }
            };

            int[,] sobelY = new int[,]
            {
                { 1, 2, 1 },
                { 0, 0, 0 },
                { -1, -2, -1 }
            };

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
                int stride = bitmapData.Stride;

                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        int gxRed = 0, gxGreen = 0, gxBlue = 0;
                        int gyRed = 0, gyGreen = 0, gyBlue = 0;

                        for (int j = -1; j <= 1; j++)
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                byte* pixelPtr = ptr + (y + j) * stride + (x + i) * 3;
                                int sobelXValue = sobelX[j + 1, i + 1];
                                int sobelYValue = sobelY[j + 1, i + 1];

                                gxRed += pixelPtr[2] * sobelXValue;
                                gxGreen += pixelPtr[1] * sobelXValue;
                                gxBlue += pixelPtr[0] * sobelXValue;

                                gyRed += pixelPtr[2] * sobelYValue;
                                gyGreen += pixelPtr[1] * sobelYValue;
                                gyBlue += pixelPtr[0] * sobelYValue;
                            }
                        }

                        // Gradiens nagyság számítása

                        // Küszöbölés: Ha a gradiens nagysága meghaladja a küszöbértéket, ott él van
                        double redGradient = Math.Sqrt((gxRed * gxRed) + (gyRed * gyRed));
                        double greenGradient = Math.Sqrt((gxGreen * gxGreen) + (gyGreen * gyGreen));
                        double blueGradient = Math.Sqrt((gxBlue * gxBlue) + (gyBlue * gyBlue));
                        double gradientMagnitude = (redGradient + greenGradient + blueGradient)/3;
                        if (gradientMagnitude > 200)
                        {
                            redGradient = 100;
                            greenGradient = 100;
                            blueGradient = 100;
                        }
                        else
                        {
                            redGradient = 0;
                            greenGradient = 0;
                            blueGradient = 0;
                        }
                        //byte* outputPixelPtr = ptr + y * stride + x * 3;

                        //outputPixelPtr[2] = (byte)redGradient;
                        //outputPixelPtr[1] = (byte)greenGradient;
                        //outputPixelPtr[0] = (byte)blueGradient;
                        Color edgeColor = Color.FromArgb((int)redGradient, (int)greenGradient, (int)blueGradient);
                        bitmap.SetPixel(x, y, edgeColor);
                    }
                }
            }


            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));
            bitmap.UnlockBits(bitmapData);
            return true;
        }

        public bool ApplyLaplaceEdgeDetection()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());
            int width = bitmap.Width;
            int height = bitmap.Height;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int[,] laplaceKernel = new int[,]
            {
                { 0, 1, 0 },
                { 1, -4, 1 },
                { 0, 1, 0 }
            };

            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
                int stride = bitmapData.Stride;
                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        int laplaceSum = 0;

                        for (int j = -1; j <= 1; j++)
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                byte* pixelPtr = ptr + (y + j) * stride + (x + i) * 3;

                                laplaceSum += laplaceKernel[j + 1, i + 1] * pixelPtr[2]; 
                            }
                        }

                        int newPixelValue = Math.Min(Math.Max(laplaceSum, 0), 255);

                        Color edgeColor = Color.FromArgb(newPixelValue, newPixelValue, newPixelValue);
                        bitmap.SetPixel(x, y, edgeColor);
                        //byte* outputPixelPtr = ptr + (y) * stride + (x) * 3;
                        //outputPixelPtr[2] = (byte)newPixelValue;
                        //outputPixelPtr[1] = (byte)newPixelValue;
                        //outputPixelPtr[0] = (byte)newPixelValue;
                    }
                }
            }
            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));
            bitmap.UnlockBits(bitmapData);
            return true;
        }
        public bool ApplyLoGEdgeDetection()
        {
            //double[,] gaussianKernel = new double[,]
            //{
            //    { 0.0625, 0.125, 0.0625 },
            //    { 0.125, 0.25, 0.125 },
            //    { 0.0625, 0.125, 0.0625 }
            //};

            int[,] laplaceKernel = new int[,]
            {
                { 0, 1, 0 },
                { 1, -4, 1 },
                { 0, 1, 0 }
            };

            // Apply Gaussian filter to the image
            this.ApplyGaussianFilter(5, 3);


            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());
            int width = bitmap.Width;
            int height = bitmap.Height;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
                int stride = bitmapData.Stride;

                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++)
                    {
                        int laplaceSum = 0;

                        for (int j = -1; j <= 1; j++)
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                byte* pixelPtr = ptr + (y + j) * stride + (x + i) * 3;

                                laplaceSum += laplaceKernel[j + 1, i + 1] * pixelPtr[2]; // Red channel
                            }
                        }

                        int newPixelValue = Math.Min(Math.Max(laplaceSum, 0), 255);
                        Color edgeColor = Color.FromArgb(newPixelValue, newPixelValue, newPixelValue);
                        bitmap.SetPixel(x, y, edgeColor);
                    }
                }
            }

            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));
            return true;
        }

        private void ApplyFilter(Bitmap bitmap, double[,] filter)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            Bitmap tempImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            int filterSize = filter.GetLength(0);
            int filterRadius = filterSize / 2;

            for (int y = filterRadius; y < height - filterRadius; y++)
            {
                for (int x = filterRadius; x < width - filterRadius; x++)
                {
                    double redSum = 0.0;

                    for (int j = -filterRadius; j <= filterRadius; j++)
                    {
                        for (int i = -filterRadius; i <= filterRadius; i++)
                        {
                            Color pixel = bitmap.GetPixel(x + i, y + j);
                            redSum += pixel.R * filter[j + filterRadius, i + filterRadius];
                        }
                    }

                    int newRedValue = (int)Math.Min(Math.Max(redSum, 0), 255);
                    Color newColor = Color.FromArgb(newRedValue, newRedValue, newRedValue);
                    tempImage.SetPixel(x, y, newColor);
                }
            }

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(tempImage, new Point(0, 0));
            }
        }

        public bool ApplyHarrisCornerDetection()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());
            int width = bitmap.Width;
            int height = bitmap.Height;
            int stride = width * 3;
            int threshold = 1000;
            double k = -0.04; // Harris corner constant (adjust as needed)

            List<Point> corners = new List<Point>();

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* inputPtr = (byte*)bitmapData.Scan0.ToPointer();
                int offset = stride - width * 3;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Calculate Harris corner response
                        double IxIx = 0, IyIy = 0, IxIy = 0;

                        for (int j = -1; j <= 1; j++)
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                byte* pixelPtr = inputPtr + (y + j) * stride + (x + i) * 3;
                                double grayValue = 0.299 * pixelPtr[2] + 0.587 * pixelPtr[1] + 0.114 * pixelPtr[0];

                                IxIx += grayValue * grayValue;
                                IyIy += grayValue * grayValue;
                                IxIy += grayValue * grayValue;
                            }
                        }

                        // Harris corner response formula
                        double detM = (IxIx * IyIy) - (IxIy * IxIy);
                        double traceM = IxIx + IyIy;
                        double cornerResponse = detM - k * (traceM * traceM);

                        // Mark the pixel as a corner in the input image
                        if (cornerResponse > threshold)
                        {
                            corners.Add(new Point(x, y));
                        }
                        
                        inputPtr += 3;
                    }

                    inputPtr += offset;
                }
            }
            
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Pen pen = new Pen(Color.Red, 2)) // Red pen for drawing corners
            {
                foreach (Point corner in corners)
                {
                    int markerSize = 1; // Size of the marker
                    int x = corner.X - markerSize / 2;
                    int y = corner.Y - markerSize / 2;

                    graphics.DrawEllipse(pen, x, y, markerSize, markerSize);
                }
            }



            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));
            bitmap.UnlockBits(bitmapData);


            return true;
        }

        public bool ApplyHarrisCornerDetection2()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());
            int width = bitmap.Width;
            int height = bitmap.Height;
            int stride = width * 3;
            int threshold = 1000;
            double k = 0.04; // Corrected the Harris corner constant (typically positive)

            List<Point> corners = new List<Point>();

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* inputPtr = (byte*)bitmapData.Scan0.ToPointer();
                int offset = stride - width * 3;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Calculate Harris corner response
                        double IxIx = 0, IyIy = 0, IxIy = 0;

                        for (int j = -1; j <= 1; j++)
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                byte* pixelPtr = inputPtr + (y + j) * stride + (x + i) * 3;
                                double grayValue = 0.299 * pixelPtr[2] + 0.587 * pixelPtr[1] + 0.114 * pixelPtr[0];

                                IxIx += grayValue * grayValue;
                                IyIy += grayValue * grayValue;
                                IxIy += grayValue * grayValue;
                            }
                        }

                        // Harris corner response formula
                        double detM = (IxIx * IyIy) - (IxIy * IxIy);
                        double traceM = IxIx + IyIy;
                        double cornerResponse = detM - k * (traceM * traceM);

                        // Mark the pixel as a corner in the input image
                        if (cornerResponse > threshold)
                        {
                            corners.Add(new Point(x, y));
                        }

                        inputPtr += 3;
                    }

                    inputPtr += offset;
                }
            }

            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Pen pen = new Pen(Color.Red, 2)) // Red pen for drawing corners
            {
                foreach (Point corner in corners)
                {
                    int markerSize = 1; // Size of the marker
                    int x = corner.X - markerSize / 2;
                    int y = corner.Y - markerSize / 2;

                    graphics.DrawEllipse(pen, x, y, markerSize, markerSize);
                }
            }

            bitmap.UnlockBits(bitmapData);
            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));

            return true;
        }

        public unsafe bool ProcessImage()
        {
            // Make sure we have a grayscale image
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());

            if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                // Create a temporary grayscale image
                this.GrayScale();
                bitmap = ConvertByteArrayToBitmap(Images.Peek());
            }

            // Get source image size
            int width = bitmap.Width;
            int height = bitmap.Height;
            int srcStride = width * 3;
            int srcOffset = srcStride - width;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            // 1. Calculate partial differences
            float[,] diffx = new float[height, width];
            float[,] diffy = new float[height, width];
            float[,] diffxy = new float[height, width];

            fixed (float* pdx = diffx, pdy = diffy, pdxy = diffxy)
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer() + srcStride + 1;

                float* dx = pdx + width + 1;
                float* dy = pdy + width + 1;
                float* dxy = pdxy + width + 1;

                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++, ptr++, dx++, dy++, dxy++)
                    {
                        // Convolution with horizontal differentiation kernel mask
                        float h = ((ptr[-srcStride + 1] + ptr[+1] + ptr[srcStride + 1]) -
                                    (ptr[-srcStride - 1] + ptr[-1] + ptr[srcStride - 1])) * 0.166666667f;

                        // Convolution vertical differentiation kernel mask
                        float v = ((ptr[+srcStride - 1] + ptr[+srcStride] + ptr[+srcStride + 1]) -
                                    (ptr[-srcStride - 1] + ptr[-srcStride] + ptr[-srcStride + 1])) * 0.166666667f;

                        // Store squared differences directly
                        *dx = h * h;
                        *dy = v * v;
                        *dxy = h * v;
                    }

                    // Skip last column
                    dx++;
                    dy++;
                    dxy++;
                    ptr += srcOffset + 1;
                }
            }


            // 3. Compute Harris Corner Response Map
            float[,] map = new float[height, width];

            double k = 0.06;
            int threshold = 1000;

            fixed (float* pdx = diffx, pdy = diffy, pdxy = diffxy, pmap = map)
            {
                float* dx = pdx;
                float* dy = pdy;
                float* dxy = pdxy;
                float* H = pmap;
                float M, A, B, C;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, dx++, dy++, dxy++, H++)
                    {
                        A = *dx;
                        B = *dy;
                        C = *dxy;

                        M = -1*(float)((A * B - C * C) - (k * ((A + B) * (A + B))));

                        if (M > threshold)
                        {
                            *H = M; // insert value in the map
                        }
                    }
                }
            }

            // 4. Suppress non-maximum points
            List<Point> cornersList = new List<Point>();
            int r = 3;
            // for each row
            for (int y = r, maxY = height - r; y < maxY; y++)
            {
                // for each pixel
                for (int x = r, maxX = width - r; x < maxX; x++)
                {
                    float currentValue = map[y, x];

                    // for each windows' row
                    for (int i = -r; (currentValue != 0) && (i <= r); i++)
                    {
                        // for each windows' pixel
                        for (int j = -r; j <= r; j++)
                        {
                            if (map[y + i, x + j] > currentValue)
                            {
                                currentValue = 0;
                                break;
                            }
                        }
                    }

                    // check if this point is really interesting
                    if (currentValue != 0)
                    {
                        cornersList.Add(new Point(x, y));
                    }
                }
            }
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Pen pen = new Pen(Color.Red, 2)) // Red pen for drawing corners
            {
                foreach (Point corner in cornersList)
                {
                    int markerSize = 1; // Size of the marker
                    int x = corner.X - markerSize / 2;
                    int y = corner.Y - markerSize / 2;

                    graphics.DrawEllipse(pen, x, y, markerSize, markerSize);
                }
            }

            bitmap.UnlockBits(bitmapData);
            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));
            return true;
        }

        public unsafe bool ProcessImage2()
        {
            // Make sure we have a grayscale image
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());

            if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                // Create a temporary grayscale image
                this.GrayScale();
                bitmap = ConvertByteArrayToBitmap(Images.Peek());
            }

            // Get source image size
            int width = bitmap.Width;
            int height = bitmap.Height;
            int srcStride = width * 3;
            int srcOffset = srcStride - width;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            // 1. Calculate partial differences
            float[,] diffx = new float[height, width];
            float[,] diffy = new float[height, width];
            float[,] diffxy = new float[height, width];

            fixed (float* pdx = diffx, pdy = diffy, pdxy = diffxy)
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer() + srcStride + 1;

                float* dx = pdx + width + 1;
                float* dy = pdy + width + 1;
                float* dxy = pdxy + width + 1;

                for (int y = 1; y < height - 1; y++)
                {
                    for (int x = 1; x < width - 1; x++, ptr++, dx++, dy++, dxy++)
                    {
                        // Convolution with horizontal differentiation kernel mask
                        float h = ((ptr[-srcStride + 1] + ptr[+1] + ptr[srcStride + 1]) -
                                    (ptr[-srcStride - 1] + ptr[-1] + ptr[srcStride - 1])) * 0.166666667f;

                        // Convolution vertical differentiation kernel mask
                        float v = ((ptr[+srcStride - 1] + ptr[+srcStride] + ptr[+srcStride + 1]) -
                                    (ptr[-srcStride - 1] + ptr[-srcStride] + ptr[-srcStride + 1])) * 0.166666667f;

                        // Store squared differences directly
                        *dx = h * h;
                        *dy = v * v;
                        *dxy = h * v;
                    }

                    // Skip the last column
                    dx++;
                    dy++;
                    dxy++;
                    ptr += srcOffset + 1;
                }
            }

            // 3. Compute Harris Corner Response Map
            float[,] map = new float[height, width];

            float k = 0.06f; // Declare 'k' as a float
            float threshold = 1000.0f; // Declare 'threshold' as a float

            fixed (float* pdx = diffx, pdy = diffy, pdxy = diffxy, pmap = map)
            {
                float* dx = pdx;
                float* dy = pdy;
                float* dxy = pdxy;
                float* H = pmap;
                float M, A, B, C;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, dx++, dy++, dxy++, H++)
                    {
                        A = *dx;
                        B = *dy;
                        C = *dxy;

                        // Original Harris corner measure
                        M = -1 * ((A * B - C * C) - (k * ((A + B) * (A + B))));

                        if (M > threshold)
                        {
                            *H = M; // insert value in the map
                        }
                    }
                }
            }

            // 4. Suppress non-maximum points
            List<Point> cornersList = new List<Point>();
            int r = 3;

            // for each row
            for (int y = r, maxY = height - r; y < maxY; y++)
            {
                // for each pixel
                for (int x = r, maxX = width - r; x < maxX; x++)
                {
                    float currentValue = map[y, x];

                    // for each windows' row
                    for (int i = -r; (currentValue != 0) && (i <= r); i++)
                    {
                        // for each windows' pixel
                        for (int j = -r; j <= r; j++)
                        {
                            if (map[y + i, x + j] > currentValue)
                            {
                                currentValue = 0;
                                break;
                            }
                        }
                    }

                    // check if this point is really interesting
                    if (currentValue != 0)
                    {
                        cornersList.Add(new Point(x, y));
                    }
                }
            }

            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Pen pen = new Pen(Color.Red, 2)) // Red pen for drawing corners
            {
                foreach (Point corner in cornersList)
                {
                    int markerSize = 1; // Size of the marker
                    int x = corner.X - markerSize / 2;
                    int y = corner.Y - markerSize / 2;

                    graphics.DrawEllipse(pen, x, y, markerSize, markerSize);
                }
            }

            bitmap.UnlockBits(bitmapData);
            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));
            return true;
        }
        public bool ApplyHarrisCornerDetection5()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());

            List<Point> corners = DetectCorners(bitmap, 100000000, 0.06, 3);

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Pen pen = new Pen(Color.Red, 2)) // Red pen for drawing corners
            {
                foreach (Point corner in corners)
                {
                    int markerSize = 1; // Size of the marker
                    int x = corner.X - markerSize / 2;
                    int y = corner.Y - markerSize / 2;

                    graphics.DrawEllipse(pen, x, y, markerSize, markerSize);
                }
            }

            bitmap.UnlockBits(bitmapData);
            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));
            return true;
        }

        public List<Point> DetectCorners(Bitmap image, double threshold, double k, int windowSize)
        {
            // Convert the input image to grayscale
            Bitmap grayImage = ConvertToGrayscale(image);

            int width = grayImage.Width;
            int height = grayImage.Height;

            List<Point> corners = new List<Point>();

            // Calculate gradients using simple central differences
            double[,] Ix = CalculateGradientX(grayImage);
            double[,] Iy = CalculateGradientY(grayImage);

            double[,] A = new double[width, height];
            double[,] B = new double[width, height];
            double[,] C = new double[width, height];

            // Calculate the elements of the structure tensor
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double iX = Ix[x, y];
                    double iY = Iy[x, y];

                    A[x, y] = iX * iX;
                    B[x, y] = iY * iY;
                    C[x, y] = iX * iY;
                }
            }

            int halfSize = windowSize / 2;

            // Compute the Harris Corner Response for each pixel
            for (int x = halfSize; x < width - halfSize; x++)
            {
                for (int y = halfSize; y < height - halfSize; y++)
                {
                    double sumA = 0, sumB = 0, sumC = 0;

                    // Sum elements in the window
                    for (int wx = -halfSize; wx <= halfSize; wx++)
                    {
                        for (int wy = -halfSize; wy <= halfSize; wy++)
                        {
                            int nx = x + wx;
                            int ny = y + wy;

                            sumA += A[nx, ny];
                            sumB += B[nx, ny];
                            sumC += C[nx, ny];
                        }
                    }

                    // Calculate the Harris Corner Response
                    double detM = sumA * sumB - sumC * sumC;
                    double traceM = sumA + sumB;
                    double cornerResponse = detM - k * (traceM * traceM);

                    if (cornerResponse > threshold)
                    {
                        corners.Add(new Point(x, y));
                    }
                }
            }

            return corners;
        }

        private Bitmap ConvertToGrayscale(Bitmap image)
        {
            Bitmap grayImage = new Bitmap(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int grayValue = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                    grayImage.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
                }
            }

            return grayImage;
        }

        private double[,] CalculateGradientX(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            double[,] gradientX = new double[width, height];

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    double iX = image.GetPixel(x + 1, y).R - image.GetPixel(x - 1, y).R;
                    gradientX[x, y] = iX;
                }
            }

            return gradientX;
        }

        private double[,] CalculateGradientY(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            double[,] gradientY = new double[width, height];

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    double iY = image.GetPixel(x, y + 1).R - image.GetPixel(x, y - 1).R;
                    gradientY[x, y] = iY;
                }
            }

            return gradientY;
        }




        public void AddNew(IFormFile img)
        {
            Images.Push(ConvertBitmapToByteArray(ConvertIFormFileToBitmap(img), ImageFormat.Jpeg));
        }
        public Bitmap GetActual()
        {
            return ConvertByteArrayToBitmap(Images.Peek());
        }
        public string GetActualSrc()
        {
            return Convert.ToBase64String(ToByteArray(GetActual(),ImageFormat.Jpeg));
        }
        public bool Undo()
        {
            Images.Pop();
            return true;
        }
        public Bitmap ConvertIFormFileToBitmap(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                return new Bitmap(stream);
            }
        }
        public static byte[] ToByteArray(Bitmap bitmap, ImageFormat format)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, format);
                return stream.ToArray();
            }
        }
        public byte[] GetImage()
        {
            byte[] imageBytes = ConvertBitmapToByteArray(GetActual(), ImageFormat.Jpeg);
            return imageBytes;
        }
        private byte[] ConvertBitmapToByteArray(Bitmap bitmap, ImageFormat format)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, format);
                return stream.ToArray();
            }
        }
        private Bitmap ConvertByteArrayToBitmap(byte[] bytearray)
        {
            Bitmap bmp;
            using (var ms = new MemoryStream(bytearray))
            {
                bmp = new Bitmap(ms);
            }
            return bmp;
        }
        
        public void AddImage(IFormFile img)
        {
            using (var stream = img.OpenReadStream())
            {
                byte[] buffer = new byte[img.Length];
                stream.Read(buffer, 0, (int)img.Length);

                // Set Image and ContentType properties using reflection
                //Color.GetType().GetProperty("Image" + (i + 1)).SetValue(Color, buffer);
                //Color.GetType().GetProperty("ContentType" + (i + 1)).SetValue(Color, Color.PictureData[i].ContentType);
            }
        }
    }
}



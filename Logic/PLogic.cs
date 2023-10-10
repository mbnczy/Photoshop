using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Pipelines;
using System.Net.Mime;
using System.Reflection;
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
                            p[0] = redGamma[p[0]];
                        else if (x % 3 == 1) // Green component
                            p[1] = greenGamma[p[1]];
                        else if (x % 3 == 2) // Blue component
                            p[2] = blueGamma[p[2]];

                        ++p;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);


            Images.Push(ConvertBitmapToByteArray(b, ImageFormat.Jpeg));

            return true;
        }

        public int[] CreateHistogramWithPointer()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());

            //if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed)
            //    throw new ArgumentException("Bitmap must be in 8-bit grayscale format.");

            int[] histogram = new int[256];

            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            unsafe
            {
                byte* scan0 = (byte*)bmpData.Scan0;

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        byte pixelValue = scan0[y * bmpData.Stride + x];
                        histogram[pixelValue]++;
                    }
                }
            }

            bitmap.UnlockBits(bmpData);

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
        public int[] CreateHistogram254()
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

        private int[] CalculateCDF(int[] histogram)
        {
            int[] cdf = new int[256];
            cdf[0] = histogram[0];

            for (int i = 1; i < 256; i++)
            {
                cdf[i] = cdf[i - 1] + histogram[i];
            }

            return cdf;
        }
        private int[] CalculateCDFWithEq(int[] histogram)
        {
            int[] cdf = new int[256];
            cdf[0] = histogram[0];

            for (int i = 1; i < 256; i++)
            {
                cdf[i] = cdf[i - 1] + histogram[i];
            }

            // Calculate equalization factor
            double equalizationFactor = 255.0 / (cdf[255] + 1);

            // Perform equalization with equalization factor
            for (int i = 0; i < 256; i++)
            {
                cdf[i] = (int)(cdf[i] * equalizationFactor);
            }

            return cdf;
        }

        public bool HistogramEqualization()
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());

            // Lock the bits of the outputBitmap for direct access
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, bitmap.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int stride = bitmapData.Stride;
            int[] cdf = CalculateCDFWithEq(CreateHistogram254());
            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color pixel = bitmap.GetPixel(x, y);
                        int intensity = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);

                        // Calculate the new intensity using the CDF
                        int newIntensity = cdf[intensity];

                        // Update the pixel with the new intensity
                        ptr[(y * stride) + (x * bytesPerPixel) + 0] = (byte)newIntensity; // Red
                        ptr[(y * stride) + (x * bytesPerPixel) + 1] = (byte)newIntensity; // Green
                        ptr[(y * stride) + (x * bytesPerPixel) + 2] = (byte)newIntensity; // Blue
                    }
                }
            }

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

        private int[,] CreateGaussianKernel(int size, int sigma)
        {
            int[,] kernel = new int[size, size];
            int sum = 0;
            int halfSize = size / 2;

            for (int y = -halfSize; y <= halfSize; y++)
            {
                for (int x = -halfSize; x <= halfSize; x++)
                {
                    int exponent = -(x * x + y * y) / (2 * sigma * sigma);
                    int value = (int)(Math.Exp(exponent) / (2 * Math.PI * sigma * sigma));
                    kernel[y + halfSize, x + halfSize] = value;
                    sum += value;
                }
            }

            // Normalize
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[y, x] /= sum;
                }
            }

            return kernel;
        }

        private double Gauss(int x, int y, double sigma)
        {
            return Math.Exp(-(x * x + y * y) / (2 * sigma * sigma)) / (2 * Math.PI * sigma * sigma);
        }

        public bool ApplyGaussianFilter(int kernelSize, int sigma)
        {
            Bitmap bitmap = ConvertByteArrayToBitmap(Images.Peek());

            // Create a copy of the input bitmap to avoid modifying the original
            Bitmap outputBitmap = new Bitmap(bitmap);

            // Create the Gaussian kernel
            //int[,] kernel = CreateGaussianKernel(kernelSize, sigma);

            int halfSize = kernelSize / 2;
            int width = bitmap.Width;
            int height = bitmap.Height;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            
            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0.ToPointer();

                int stride = bitmapData.Stride;

                for (int y = halfSize; y < height - halfSize; y++)
                {
                    for (int x = halfSize; x < width - halfSize; x++)
                    {
                        double totalRed = 0, totalGreen = 0, totalBlue = 0;

                        for (int j = -halfSize; j <= halfSize; j++)
                        {
                            for (int i = -halfSize; i <= halfSize; i++)
                            {
                                byte* pixelPtr = ptr + (y + j) * stride + (x + i) * 3;

                                double kernelValue = Gauss(i, j, sigma);
                                totalRed += pixelPtr[2] * kernelValue;
                                totalGreen += pixelPtr[1] * kernelValue;
                                totalBlue += pixelPtr[0] * kernelValue;
                            }
                        }

                        byte* outputPixelPtr = ptr + y * stride + x * 3;
                        outputPixelPtr[2] = (byte)totalRed;
                        outputPixelPtr[1] = (byte)totalGreen;
                        outputPixelPtr[0] = (byte)totalBlue;
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);

            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));
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
                        int redGradient = Math.Abs(gxRed) + Math.Abs(gyRed);
                        int greenGradient = Math.Abs(gxGreen) + Math.Abs(gyGreen);
                        int blueGradient = Math.Abs(gxBlue) + Math.Abs(gyBlue);
                        int gradientMagnitude = redGradient + greenGradient + blueGradient;
                        if (!(gradientMagnitude > 750))
                        {
                            // Él jelenik meg a kimeneti képen
                            redGradient = 0;
                            greenGradient = 0;
                            blueGradient = 0;
                        }
                        else
                        {
                            redGradient = 100;
                            greenGradient = 100;
                            blueGradient = 100;
                        }
                        
                        byte* outputPixelPtr = ptr + y * stride + x * 3;
                        //outputPixelPtr[2] = (byte)Math.Min(255, redGradient);
                        //outputPixelPtr[1] = (byte)Math.Min(255, greenGradient);
                        //outputPixelPtr[0] = (byte)Math.Min(255, blueGradient);
                        //int darknessFactor = 6;
                        //outputPixelPtr[2] = (byte)Math.Max(0, outputPixelPtr[2] - redGradient / darknessFactor);
                        //outputPixelPtr[1] = (byte)Math.Max(0, outputPixelPtr[1] - greenGradient / darknessFactor);
                        //outputPixelPtr[0] = (byte)Math.Max(0, outputPixelPtr[0] - blueGradient / darknessFactor);

                        // Küszöbölés hozzáadása
                        //int threshold = 100;
                        //int thresholdedRed = redGradient > threshold ? 255 : 0;
                        //int thresholdedGreen = greenGradient > threshold ? 255 : 0;
                        //int thresholdedBlue = blueGradient > threshold ? 255 : 0;

                        // Kiemelt élek a kimeneti képen, de sötétedés nélkül
                        outputPixelPtr[2] = (byte)redGradient;
                        outputPixelPtr[1] = (byte)greenGradient;
                        outputPixelPtr[0] = (byte)blueGradient;
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);

            Images.Push(ConvertBitmapToByteArray(bitmap, ImageFormat.Jpeg));
            return true;
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



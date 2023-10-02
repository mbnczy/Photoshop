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
        //public byte[] GetImageUndo()
        //{
        //    //Images.Pop();
        //    //System.Threading.Thread.Sleep(1000);
        //    List<Bitmap> imagelist = Images.ToList();
        //    byte[] imageBytes = ConvertBitmapToByteArray(imagelist[imagelist.Count-1], ImageFormat.Jpeg);
        //    ;
        //    return imageBytes;
        //}
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



using System;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Photoshop.Logic;

namespace Photoshop.Controllers
{
	public class HomeController : Controller
    {
		public PLogic logic;

        public IActionResult Index()
		{
            return View();
		}

        [HttpPost]
        public IActionResult Index(IFormFile img)
        {
            PLogic.Instance.AddNew(img);
            Bitmap newimage = PLogic.Instance.GetActual();
            

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Gamma(string colors)
        {
            string[] splitted = colors.Split(";");
            PLogic.Instance.Gamma(
                Convert.ToDouble(splitted[0]),
                Convert.ToDouble(splitted[1]),
                Convert.ToDouble(splitted[2])
            );

            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
        }

        public IActionResult Invert()
        {
            PLogic.Instance.Invert();

            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
        }
        public IActionResult Grayscale()
        {
            PLogic.Instance.GrayScale();

            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
        }
        public IActionResult Brightness(double brightness)
        {
            PLogic.Instance.Brightness((int)brightness);

            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
        }
        public IActionResult Contrast(double contrast)
        {
            PLogic.Instance.Contrast((int)contrast);

            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
        }
        public IActionResult Logarithm(double log)
        {
            PLogic.Instance.Logarithm((int)log);

            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
        }

        public IActionResult Histogram()
        {
            int[] hist = PLogic.Instance.CreateHistogram2();
            return Ok(hist);
        }

        public IActionResult HistogramEq(double log)
        {
            PLogic.Instance.HistogramEqualization();

            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
        }
        public IActionResult BoxFilter(double box)
        {
            PLogic.Instance.ApplyAverageFilter((int)box);

            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
        }
        public IActionResult GaussFilter(string id, string sigma)
        {
            PLogic.Instance.ApplyGaussianFilter(Convert.ToInt32(id), Convert.ToInt32(sigma));

            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
        }

        public IActionResult Sobel()
        {
            PLogic.Instance.ApplySobelEdgeDetection2();

            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
        }

        public IActionResult Laplace()
        {
            PLogic.Instance.ApplyLoGEdgeDetection();

            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
        }
        public IActionResult Harris()
        {
            PLogic.Instance.ApplyHarrisCornerDetection();

            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
        }

        public IActionResult GetActualImage()
        {
            if (PLogic.Instance.Images.Count!=0)
            {
                byte[] img = PLogic.Instance.GetImage();
                return new FileContentResult(img, "image/jpeg");
            }

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Undo()
        {
            if (PLogic.Instance.Undo())
            {
                byte[] img = PLogic.Instance.GetImage();
                
                return new FileContentResult(img, "image/jpeg");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}


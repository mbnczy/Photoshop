using System;
using System.Drawing;
using System.Net;
using System.Text.Json;
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
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.Gamma(
                Convert.ToDouble(splitted[0]),
                Convert.ToDouble(splitted[1]),
                Convert.ToDouble(splitted[2])
            ));
            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Opt_Gamma(string colors)
        {
            string[] splitted = colors.Split(";");
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.OptGamma(
                Convert.ToDouble(splitted[0]),
                Convert.ToDouble(splitted[1]),
                Convert.ToDouble(splitted[2])
            ));
            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }

        public IActionResult Invert()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.Invert());

            byte[] img = PLogic.Instance.GetImage();

            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Opt_Invert()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.OptInvert());

            byte[] img = PLogic.Instance.GetImage();

            return FormatJSON(img, elapsedTime);
        }


        public IActionResult Grayscale()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.GrayScale());

            byte[] img = PLogic.Instance.GetImage();

            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Opt_Grayscale()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.OptGrayScale());

            byte[] img = PLogic.Instance.GetImage();

            return FormatJSON(img, elapsedTime);
        }


        public IActionResult Brightness(double brightness)
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.Brightness((int)brightness));

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Opt_Brightness(double brightness)
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.OptBrightness((int)brightness));

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }


        public IActionResult Contrast(double contrast)
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.Contrast((int)contrast));

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Opt_Contrast(double contrast)
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.OptContrast((int)contrast));

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }


        public IActionResult Logarithm(double log)
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.Logarithm((int)log));

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Opt_Logarithm(double log)
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.OptLogarithm((int)log));

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }


        public IActionResult Histogram()
        {
            int[] hist = PLogic.Instance.CreateHistogram();
            return Ok(hist);
        }

        public IActionResult HistogramEq()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.HistogramEqualization());

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Opt_HistogramEq()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.OptHistogramEqualization());

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }

        public IActionResult BoxFilter(double box)
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.ApplyAverageFilter((int)box));

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Opt_BoxFilter(double box)
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.OptApplyAverageFilter((int)box));

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }


        public IActionResult GaussFilter(string id, string sigma)
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.ApplyGaussianFilter2(Convert.ToInt32(id), Convert.ToInt32(sigma)));

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Opt_GaussFilter(string id, string sigma)
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.ApplyGaussianFilter(Convert.ToInt32(id), Convert.ToInt32(sigma)));

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }

        public IActionResult Sobel()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.ApplySobelEdgeDetection());

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Opt_Sobel()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.OptApplySobelEdgeDetection());

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }

        public IActionResult Laplace()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.ApplyLaplaceEdgeDetection());

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Opt_Laplace()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.OptApplyLaplaceEdgeDetection());

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Harris()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.ApplyHarrisCornerDetection());

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }
        public IActionResult Opt_Harris()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.OptApplyHarrisCornerDetection());

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
        }
        public IActionResult LsdFilter()
        {
            TimeSpan elapsedTime = PLogic.MeasureExecutionTime(() => PLogic.Instance.LSD_Filter());

            byte[] img = PLogic.Instance.GetImage();
            return FormatJSON(img, elapsedTime);
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

        private ContentResult FormatJSON(byte[] img, TimeSpan elapsedTime)
        {
            var data = new
            {
                FileContent = new FileContentResult(img, "image/jpeg"),
                ElapsedTime = $"{elapsedTime.Seconds}.{elapsedTime.Milliseconds}"
            };
            string jsonData = JsonSerializer.Serialize(data);
            return new ContentResult
            {
                Content = jsonData,
                ContentType = "application/json",
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}


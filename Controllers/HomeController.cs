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

        public IActionResult Gamma(double red, double green, double blue)
        {
            PLogic.Instance.Gamma(red, green, blue);

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


        public IActionResult GetActualImage()
        {
            byte[] img = PLogic.Instance.GetImage();
            return new FileContentResult(img, "image/jpeg");
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


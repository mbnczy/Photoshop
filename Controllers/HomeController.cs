using System;
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
		public IActionResult Gamma(IFormFile img)
		{

			
			;
			
			return RedirectToAction(nameof(Index));
		}
	}
}


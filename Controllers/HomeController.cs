/*
using In_Out.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using InOut.Controllers;

namespace In_Out.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Fetch allowed tags from ReaderDataController
        public IActionResult AllowedTags()
        {
            var allowedTags = ReaderDataController.AllowedTags.ToList();  // Get the allowed tags
            ViewBag.AllowedTags = allowedTags;  // Pass to the view
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
*/

using In_Out.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using InOut.Controllers;

namespace In_Out.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }


        // Contact Us 
        public IActionResult ContactUs()
        {
            return View();
        }

        /*   public IActionResult Privacy()
           {
               return View();
           }*/


        // Fetch allowed tags from ReaderDataController
        public IActionResult AllowedTags()
        {
            // Get the allowed tags from the static property in ReaderDataController
            var allowedTags = ReaderDataController.AllowedTags.ToList();
            ViewBag.AllowedTags = allowedTags;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Add a new allowed tag
        [HttpPost]
        public IActionResult AddAllowedTag(string epc)
        {
            if (!string.IsNullOrEmpty(epc) && !ReaderDataController.AllowedTags.Contains(epc))
            {
                ReaderDataController.AllowedTags.Add(epc); // Add to allowed tags
            }

            return RedirectToAction("AllowedTags");
        }

        // Remove an allowed tag
        [HttpPost]
        public IActionResult RemoveAllowedTag(string epc)
        {
            if (ReaderDataController.AllowedTags.Contains(epc))
            {
                ReaderDataController.AllowedTags.Remove(epc); // Remove from allowed tags
            }

            return RedirectToAction("AllowedTags");
        }
    }
}


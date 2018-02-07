using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace aspnet4_sample1.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index(String error, String error_description)
        {
            ViewBag.Error = error;
            ViewBag.ErrorDescription = error_description;
            return View();
        }
    }
}
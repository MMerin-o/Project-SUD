using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StaffingAssign.Models;
using StaffingAssign.Controllers;
using StaffingAssign.Models.View_Models;

namespace StaffingAssign.Controllers
{
    public class HomeController : Controller  
    {

        public ActionResult Index()
        {

            return View();
        }
       
    }
}
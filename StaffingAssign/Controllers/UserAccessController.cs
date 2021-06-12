using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StaffingAssign.Controllers
{
    using GetMembersFromGroup;
    public class UserAccessController : Controller
    {
        // GET: UserAccess
        public ActionResult UserLogin()
        {
            return View();
        }


        [HttpPost]
        
        public ActionResult UserLogin(string user, string password)
        {
            string _dominio = "corp.intusurg.com";
            ADHelper ad_user = new ADHelper();
            try
            {
                if (true == ad_user.AuthenticateUser(_dominio, user, password))
                {
                    return RedirectToAction("Index","Home");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }
    }
}
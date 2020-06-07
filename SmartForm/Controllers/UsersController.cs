using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SmartForm.Domain;
using SmartForm.Repository.Repositories;
using SmartForm.Service.EntityServices;

namespace SmartForm.Controllers
{
    public class UsersController : Controller
    {
        private UserService userService = new UserService(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities")), new EFRepository<User>(new EFUnitOfWork(new DbContextFactory("SmartFormEntities", "SmartFormEntities"))));

        public ActionResult Index()
        {
            return View(userService.Load().ToList());
        }


        public ActionResult UserProfile()
        {
            String username = Convert.ToString(Session["UserName"]);
            User user = userService.Load(x => x.UserName == username && x.DateDeleted == null).FirstOrDefault();
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        public ActionResult Register()
        {
            if (Session["UserName"] != null)
            {
                return RedirectToAction("Index", "Forms");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "UserId,UserName,PasswordHash,DisplayName,Email,DateCreated,DateModfied,DateDeleted")] User user)
        {

            if (ModelState.IsValid)
            {
                User existinguser = userService.Load(u => u.UserName == user.UserName).SingleOrDefault();
                if (existinguser == null)
                {
                    user.PasswordHash = EncryptandDecrypt.Crypt(user.PasswordHash);
                    user.DateCreated = DateTime.Now;
                    userService.Add(user);
                    userService.Save();
                    Session["UserName"] = user.UserName;
                    Session["DisplayName"] = user.DisplayName;
                    Session["UserId"] = user.UserId;
                    return RedirectToAction("Index", "Forms");
                }
                TempData["Error"] = "User with given Username already existed.";
                return RedirectToAction("Register", "Users");
            }

            return View(user);
        }

        [HttpPost]
        public JsonResult EditDisplayName(String userid, String displayname)
        {
            JsonResult jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            int id = Convert.ToInt32(userid);
            User user = userService.LoadByID(id);
            user.DisplayName = displayname;
            Session["DisplayName"] = displayname;
            userService.Save();
            jsonResult.Data = new { Success = true, Message = "Update DisplayName" };
            return jsonResult;
        }

        [HttpPost]
        public ActionResult UpdateProfile(User user) {
            User dbUser = userService.LoadByID(user.UserId);
            dbUser.DisplayName = user.DisplayName;
            dbUser.Email = user.Email;
            dbUser.DateModfied = DateTime.Now;
            userService.Save();
            Session["DisplayName"] = user.DisplayName;
            return RedirectToAction("UserProfile");
        }

        public ActionResult Login()
        {
            if (Session["UserName"] != null)
            {
                return RedirectToAction("Index", "Forms");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "UserId,UserName,PasswordHash,DisplayName,Email,DateCreated,DateModfied,DateDeleted")] User user)
        {

            String passwordhash = EncryptandDecrypt.Crypt(user.PasswordHash);
            User cuser = userService.Load(x => x.UserName == user.UserName && x.PasswordHash == passwordhash && !x.DateDeleted.HasValue).SingleOrDefault();
            if (cuser != null)
            {
                Session["DisplayName"] = cuser.DisplayName;
                Session["UserName"] = cuser.UserName;
                Session["UserId"] = cuser.UserId;
                ViewBag.Error = "User Logged IN";
                return RedirectToAction("Index", "Forms");
            }
            TempData["Error"] = "UserName or Password do not match";
            ViewBag.Error = "UserName or Password do not match";
            return RedirectToAction("Login", "Users");
        }

        public ActionResult Setting() {
            if (Session["UserId"] != null) {
                return View();
            }
            return new HttpStatusCodeResult(403);
        }

        [HttpPost]
        public ActionResult Setting(String oldPassword,String newPassword) {
            int UserId = Convert.ToInt32(Session["UserId"]);
            User dbUser = userService.LoadByID(UserId);
            String dbPasswordHash = dbUser.PasswordHash;
            String dbPasswordUnhash = EncryptandDecrypt.Decrypt(dbPasswordHash);
            bool PasswordMatch = String.Equals(dbPasswordUnhash, oldPassword);
            if (!PasswordMatch)
            {
                TempData["Error"] = "Old Password did not match our Records";
                return RedirectToAction("Setting", "Users");
            }
            else {
                dbUser.PasswordHash = EncryptandDecrypt.Crypt(newPassword);
                dbUser.DateModfied = DateTime.Now;
                userService.Save();
                TempData["Success"] = "Password Updated Successfully";
                return RedirectToAction("Setting", "Users");
            }
        }

       

        public ActionResult LoggedIn()
        {
            if (Session["UserName"] != null)
            {
                return View();
            }
            return RedirectToAction("Register");
        }
        [HttpPost]
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }


        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}

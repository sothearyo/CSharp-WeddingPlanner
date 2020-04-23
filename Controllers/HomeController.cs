using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        // ------------------- Login and Registration -----------------------------
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email","Email already in use.");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                dbContext.Add(newUser);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("UserId",newUser.UserId);

                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.UserEmail);
                if (userInDb == null)
                {
                    ModelState.AddModelError("UserEmail", "Invalid Email/Password.");
                    return View("Index");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.UserPassword);
                if(result == 0)
                {
                    ModelState.AddModelError("UserEmail", "Incorrect Email/Password.");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("UserId",userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // -------------------- Wedding Planner --------------------------------------
        // -------- Render Page --------
        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            if (user_id == 0)
            {
                return View("Index");
            }
            User ThisUser = dbContext.Users
                .Include(u => u.AllWeddings)
                .ThenInclude(a => a.Wedding)
                .FirstOrDefault(u => u.UserId == user_id);
            List<Wedding> EveryWedding = dbContext.Weddings
                .Include(w => w.AllUsers)
                .ThenInclude(a => a.User)
                .ToList();  

            // Remove weddings if it passed already
            foreach (var w in EveryWedding)
            {
                int wed_date = w.Date.Year*1000 + w.Date.DayOfYear;
                int today_date = DateTime.Now.Year*1000 + DateTime.Now.DayOfYear;
                if(wed_date < today_date)
                {
                    dbContext.Remove(w);
                    dbContext.SaveChanges();
                    System.Console.WriteLine($"********* Removed past wedding on {wed_date}, since today is {today_date} **********");
                    return RedirectToAction("Dashboard");
                }
            }
            UserWedWrapper newWrap = new UserWedWrapper();
            newWrap.ThisUser = ThisUser;
            newWrap.EveryWedding = EveryWedding;
            return View(newWrap);
        }

        [HttpGet("planWedding")]
        public IActionResult PlanWedding()
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            if (user_id == 0)
            {
                return View("Index");
            }
            return View();
        }

        [HttpGet("wedding/{wed_id}")]
        public IActionResult OneWedding(int wed_id)
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            if (user_id == 0)
            {
                return View("Index");
            }
            Wedding thisWed = dbContext.Weddings
                .Include(w => w.AllUsers)
                .ThenInclude(a => a.User)
                .FirstOrDefault(w => w.WeddingId == wed_id);
            return View(thisWed);
        }

        // --------- Action Methods --------
        [HttpPost("addNewWedding")]
        public IActionResult AddNewWedding(Wedding NewWedding)
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            NewWedding.UserId = user_id;
            if(ModelState.IsValid)
            {
                dbContext.Add(NewWedding);
                dbContext.SaveChanges();
                return RedirectToAction("OneWedding", new { wed_id = NewWedding.WeddingId });
            }
            return View("PlanWedding");
        }

        [HttpGet("rsvp/{wed_id}")]
        public IActionResult RSVP(int wed_id)
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            Association newAssoc = new Association();
            newAssoc.UserId = user_id;
            newAssoc.WeddingId = wed_id;
            dbContext.Add(newAssoc);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("unrsvp/{wed_id}")]
        public IActionResult UnRSVP(int wed_id)
        {
            int user_id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            Association thisAssoc = dbContext.Associations
                .FirstOrDefault(a => a.UserId == user_id && a.WeddingId == wed_id);
            dbContext.Remove(thisAssoc);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("delete/{wed_id}")]
        public IActionResult DeleteWedding(int wed_id)
        {
            Wedding thisWed = dbContext.Weddings
                .FirstOrDefault(w => w.WeddingId == wed_id);
            dbContext.Remove(thisWed);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }


        // ------------------ ErrorView Model ---------------------------------------
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using Music.ViewModels;
using MusicStoreEntity;
using MusicStoreEntity.UserAndRole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Music.Controllers
{
    public class MyController : Controller
    {
        private static readonly EntityDbContext _context = new EntityDbContext();
        public ActionResult Add(PersonAddress personAddress)
        {
            //1.确认用户是否登陆 是否登陆过期
            if (Session["loginUserSessionModel"] == null)
            {
                return RedirectToAction("login", "Account", new { returnUrl = Url.Action("Buy", "Order") });

            }
            //2.查询出当前用户Person 查询该用户的购物项
            var persons = (Session["loginUserSessionModel"] as LoginUserSessionModel).Person;
            var person = _context.Persons.Find(persons.ID);
            person.PersonAddresss = new List<PersonAddress>();
            if (personAddress.AddresPerson != null)
            {
                person.PersonAddresss.Add(personAddress);
                _context.SaveChanges();
            }
            
            return View();
        }
        public ActionResult Remove()
        {

            return View();
        }
        // GET: My
        public ActionResult Index()
        {
            //1.确认用户是否登陆 是否登陆过期
            if (Session["loginUserSessionModel"] == null)
            {
                return RedirectToAction("login", "Account", new { returnUrl = Url.Action("Buy", "Order") });

            }
            var persons = (Session["loginUserSessionModel"] as LoginUserSessionModel).Person;
            var address = _context.Persons.Find(persons.ID).PersonAddresss.ToList();
            return View(address);
        }
    }
}
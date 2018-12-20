﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MusicStoreEntity;
using Music.ViewModels;

namespace Music.Controllers
{
    public class StoreController : Controller
    {
        private static readonly EntityDbContext _context = new EntityDbContext();
        /// <summary>
        /// 显示专辑的明细
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: Store
        public ActionResult Detail(Guid id)
        {
            var detail = _context.Albums.Find(id);
            var reply = _context.Replys.Where(x => x.Album.ID == detail.ID).ToList();
            var cartVM = new DetailReply()
            {
                ID = detail.ID,
                Title = detail.Title,
                Price =detail.Price,
                PublisherDate = detail.PublisherDate,
                AlbumArtUrl = detail.AlbumArtUrl,
                Genre=detail.Genre,
                Artist=detail.Artist,
                MusicUrl = detail.MusicUrl,
                Replys = reply

            };
            return View(cartVM);
        }
        /// <summary>
        /// 按分类显示专辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Browser(Guid id)
        {
            var list = _context.Albums.Where(x => x.Genre.ID == id).OrderByDescending(x => x.PublisherDate).ToList();
            return View(list);
        }
        /// <summary>
        /// 显示所有的分类
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var genres = _context.Genres.OrderBy(x=>x.Name).ToList();

            return View(genres);
        }
        [HttpPost]
        public ActionResult Reply(Guid id, string content)
         {
            //判断用户是否登陆
            if (Session["loginUserSessionModel"] == null)
            {
                return RedirectToAction("login", "Account", new { returnUrl = Url.Action("Index", "ShoppingCart") });
            }
            //查询出当前登陆用户
            var person = (Session["loginUserSessionModel"] as LoginUserSessionModel).Person;

            var replys= new Reply()
            {
                Title=person.Name,
                Content=content,
                Person=person,
                Album=_context.Albums.Find(id),
                ParentReply=null

            };
            _context.Replys.Add(replys);
            _context.SaveChanges();
            var reps = _context.Replys.Where(x=>x.Album.ID==id);
            var htmlString = "";
            foreach (var item in reps)
            {
                htmlString += "<div>";
                htmlString += "<p>"+item.Title+"</p>";
                htmlString += "<p>" + item.CreateDateTime + "</p>";
                htmlString += "<p>" + item.Content + "</p>";
                htmlString += "</div>";
            }
            return Json(htmlString);
        }
    }
}
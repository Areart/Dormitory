using System;
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
            var reply = _context.Replys.Where(x => x.Album.ID == detail.ID).Where(x => x.ID == x.ParentReply.ID).OrderByDescending(x=>x.CreateDateTime).ToList();
            var prenreply=_context.Replys.Where(x => x.Album.ID == detail.ID).Where(x => x.ID != x.ParentReply.ID).OrderByDescending(x => x.CreateDateTime).ToList();
            var htmlString = "";
            var louc = reply.Count+1;//用于显示楼层
            foreach (var item in reply)
            {
                louc--;
                htmlString += "<div id='pinl_main'>";//最外层
                htmlString += "<div id='pinl_main_main'>";//第一层
                htmlString += "<div>";//第一层中的图片
                htmlString += "<img onload='imgWidth(this)' src=" + item.Person.Avarda + ">";
                htmlString += "</div>";//图片结束
                htmlString += "<div>";//评论内容
                htmlString += "<p>" + item.Title + "</p>";
                htmlString += "<p>" + item.Content + "</p>";
                htmlString += "</div>";//内容结束
                htmlString += "<div>";//第一层中的功能栏
                htmlString += "<ul id='"+item.ID+"'>";
                htmlString += "<li>#"+louc+"</li>";
                htmlString += "<li>"+item.CreateDateTime.ToString("yyyy年MM月hh日")+"</li>";
                htmlString += "<li onclick='ADD(this)'>回复</li>";
                htmlString += "<li><i class='glyphicon glyphicon-thumbs-up'></i>（" + item.Like + "）</li>";
                htmlString += "<li><i class='glyphicon glyphicon-thumbs-down'></i>（" + item.Hate + "）</li>";
                htmlString += "<li onclick='pinlMain(this)'>对话内容</li>";
                htmlString += "</ul>";
                htmlString += "</div>";//功能栏结束
                htmlString += "</div>";//第一层结束
                htmlString += "<div id='pinl_main_second'   class='a" + louc + "'>";//第二层回复

                if (prenreply.Where(x => x.ParentReply.ID == item.ID).ToList().Count>0)
                {
                    foreach (var ry in prenreply.Where(x => x.ParentReply.ID == item.ID).OrderBy(x => x.CreateDateTime).ToList())
                    {
                        htmlString += "<div id='pinl_main_main'>";//第一层
                        htmlString += "<div style='margin-bottom:50px'>";//第一层中的图片
                        htmlString += "<img style='width:70%;'  src=" + ry.Person.Avarda + ">";
                        htmlString += "</div>";//图片结束
                        htmlString += "<div>";//评论内容
                        if (_context.Replys.Find(ry.ParentReplyNameID)!= null)
                        {
                            htmlString += "<p>" + ry.Title + " 回复  " + _context.Replys.Find(ry.ParentReplyNameID).Title + " : " + ry.Content + "</p>";
                        }
                        else
                        {
                            htmlString += "<p>" + ry.Title + " 回复  " + item.Title + " : " + ry.Content + "</p>";
                        }
                      
                        htmlString += "</div>";//内容结束
                        htmlString += "<div>";//第一层中的功能栏
                        htmlString += "<ul id='" + ry.ID + "'>";
                        htmlString += "<li></li>";
                        htmlString += "<li>" + ry.CreateDateTime.ToString("yyyy年MM月hh日") + "</li>";
                        htmlString += "<li onclick='ADD(this)'>回复</li>";
                        htmlString += "<li><i class='glyphicon glyphicon-thumbs-up'></i>(" + item.Like + ")</li>";
                        htmlString += "<li><i class='glyphicon glyphicon-thumbs-down'></i>(" + item.Hate + ")</li>";
                        htmlString += "<li>对话内容</li>";
                        htmlString += "</ul>";
                        htmlString += "</div>";//功能栏结束
                        htmlString += "</div>";//第一层结束
                    }

                }
                htmlString += "</div>";//第二层回复结束
                if (prenreply.Where(x => x.ParentReply.ID == item.ID).ToList().Count>1)
                    htmlString += "<p class='pinl_p' id='a" + louc + "' onclick='openmain(this)'>共" + prenreply.Where(x => x.ParentReply.ID == item.ID).ToList().Count + "条回复，点击查看</p>";
                htmlString += "<hr>";
                htmlString += "</div>";
            }
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
                Replys = htmlString
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

        /// <summary>
        /// 回复兼子回复
        /// </summary>
        /// <param name="id">专辑ID</param>
        /// <param name="replyID">被回复的人的ID</param>
        /// <param name="content">回复内容</param>
        /// <param name="contentreply">子回复内容</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]//关闭验证
        public ActionResult Reply(Guid id,Guid replyID, string content,string contentreply)
         {
            //判断用户是否登陆
            if (Session["loginUserSessionModel"] == null)
            {
                return Json("OK");
            }
            //查询出当前登陆用户
            var person = (Session["loginUserSessionModel"] as LoginUserSessionModel).Person;

            var replys= new Reply()
            {
                Title=person.Name,
                Person=_context.Persons.Find(person.ID),
                Album=_context.Albums.Find(id)
            };
            if (contentreply != null)
            {
                //判断回复的是否是评论
                if (_context.Replys.Find(replyID).ID== _context.Replys.Find(replyID).ParentReply.ID)
                {
                    replys.Content = contentreply;
                    replys.ParentReply = _context.Replys.Find(replyID);
                }
                else
                {
                    replys.Content = contentreply;
                    replys.ParentReply = _context.Replys.Find(replyID).ParentReply;
                    replys.ParentReplyNameID = replyID;
                }
            }
            else
            {
                replys.Content = content;
                replys.ParentReply = replys;
            }            
            _context.Replys.Add(replys);
            _context.SaveChanges();
            var reps = _context.Replys.Where(x=>x.Album.ID==id).Where(x=>x.ID==x.ParentReply.ID).OrderByDescending(x => x.CreateDateTime).ToList();
            var prenreply = _context.Replys.Where(x => x.Album.ID == id).Where(x => x.ID != x.ParentReply.ID).OrderByDescending(x => x.CreateDateTime).ToList();
            var htmlString = "";
            var louc = reps.Count + 1;
            foreach (var item in reps)
            {
                louc--;
                htmlString += "<div id='pinl_main'>";//最外层
                htmlString += "<div id='pinl_main_main'>";//第一层
                htmlString += "<div>";//第一层中的图片
                htmlString += "<img onload='imgWidth(this)' src=" + item.Person.Avarda + ">";
                htmlString += "</div>";//图片结束
                htmlString += "<div>";//评论内容
                htmlString += "<p>" + item.Title + "</p>";
                htmlString += "<p>" + item.Content + "</p>";
                htmlString += "</div>";//内容结束
                htmlString += "<div>";//第一层中的功能栏
                htmlString += "<ul id='" + item.ID + "'>";
                htmlString += "<li>#" + louc + "</li>";
                htmlString += "<li>" + item.CreateDateTime.ToString("yyyy年MM月hh日") + "</li>";
                htmlString += "<li onclick='ADD(this)'>回复</li>";
                htmlString += "<li><i class='glyphicon glyphicon-thumbs-up'></i>（" + item.Like + "）</li>";
                htmlString += "<li><i class='glyphicon glyphicon-thumbs-down'></i>（" + item.Hate + "）</li>";
                htmlString += "<li onclick='pinlMain(this)'>对话内容</li>";
                htmlString += "</ul>";
                htmlString += "</div>";//功能栏结束
                htmlString += "</div>";//第一层结束
                htmlString += "<div id='pinl_main_second'   class='a" + louc + "'>";//第二层回复

                if (prenreply.Where(x => x.ParentReply.ID == item.ID).ToList().Count > 0)
                {
                    foreach (var ry in prenreply.Where(x => x.ParentReply.ID == item.ID).OrderBy(x => x.CreateDateTime).ToList())
                    {
                        htmlString += "<div id='pinl_main_main'>";//第一层
                        htmlString += "<div style='margin-bottom:50px'>";//第一层中的图片
                        htmlString += "<img style='width:40px;height:40px'  src=" + ry.Person.Avarda + ">";
                        htmlString += "</div>";//图片结束
                        htmlString += "<div>";//评论内容
                        if (_context.Replys.Find(ry.ParentReplyNameID) != null)
                        {
                            htmlString += "<p>" + ry.Title + " 回复  " + _context.Replys.Find(ry.ParentReplyNameID).Title + " : " + ry.Content + "</p>";
                        }
                        else
                        {
                            htmlString += "<p>" + ry.Title + " 回复  " + item.Title + " : " + ry.Content + "</p>";
                        }

                        htmlString += "</div>";//内容结束
                        htmlString += "<div>";//第一层中的功能栏
                        htmlString += "<ul id='" + ry.ID + "'>";
                        htmlString += "<li></li>";
                        htmlString += "<li>" + ry.CreateDateTime.ToString("yyyy年MM月hh日")+"</li>";
                        htmlString += "<li onclick='ADD(this)'>回复</li>";
                        htmlString += "<li><i class='glyphicon glyphicon-thumbs-up'></i>(" + item.Like + ")</li>";
                        htmlString += "<li><i class='glyphicon glyphicon-thumbs-down'></i>(" + item.Hate + ")</li>";
                        htmlString += "<li>对话内容</li>";
                        htmlString += "</ul>";
                        htmlString += "</div>";//功能栏结束
                        htmlString += "</div>";//第一层结束
                    }

                }
                htmlString += "</div>";//第二层回复结束
                if (prenreply.Where(x => x.ParentReply.ID == item.ID).ToList().Count > 1)
                    htmlString += "<p class='pinl_p' id='a" + louc + "' onclick='openmain(this)'>共" + prenreply.Where(x => x.ParentReply.ID == item.ID).ToList().Count + "条回复，点击查看</p>";
                htmlString += "<hr>";
                htmlString += "</div>";
            }
            return Json(htmlString);
        }

        /// <summary>
        /// 点赞
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Like(Guid id)
        {
            //判断用户是否登陆

            //判断用户是否队这条回复点过赞或踩

            //保存 Reply实体中like +1 或hate+1 LikeReply添加一条纪录

            //生成html 注入视图
            return Json("OK");
        }

        /// <summary>
        /// 踩
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Hate(Guid id)
        {
            //判断用户是否登陆

            //判断用户是否队这条回复点过赞或踩

            //保存 Reply实体中like +1 或hate+1 LikeReply添加一条纪录

            //生成html 注入视图
            return Json("OK");
        }
    }
}
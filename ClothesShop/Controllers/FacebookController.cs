using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ClothesShop.Models;
using ClothesShop.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Facebook;

namespace ClothesShop.Controllers
{
    public class FacebookController : Controller
    {
        private readonly ClothesShopContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly string pageToken;
        private readonly FacebookClient client;

        public FacebookController(ClothesShopContext context, IWebHostEnvironment iwebHostEnvironment) 
        {
            _context = context;
            webHostEnvironment = iwebHostEnvironment;
            pageToken = "EAAFAcZBZAUmBEBAMDgVtfBmYwE355MpZA8ySKRy3bYA4QBTQofCZAY4vRedYhuXmWUg6ivlxJpEP9dFt2nMTCQdfQErk1MsB3c9k6UwiqBYOpc4oZCmHeVVI8RdMYFZCJX5FkVUwDWuFZCrspeavYOdkwTk6pBbUmDiMXIBVOk99KFHTkdvywXh";
            client = new FacebookClient(pageToken);
        } 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> postOnFacebook(string message, string link=null, IFormFile img=null)
        {
            /*if (HttpContext.Session.GetInt32("adminId") == null)
            {
                return View("Views/Users/NotFound.cshtml");
            }*/

            /*string pathToSave = await SaveImageFile(img, );*/

            var postMessage = message + "\n >>>>>>" + link;

            FacebookPost postObj = new FacebookPost()
            {
                message = postMessage,
                image = img
            };
            await client.PostTaskAsync("/109078328141449/feed", postObj);

            /*await _context.SaveChangesAsync();*/
            return Redirect("/Admin/Facebook");
        }
    }
}

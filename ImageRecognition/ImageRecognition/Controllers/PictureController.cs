using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.IO;
//using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

using WebApp.Models;

namespace WebApplication4.Controllers
{
    public class PictureController : Controller
    {
        
        public ActionResult Images(List<ImageRecognitionModule.ImageModel>models)
        {
            var images = models.Select(x => new Picture
            {
                Path = Url.Content("~\\"+x.path)
            });
            
            //foreach (var img in images) Debug.WriteLine(img);
            return PartialView(images);
        }
    }
}
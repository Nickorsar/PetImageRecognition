using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
//using System.Web.Mvc;
//using System.Data.Entity;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using Npgsql;

//using ImageClassifyModule;
using ImageRecognitionModule;






namespace WebApp.Models
{
    public class HomeController : Controller
    {
       // string AllowedTypes = "image/jpeg | image/png | image/bmp";

        public ActionResult Index()
        {
            ViewBag.Title = "Image Recognition";     
            return View("Index");
        }

        

        [HttpPost]
        public ActionResult Index(IFormFile file)
        {

            try
            {
                var imageRecognition = new ImageRecognition(new ImageRecognitionParameters
                {
                    DatabaseDSN = "Server=127.0.0.1;User Id=postgres;Password=12345;Port=5432;Database=image-recognition;",
                    MaxDistance = 250,
                    MinMatches = 5,
                    MaxWidth = 1024
                });
                if (file == null) throw new Exception();
                string fileName = System.IO.Path.GetFileName(file.FileName);
                ViewBag.Filename = file.FileName;
                string fileNameServer = "~/Content/Images/" + fileName;
                System.IO.Path.GetRandomFileName();

                //file.SaveAs(Server.MapPath(fileNameServer));
               // Debug.WriteLine(fileNameServer);
                var ms = new MemoryStream();
                file.CopyTo(ms);
                
                byte[] bytes;

                bytes = ms.ToArray();

                string base64 = Convert.ToBase64String(bytes);

                ViewBag.Image = base64;

                //List<string> files = new();

                //var sql = "SELECT * FROM image_descriptors";
                //using var connection = new NpgsqlConnection(imageRecognition._connectionDsn);
                //connection.Open();

               // using var cmd = new NpgsqlCommand(sql, connection);
               // using NpgsqlDataReader reader = cmd.ExecuteReader();

               // while (reader.Read())
               // {
               //     files.Add(reader.GetString(1));
               // }

                //Debug.WriteLine(files.Count + " изображений");
                //foreach(var path in files)
                //{
                //    Debug.WriteLine(path);
                //}

                var similar = imageRecognition.FindSimilar(file.OpenReadStream());
                //foreach (var image in similar)
                //{
                //    Debug.WriteLine(image.path);
                //}
                //System.IO.File.Delete(Server.MapPath(fileNameServer));
                ViewBag.Count = similar.Count;

                Debug.WriteLine(similar.Count + " изображений похожих");
                if (similar.Count == 0) return View("SearchNull");

                foreach (var img in similar)
                {
                    img.path = img.path.Replace("\\", "/");
                    img.path = img.path.Replace("wwwroot/", "");

                } 
                //foreach (var image in similar)
                //{
                //    Debug.WriteLine(image.path);
                //}
                //var images = similar.Select(x => new Picture
                //{
                //    Path = Url.Content(x)
                //});

                return View("Search", similar);
            }
            catch (Exception) { return Redirect(Url.Content("~/Home/Index")); }
        }

        public ActionResult How()
        {
            return View("How");
        }

        public ActionResult About()
        {
            return View("About");
        }
    }
}

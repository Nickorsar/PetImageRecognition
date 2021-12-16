using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

using System.Configuration;
using Npgsql;
using ImageRecognitionModule;

namespace WebApplication4.Controllers
{
    public class UploadController : Controller
    {
        string AllowedTypes = "image/jpeg | image/png | image/bmp";
        private readonly IHostEnvironment _env;

        public UploadController(IHostEnvironment env)
        {
            _env = env;
        }

        public bool Insert(string Path)
        {
            Debug.WriteLine(Path);
            var filePath = _env.ContentRootPath;
            var PathString = System.IO.Path.Combine(filePath, "wwwroot",Path);
            Debug.WriteLine(Path);
            var PathStringRoot  = System.IO.Path.Combine("wwwroot", Path);
            FileStream fs = new(PathString, FileMode.Open);
            try
            {
                if (!System.IO.File.Exists(PathString)) throw new Exception("Файл не найден");
               // var descriptors = PictureUtils.GetPHash(Server.MapPath(Path), 128);

                var descriptors = PictureUtils.ComputeDescriptors(fs, 512);
                if (descriptors == null) throw new Exception("Не вычислились дескрипторы");
                Debug.WriteLine(descriptors.Length + "элементов в массиве");


                var sql = $"INSERT INTO image_descriptors(path, descriptor) VALUES('{PathStringRoot}', @descriptor)";
                using (var connection = new NpgsqlConnection("Server=127.0.0.1;User Id=postgres;Password=12345;Port=5432;Database=image-recognition;"))
                {
                    
                    using (var cmd = new NpgsqlCommand(sql, connection)) 
                    {
                        NpgsqlParameter param = cmd.CreateParameter();
                        param.ParameterName = "@descriptor";
                        param.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Bytea;
                        param.Value = descriptors;
                        cmd.Parameters.Add(param);
                        
                        connection.Open();
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                }

                fs.Close();
                return true;
            }
            catch (Exception ex) {fs.Close(); Debug.WriteLine(ex.ToString()); return false; }

        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public ActionResult Upload(IFormFileCollection files)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var file in files)
            {
                if (file != null)
                {

                    var fileType = file.ContentType;
                    if (AllowedTypes.Contains(fileType))
                    {
                        var filePath = _env.ContentRootPath;
                        string fileName = System.IO.Path.GetFileName(file.FileName);
                        string fileNameServer = Path.Combine( "Content\\Images", System.IO.Path.GetRandomFileName() + fileName);
                        //string fileNameServer = $"{filePath}\\Content\\Images\\" + System.IO.Path.GetRandomFileName() + fileName;
                        using (var fileStream = new FileStream(Path.Combine(filePath,"wwwroot", fileNameServer), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        
                            //file.SaveAs(Server.MapPath(fileNameServer));
                        if (!Insert(fileNameServer))
                        {
                            System.IO.File.Delete(fileNameServer);
                        }
                    }
                }
            }
            stopwatch.Stop();
            Debug.WriteLine(stopwatch.ElapsedMilliseconds / 1000f+"sec");
            return View("Upload");
        }


        public ActionResult Upload()
        {
            ViewBag.Title = "Fill DataBase";
            return View();
        }

        [HttpGet]
        public ActionResult Upload(List<string> list)
        {
            ViewBag.Title = "Fill DataBase";
            if (list != null && list.Count != 0)
                ViewBag.Result = $"Было загружено {list.Count} файла(ов)";
            return View(list);
        }
    }
}
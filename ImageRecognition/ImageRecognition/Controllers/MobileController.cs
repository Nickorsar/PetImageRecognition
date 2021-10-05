using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using WebApp.Models;
using ImageClassifyModule;
using Npgsql;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Web;



namespace WebApplication4.Controllers
{
    public class MobileController : ApiController
    {
       [HttpPost]
       [Route("Mobile/Search")]
       public async Task<IHttpActionResult> Search()
        {
            Console.WriteLine("попытка запроса");
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest();
            }
            var provider = new MultipartMemoryStreamProvider();

            await Request.Content.ReadAsMultipartAsync(provider);

            List<string> returnByteString = new List<string>();
            try
            {
                var imageClassify = new ImageClassify<int>(new ImageClassifyParameters
                {
                    DatabaseDSN = ConfigurationManager.ConnectionStrings["Postgres"].ConnectionString,
                    ClassifyModelPath = System.Web.HttpContext.Current.Server.MapPath("~/App_data/mnist_models/mnist_models.pb"),
                    ClassifyImageSize = 28,
                    MaxHammingDistance = 7,
                });
                
                foreach(var file in provider.Contents)
                {
                    string fileNameServer = "~/Content/Images/" + Path.GetRandomFileName();

                    byte[] fileArray = await file.ReadAsByteArrayAsync();

                    using(FileStream fs = new FileStream(fileNameServer, FileMode.Create))
                    {
                        await fs.WriteAsync(fileArray, 0, fileArray.Length);
                    }

                    var similar = imageClassify.FindSimilar(System.Web.HttpContext.Current.Server.MapPath(fileNameServer));
                    System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath(fileNameServer));
                    return Ok(similar);
                }

               // byte[] bytes = Encoding.Default.GetBytes(stringbytes);
               // MemoryStream imageStream2 = new MemoryStream(bytes);
               
                //string fileName = System.IO.Path.GetFileName(file.FileName);
             
              
            }
            catch
            {

            }
            return Ok();
        }

        public bool Insert(string Path)
        {
            try
            {
                if (!System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(Path))) throw new Exception("Файл не найден");
                var hash = PictureUtils.GetPHash(System.Web.HttpContext.Current.Server.MapPath(Path), 28);
                if (hash == null) throw new Exception("не вычислился хэш");
                string hashString = "{";
                for (int i = 0; i < hash.Length; i++)
                {
                    hashString += Convert.ToInt32(hash[i]).ToString();
                    if (i + 1 != hash.Length) hashString += ",";
                    else hashString += "}";
                }

                // string md5 = PictureUtils.GetMD5Hash(Server.MapPath(Path));
                var sql = $"INSERT INTO image(path,md5_hash,perceptual_hash) VALUES('{Path}','{Path}' ,'{hashString}')";
                using (var connection = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["Postgres"].ConnectionString))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand(sql, connection)) { cmd.ExecuteNonQuery(); }
                }


                return true;
            }
            catch (Exception) { return false; }

        }

        [HttpPost]
        [Route("Mobile/Upload")]
        public async Task<IHttpActionResult> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest();
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            foreach (var stringbyte in provider.Contents)
            { 
                string fileNameServer = "~/Content/Images/" + Path.GetRandomFileName();
                byte[] fileArray = await stringbyte.ReadAsByteArrayAsync();

                using (FileStream fs = new FileStream(fileNameServer, FileMode.Create))
                {
                      await fs.WriteAsync(fileArray, 0, fileArray.Length);
                }

                if (!Insert(fileNameServer)) System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath(fileNameServer));
            }
            return Ok();
        }
    }
}

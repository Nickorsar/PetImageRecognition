using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageRecognitionModule;
using ImageRecognitionService.Models;
using Npgsql;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ImageRecognitionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        readonly string AllowedTypes = "image/jpeg | image/png | image/bmp";
        private readonly IHostEnvironment _env;

        public UtilityController(IHostEnvironment env)
        {
            _env = env;
        }

        // GET: api/<UtilityController>
        [HttpGet]
        public string Get()
        {
            return "value2";
        }

        // GET api/<UtilityController>/5
        [HttpGet("{id}")]
        public CheckResponse Get(int id)
        {
            return new CheckResponse
            {
                idResponse = id,
                StringResponse = $"blabla + {id}"
            };
        }

        [HttpGet("path")]
        public CheckResponse GetImagesPath()
        {
            return new CheckResponse
            {
                idResponse = 1000,
                StringResponse = @"wwwroot\Content\Images\"
            };
        }

        // POST api/<UtilityController>
        [HttpPost("post")]
        public CheckResponse Post(IFormFile file)
        {
            if(file == null) throw new Exception("kek");
            var fileType = file.ContentType;
            
            
                var filePath = _env.ContentRootPath;
                string fileName = System.IO.Path.GetFileName(file.FileName);
                string fileNameServer = Path.Combine("Content\\Images",  fileName);
                //string fileNameServer = $"{filePath}\\Content\\Images\\" + System.IO.Path.GetRandomFileName() + fileName;
                using (var fileStream = new FileStream(Path.Combine(filePath, "wwwroot", fileNameServer), FileMode.Create))
                {
                    
                    file.CopyTo(fileStream);
                }
                return new CheckResponse
                {
                    idResponse = 228,
                    StringResponse = Path.Combine(filePath, "wwwroot", fileNameServer)
                };


        }

        // PUT api/<UtilityController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UtilityController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using ImageRecognitionModule;

namespace WebApp.Models
{
    public class ImageContext : DbContext
    {
        public DbSet<ImageModel> Images{get;set;}
        public ImageContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server = 127.0.0.1; User Id = postgres; Password = 12345; Port = 5432; Database = image - recognition;");
        }

    }
}

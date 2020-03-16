using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Panda.App.Data;
using Panda.App.Domain;
using Panda.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Panda.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly PandaDbContext db;

        public HomeController(PandaDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            List<PackageHomeViewModel> model = this.db.Packages
                .Where(package => package.Recipient.UserName == this.User.Identity.Name)
                .Include(package => package.Status)
                .Select(package => new PackageHomeViewModel
                {
                    Id = package.Id,
                    Description = package.Description,
                    Status = package.Status.Name
                })
                .ToList();

            return this.View(model);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Panda.App.Data;
using Panda.App.Domain;
using Panda.App.Models;

namespace Panda.App.Controllers
{
    public class PackageController : Controller
    {
        private readonly PandaDbContext db;

        public PackageController(PandaDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            this.ViewData["Recipients"] = this.db.Users.ToList();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(PackageCreateBindingModel bindingModel)
        {
            Package package = new Package
            {
                Description = bindingModel.Description,
                Weight = bindingModel.Weight,
                ShippingAddress = bindingModel.ShippingAddress,
                Recipient = this.db.Users.SingleOrDefault(x => x.UserName == bindingModel.Recipient),
                Status = this.db.PackageStatus.SingleOrDefault(x => x.Name == "Pending") 
            };
            this.db.Packages.Add(package);
            this.db.SaveChanges();

            return this.Redirect("/Package/Pending");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Pending()
        {
            List<PendingPackageViewModel> viewModel = this.db.Packages
                .Where(package => package.Status.Name == "Pending")
                .Select(x => new PendingPackageViewModel
                {
                    Id = x.Id,
                    Description = x.Description,
                    ShippingAddress = x.ShippingAddress,
                    Weight = x.Weight,
                    Recipient = x.Recipient.UserName
                })
                .ToList();

            return this.View(viewModel);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Shipped()
        {
            List<ShippedPackageViewModel> viewModel = this.db.Packages
                .Where(package => package.Status.Name == "Shipped")
                .Select(x => new ShippedPackageViewModel
                {
                    Id = x.Id,
                    Description = x.Description,
                    EstimatedDeliveryDate = x.EstimatedDeliveryDate.ToString("dd/mm/yyyy", CultureInfo.InvariantCulture),
                    Weight = x.Weight,
                    Recipient = x.Recipient.UserName
                })
                .ToList();

            return this.View(viewModel);
        }

        [HttpGet("/Package/Ship/{id}")]
        [Authorize]
        public IActionResult Ship(string id)
        {
            Package package = this.db.Packages.SingleOrDefault(x => x.Id == id);

            package.Status = this.db.PackageStatus.SingleOrDefault(status => status.Name == "Shipped");
            package.EstimatedDeliveryDate = DateTime.Now.AddDays(new Random().Next(20, 40));
            this.db.Update(package);
            this.db.SaveChanges();

            return Redirect("/Package/Pending");
        }

        [HttpGet("/Package/Deliver/{id}")]
        [Authorize]
        public IActionResult Deliver(string id)
        {
            Package package = this.db.Packages.SingleOrDefault(x => x.Id == id);

            package.Status = this.db.PackageStatus.SingleOrDefault(status => status.Name == "Delivered");
            this.db.Update(package);
            this.db.SaveChanges();

            return Redirect("/Package/Shipped");
        }

        [HttpGet("/Package/Acquire/{id}")]
        [Authorize]
        public IActionResult Acquire(string id)
        {
            Package package = this.db.Packages.SingleOrDefault(x => x.Id == id);

            package.Status = this.db.PackageStatus.SingleOrDefault(status => status.Name == "Acquired");
            this.db.Update(package);

            Receipt receipt = new Receipt
            {
                Fee = (decimal)(2.67 * package.Weight),
                IssuedOn = DateTime.Now,
                Package = package,
                Recipient = this.db.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name)
            };

            this.db.Receipts.Add(receipt);

            this.db.SaveChanges();

            return Redirect("/Home/Index");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Delivered()
        {
            List<DeliveredPackageViewModel> viewModel = this.db.Packages
                .Where(package => package.Status.Name == "Delivered" || package.Status.Name == "Acquired")
                .Select(x => new DeliveredPackageViewModel
                {
                    Id = x.Id,
                    Description = x.Description,
                    ShippingAddress = x.ShippingAddress,
                    Weight = x.Weight,
                    Recipient = x.Recipient.UserName
                })
                .ToList();

            return this.View(viewModel);
        }

        [HttpGet("/Package/Details/{id}")]
        [Authorize]
        public IActionResult Details(string id)
        {
            Package package = this.db.Packages
                .Include(x => x.Recipient)
                .Include(x => x.Status)
                .SingleOrDefault(x => x.Id == id);

            PackageDetailsViewModel viewModel = new PackageDetailsViewModel
            {
                Description = package.Description,
                Recipient = package.Recipient.UserName,
                ShippingAddress = package.ShippingAddress,
                Weight = package.Weight,
                Status = package.Status.Name
            };

            if (package.Status.Name == "Pending")
            {
                viewModel.EstimatedDeliveryDate = "N/A";
            }
            else if (package.Status.Name == "Shipped")
            {
                viewModel.EstimatedDeliveryDate = package.EstimatedDeliveryDate.ToString("dd/MM/YYYY", CultureInfo.InvariantCulture);
            }
            else
            {
                viewModel.EstimatedDeliveryDate = "Delivered";
            }

            return this.View(viewModel);
        }
    }
}
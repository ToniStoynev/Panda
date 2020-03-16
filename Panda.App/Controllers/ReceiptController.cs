using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Panda.App.Data;
using Panda.App.Domain;
using Panda.App.Models;

namespace Panda.App.Controllers
{
    public class ReceiptController : Controller
    {
        private readonly PandaDbContext db;

        public ReceiptController(PandaDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        [Authorize]
        public IActionResult My()
        {
            List<PackageReceiptViewModel> viewModel = this.db.Receipts
                .Where(receipt => receipt.Recipient.UserName == this.User.Identity.Name)
                .Select(receipt => new PackageReceiptViewModel
                {
                    Id = receipt.Id,
                    IssuedOn = receipt.IssuedOn.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    Fee = receipt.Fee,
                    Recipient = receipt.Recipient.UserName
                })
                .ToList();

            return View(viewModel);
        }

        [HttpGet("/Receipt/Details/{id}")]
        [Authorize]
        public IActionResult Details(string id)
        {
            Receipt receiptFromDb = this.db.Receipts
                .Include(package => package.Package)
                .Include(recipient => recipient.Recipient)
                .SingleOrDefault(x => x.Id == id);

            var model = new ReceiptDetailsViewModel
            {
                ReceiptNumber = receiptFromDb.Id,
                DeliveryAddress = receiptFromDb.Package.ShippingAddress,
                Description = receiptFromDb.Package.Description,
                IssuedOn = receiptFromDb.IssuedOn.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                Reciepient = receiptFromDb.Recipient.UserName,
                Weight = receiptFromDb.Package.Weight,
                Total = (decimal)(receiptFromDb.Package.Weight * 2.67)
            };
                

            return this.View(model);
        }
    }
}
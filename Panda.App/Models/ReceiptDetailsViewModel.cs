using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Panda.App.Models
{
    public class ReceiptDetailsViewModel
    {
        public string ReceiptNumber { get; set; }

        public string IssuedOn { get; set; }

        public string DeliveryAddress { get; set; }

        public double Weight { get; set; }

        public string Description { get; set; }

        public string Reciepient { get; set; }

        public decimal Total { get; set; } 
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Panda.App.Models
{
    public class ShippedPackageViewModel
    {
        public string Id { get; set; }
        public string Description { get; set; }

        public double Weight { get; set; }

        public string EstimatedDeliveryDate { get; set; }

        public string Recipient { get; set; }
    }
}

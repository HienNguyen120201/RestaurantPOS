using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantManagement.Models
{

    public class BillViewModel
    {
        public string FoodName { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }

    public class PaymentViewModel
    {
        public Guid BillId { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<BillViewModel> BillToPay { get; set; }
        public int Total { get; set; }
        public string PaymentMethod { get; set; }
    }
}

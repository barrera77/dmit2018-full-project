using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBikeSystem.ModelViews.Purchasing
{
    public class PurchaseOrderView
    {
        public int PurchaseOrderID { get; set; }
        public int PurchaseOrderNumber { get; set; }
        public int VendorID { get; set; }
        public decimal SubTotal { get; set; }
        public decimal GST { get; set; }
        public int EmployeeID { get; set; }
        public List<PurchaseOrderDetailView> PurchaseOrderDetails { get; set; }
    }
}

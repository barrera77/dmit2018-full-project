using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBikeSystem.ModelViews.Purchasing
{
    public class PurchaseOrderDetailView
    {
        public int PurchaseOrderDetailID { get; set; }
        public int PartID { get; set; }
        public string Description { get; set; }
        public int QOH { get; set; }
        public int ROL { get; set; }
        public int QOO { get; set; }
        public int QTO { get; set; }
        public decimal Price { get; set; }
    }
}

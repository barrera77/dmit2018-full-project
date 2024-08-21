using eBikeSystem.Entities;
using eBikeSystem.ModelViews.Purchasing;


namespace eBikeSystem.BLL.Purchasing
{
    public class PurchasingServices
    {

        private readonly eBikeContext _context;
        
        internal PurchasingServices (eBikeContext context)
        {
            _context = context;
        }

        #region Validation
        List<Exception> errorList = new List<Exception>();

        #endregion


        #region Query Service Methods

        public List<VendorView> GetVendors()
        {
            return _context.Vendors
                    .Select(v => new VendorView
                    {
                        VendorID = v.VendorId,
                        VendorName = v.VendorName,
                        HomePhone = v.Phone,
                        City = v.City,
                    })
                    .ToList();
        }

        public PurchaseOrderView GetPurchaseOrder(int vendorID) 
        {
            PurchaseOrder? existingPO = _context.PurchaseOrders
                                        .FirstOrDefault(po => po.VendorId == vendorID && po.OrderDate == null);

            if(existingPO != null)
            {
                return _context.PurchaseOrders
                .Where(po => po.VendorId == vendorID && po.OrderDate == null)
                .Select(po => new PurchaseOrderView
                {
                    PurchaseOrderID = po.PurchaseOrderId,
                    PurchaseOrderNumber = po.PurchaseOrderNumber,
                    VendorID = po.VendorId,
                    SubTotal = po.SubTotal,
                    GST = po.TaxAmount,
                    EmployeeID = po.EmployeeId,
                    PurchaseOrderDetails = _context.PurchaseOrderDetails
                                            .Where(pod => pod.PurchaseOrderId == po.PurchaseOrderId)
                                            .Select(pod => new PurchaseOrderDetailView
                                            {
                                                PurchaseOrderDetailID = pod.PurchaseOrderId,
                                                PartID = pod.PartId,
                                                Description = pod.Part.Description,
                                                QOH = pod.Part.QuantityOnHand,
                                                ROL = pod.Part.ReorderLevel,
                                                QOO = pod.Part.QuantityOnOrder,
                                                QTO = pod.Quantity,
                                                Price = pod.PurchasePrice,
                                            })
                                            .ToList()
                })
                .FirstOrDefault();
            }

            PurchaseOrderView newPO = new PurchaseOrderView()
            {
                VendorID = vendorID,
                PurchaseOrderDetails = _context.PurchaseOrderDetails
                                        .Where(pod => pod.Part.ReorderLevel == (pod.Part.ReorderLevel-(pod.Part.QuantityOnHand + pod.Quantity)))
                                        .Select(pod => new PurchaseOrderDetailView
                                        {
                                            PurchaseOrderDetailID = pod.PurchaseOrderId,
                                            PartID = pod.PartId,
                                            Description = pod.Part.Description,
                                            QOH = pod.Part.QuantityOnHand,
                                            ROL = pod.Part.ReorderLevel,
                                            QOO = pod.Part.QuantityOnOrder,
                                            QTO = pod.Quantity,
                                            Price = pod.PurchasePrice,
                                        })
                                        .ToList()
            };

            return newPO;
        }

        public List<ItemView> GetInventory(int vendorID)
        {
            return new List<ItemView> ();
        }

        #endregion

        #region Command Methods
        public void PurchaseOrder_Save(PurchaseOrderView purchaseOrderView)
        {
            

        }

        public void DeleteOrder(int purchaseOrderID)
        {

        }

        #endregion
    }
}


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
            return _context.Parts
                    .Where(p => p.VendorId == vendorID && p.PartId != p.PurchaseOrderDetails.Where(pd => pd.PurchaseOrder.OrderDate == null).Select(pod => pod.PartId).FirstOrDefault())
                    .Select(part => new ItemView
                    {
                        PartID = part.PartId,
                        Description = part.Description,
                        QOH = part.QuantityOnHand,
                        ROL = part.ReorderLevel,
                        QOO = part.QuantityOnOrder,
                        Buffer = (part.ReorderLevel - (part.QuantityOnHand - part.QuantityOnOrder)) >= 0 ? 0 : (part.ReorderLevel - (part.QuantityOnHand - part.QuantityOnOrder)) * -1,
                        Price = part.PurchasePrice,
                    })
                    .ToList();            
        }

        #endregion

        #region Command Methods
        public void PurchaseOrder_Save(PurchaseOrderView purchaseOrderView)
        {
            PurchaseOrder poToAdd = _context.PurchaseOrders.FirstOrDefault(po => po.PurchaseOrderId == purchaseOrderView.PurchaseOrderID);

            if (poToAdd.OrderDate != null)
            {
                throw new ArgumentException("Purchase order already closed");
            }
            if(poToAdd.PurchaseOrderDetails.Count <= 0)
            {
                throw new ArgumentException("Purchase order does not contain any items in order");
            }

            foreach(var purchase in poToAdd.PurchaseOrderDetails)
            {
                if(purchase.Quantity <= 0)
                {
                    errorList.Add(new Exception($"Quantity for PO item{purchase.Part.Description} must have a positive non-zero value"));
                }
                if(purchase.PurchasePrice <= 0)
                {
                    errorList.Add(new Exception($"Price for PO item {purchase.Part.Description} must have a positive non-zero value"));
                }
            }
            
            if(errorList.Count > 0)
            {
                throw new AggregateException("Please acheck error message(s):", errorList);
            }

            int lastPoNumber = _context.PurchaseOrders.OrderByDescending(po => po.PurchaseOrderNumber)
                                                       .Select(po => po.PurchaseOrderNumber).FirstOrDefault();
            poToAdd = new PurchaseOrder()
            {
                PurchaseOrderNumber = purchaseOrderView.PurchaseOrderNumber == 0 ? lastPoNumber + 1 : purchaseOrderView.PurchaseOrderNumber,
                OrderDate = DateTime.Now,
                TaxAmount = purchaseOrderView.GST,
                SubTotal = purchaseOrderView.SubTotal,
                Notes = "",
                EmployeeId = purchaseOrderView.EmployeeID,
                VendorId = purchaseOrderView.VendorID,
            };

            if(errorList.Count > 0)
            {
                _context.ChangeTracker.Clear();

                throw new AggregateException("Please check error message(s): ", errorList);
            }
           
            //Create the prchaseOrderDetails

            foreach(var poDetail in purchaseOrderView.PurchaseOrderDetails)
            {
                if(purchaseOrderView.PurchaseOrderDetails.Count > 0)
                {
                    PurchaseOrderDetail newPoDetail = new PurchaseOrderDetail()
                    {
                        PurchaseOrderId = poDetail.PurchaseOrderDetailID,
                        PartId = poDetail.PartID,
                        Quantity = poDetail.QTO,
                        PurchasePrice = poDetail.Price,
                    };

                    //Find the part to update
                    Part partToUpdate = _context.Parts.FirstOrDefault( p => p.PartId == poDetail.PartID);

                    //update the QOO for the item
                    partToUpdate.QuantityOnOrder += poDetail.QTO;
                    _context.Update(partToUpdate);


                }

            }







        }

        public void DeleteOrder(int purchaseOrderID)
        {
            PurchaseOrder poToDelete = _context.PurchaseOrders.FirstOrDefault(po => po.PurchaseOrderId == purchaseOrderID);
            if(poToDelete.OrderDate != null)
            {
                throw new ArgumentException("Orders currently in place cannot be deleted");
            }

            if(errorList.Count > 0)
            {
                throw new AggregateException("Please acheck error message(s):", errorList);
            }
            else
            {
                foreach(var po in poToDelete.PurchaseOrderDetails)
                {
                    _context.PurchaseOrderDetails.Remove(po);
                }

                _context.PurchaseOrders.Remove(poToDelete);
            }
        }

        #endregion
    }
}


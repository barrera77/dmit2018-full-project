using eBikeSystem.BLL.Purchasing;
using eBikeSystem.Entities;
using eBikeSystem.ModelViews.Purchasing;
using Microsoft.AspNetCore.Components;
using TakeHomeExercise4WebApp.Components;

namespace eBikeWebApp.Components.Pages.eBikePages
{
    public partial class Purchasing : ComponentBase
    {

        #region Properties & Fields

        [Inject]
        private NavigationManager _navManager { get; set; }

        [Inject]
        PurchasingServices PurchasingServices { get; set; }

        //Fields

        [Parameter]
        public int VendorId { get; set; }

        [Parameter]
        public string PoNumber { get; set; }

        private decimal poSubtotal { get; set; }
        private decimal poGST { get; set; }
        private decimal poTotal { get; set; }
        private decimal itemPrice { get; set; }
        private bool isNewOrder { get; set; }


        public List<VendorView> vendorsList { get; set; } = new List<VendorView>();
        public VendorView SelectedVendor { get; set; } = new VendorView();

        public PurchaseOrderView? PurchaseOrder { get; set; } =  new PurchaseOrderView();   
        
        public PurchaseOrderDetail PoDetail { get; set; } = new PurchaseOrderDetail();
        public List<PurchaseOrderDetailView> purchaseOrderDetailsList { get; set; } = new List<PurchaseOrderDetailView>();  

        public List<ItemView> ItemsList { get; set; } = new List<ItemView>();

        #endregion

        #region Feedback and Error Messaging

        private string feedbackMessage = "";
        private string errorMessage;

        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage);

        private List<string> errorDetails = new List<string>();

        #endregion


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            try
            {
                vendorsList = PurchasingServices.GetVendors();
            }
            catch (AggregateException ex)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage += Environment.NewLine;
                }

                foreach (Exception error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }
            catch (ArgumentNullException ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;

            }
            catch (Exception ex)
            {
                errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
            }
        }

        private async Task OnHandleSelectedVendor(ChangeEventArgs e)
        {
            VendorId = Convert.ToInt32(e.Value);
            ItemsList.Clear();
            purchaseOrderDetailsList.Clear();



           if (VendorId > 0)
            {
                try
                {
                    //Display vendor info
                    SelectedVendor = vendorsList.FirstOrDefault(v => v.VendorID == VendorId);                   
                }
                catch (AggregateException ex)
                {
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage += Environment.NewLine;
                    }
                    errorMessage += "Vendor not found";
                    foreach (Exception error in ex.InnerExceptions)
                    {
                        errorDetails.Add(error.Message);
                    }
                }
                catch (ArgumentNullException ex)
                {
                    errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
                }
                catch (Exception ex)
                {
                    errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
                }
            }
            else
            {
                SelectedVendor = new VendorView();
            }
        }

        private void DisplayPODetails(int vendorId)
        {
            poGST = 0;
            poSubtotal = 0; 
            poTotal = 0;
            itemPrice = 0;
            PurchaseOrder = null;
            purchaseOrderDetailsList.Clear();
            ItemsList.Clear();


            if (vendorId > 0)
            {
                try
                {
                    PurchaseOrder = PurchasingServices.GetPurchaseOrder(vendorId);
                    ItemsList = PurchasingServices.GetInventory(vendorId);

                    foreach (var item in ItemsList)
                    {
                        item.Price = Math.Round(item.Price, 2);
                    }

                    //Update totals
                    poSubtotal = Math.Round(PurchaseOrder.SubTotal, 2);
                    poGST = Math.Round(PurchaseOrder.GST, 2);
                    poTotal = Math.Round((poSubtotal + poGST), 2);
                    

                    //get the PO details
                    purchaseOrderDetailsList = PurchaseOrder.PurchaseOrderDetails;
                    if (PurchaseOrder.PurchaseOrderID <= 0)
                    {
                        PoNumber = "New Order";
                        isNewOrder = true;
                    }
                    else
                    {
                        
                        PoNumber = PurchaseOrder.PurchaseOrderNumber.ToString();
                        isNewOrder = false;

                        foreach(var detail in purchaseOrderDetailsList)
                        {
                            detail.Price = Math.Round(detail.Price, 2); 
                        }                        
                    }
                }
                catch (AggregateException ex)
                {
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage += Environment.NewLine;
                    }
                    errorMessage += "Vendor not found";
                    foreach (Exception error in ex.InnerExceptions)
                    {
                        errorDetails.Add(error.Message);
                    }
                }
                catch (ArgumentNullException ex)
                {
                    errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
                }
                catch (Exception ex)
                {
                    errorMessage = BlazorHelperClass.GetInnerException(ex).Message;
                }
            }            
        }

        
        private void RemovePurchaseOrderDetail(int partId)
        {
            if(partId > 0)
            {
                PurchaseOrderDetailView partToRemove = purchaseOrderDetailsList.FirstOrDefault(p => p.PartID == partId);

                if (partToRemove != null)
                {
                    //Remove part from PO
                    purchaseOrderDetailsList.Remove(partToRemove);

                    ItemView ItemToAdd = new ItemView
                    {
                        PartID = partToRemove.PartID,
                        Description = partToRemove.Description,
                        QOH = partToRemove.QOH,
                        ROL = partToRemove.ROL,
                        QOO = partToRemove.QOO,
                        Buffer = (partToRemove.ROL - (partToRemove.QOH - partToRemove.QOO)) >= 0 ? 0 : (partToRemove.ROL - (partToRemove.QOH - partToRemove.QOO)),
                        Price = partToRemove.Price,
                    };

                    //Return part to vendor inventory
                    ItemsList.Add(ItemToAdd);
                    StateHasChanged();
                }

                else
                {
                    errorMessage = "Part not found";
                }
                


            }
        }


        private void AddPart(int partId)
        {
            

        }

        /// <summary>
        /// Updates the subtotal and tax values
        /// </summary>
        private void Refresh()
        {

        }

        /// <summary>
        /// Empties bound properties
        /// </summary>
        private void Clear()
        {
            

        }
    }
}

using eBikeSystem.BLL.Purchasing;
using eBikeSystem.Entities;
using eBikeSystem.ModelViews.Purchasing;
using Microsoft.AspNetCore.Components;
using TakeHomeExercise4WebApp.Components;
using BlazorDialog;
using MudBlazor;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace eBikeWebApp.Components.Pages.eBikePages
{
    public partial class Purchasing : ComponentBase
    {

        #region Properties & Fields

        [Inject]
        private ILogger<Purchasing> Logger { get; set; }

        [Inject]
        protected IDialogService DialogService { get; set; }

        [Inject]
        protected IBlazorDialogService BlazorDialogService { get; set; }

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
        private int qto { get; set; }
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

        /// <summary>
        /// displayes the Phone and City for the selected vendor
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Display de PO details and get the vendor current inventory
        /// </summary>
        /// <param name="vendorId"></param>
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
                            qto = detail.QTO;
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

        
        /// <summary>
        /// Remove an item (detail line) from the current PO details and send it to the vendor inventory
        /// </summary>
        /// <param name="partId"></param>
        private void RemovePurchaseOrderDetail(int partId)
        {
            if (partId > 0)
            {
                //Verify that the part exists
                PurchaseOrderDetailView partToRemove = purchaseOrderDetailsList.FirstOrDefault(p => p.PartID == partId);

                if (partToRemove != null)
                {
                    //Remove part from PO
                    purchaseOrderDetailsList.Remove(partToRemove);

                    //Create the item object
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

                    //Update totals
                    poSubtotal = Math.Round(poSubtotal - (partToRemove.Price * partToRemove.QTO), 2);
                    poGST = Math.Round(poGST - ((partToRemove.Price * partToRemove.QTO) * 0.05m), 2);
                    poTotal = Math.Round((poSubtotal + poGST), 2);

                    Logger.LogInformation($"GST: {(partToRemove.Price * partToRemove.QTO) * 0.05m}");

                    StateHasChanged();
                }

                else
                {
                    errorMessage = "Part not found";
                }   
            }
           
        }


        /// <summary>
        /// add a part fom the vendor inventory to the PO details list
        /// </summary>
        /// <param name="partId"></param>
        private void AddPart(int partId)
        {           

            if (partId > 0)
            {
                //Verify that the part exists
                ItemView partToRemove = ItemsList.FirstOrDefault(i => i.PartID == partId);


                if(partToRemove != null)
                {
                    //Remove the part from the items list
                    ItemsList.Remove(partToRemove);

                    //Create a new PO detail object
                    PurchaseOrderDetailView poDetailToAdd = new PurchaseOrderDetailView
                    {
                        PartID = partToRemove.PartID,
                        Description = partToRemove.Description,
                        QOH = partToRemove.QOH,
                        ROL = partToRemove.ROL,
                        QOO = partToRemove.QOO,
                        QTO = 0,
                        Price = partToRemove.Price,
                        
                    };

                    //Add the new POdetail to the list
                    purchaseOrderDetailsList.Add(poDetailToAdd);                  

                    StateHasChanged();
                }

                else
                {
                    errorMessage = "Part not found";
                }
            }           

        }

        /// <summary>
        /// Updates the subtotal and tax values
        /// </summary>
        private void Refresh(int partId)
        {
            // Find the part that needs to be refreshed
            PurchaseOrderDetailView partToRefresh = purchaseOrderDetailsList.FirstOrDefault(i => i.PartID == partId);

            if (partToRefresh.QTO >= 0)
            {               
               
                // Update totals 
                poSubtotal = Math.Round(purchaseOrderDetailsList.Sum(pod => pod.QTO * pod.Price), 2);
                poGST = Math.Round((poSubtotal * 0.05m), 2);
                poTotal = Math.Round((poSubtotal + poGST), 2);                
             
                StateHasChanged();               
            }
            else
            {
                errorMessage = "Please enter a valid quantity";
            }

        }

        /// <summary>
        /// Empties bound properties
        /// </summary>
        private void Clear()
        {
            

        }
    

        private async Task OnDeleteCurrentPO(int purchaseOrderId)
        {
            string bodyText = "Are you sure you want to delete the current PO? It will be permanently deleted from the DB.";

            string dialogResult =
               await BlazorDialogService.ShowComponentAsDialog<string>(
                   new ComponentAsDialogOptions(typeof(SimpleComponentDialog))
                   {
                       Size = DialogSize.Normal,
                       Parameters = new()
                       {
                            { nameof(SimpleComponentDialog.Input), "Delete PO" },
                            { nameof(SimpleComponentDialog.BodyText), bodyText }
                       }
                   });

            if (dialogResult == "Ok")
            {
                PurchasingServices.DeleteOrder(purchaseOrderId);
                await InvokeAsync(StateHasChanged);
            }            
        }
    }
}

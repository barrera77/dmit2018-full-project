using eBikeSystem.BLL.Purchasing;
using eBikeSystem.Entities;
using eBikeSystem.ModelViews.Purchasing;
using Microsoft.AspNetCore.Components;
using TakeHomeExercise4WebApp.Components;

namespace eBikeWebApp.Components.Pages.eBikePages
{
    public partial class Purchasing
    {

        #region Properties & Fields

        [Inject]
        private NavigationManager _navManager { get; set; }

        [Inject]
        PurchasingServices PurchasingServices { get; set; }

        //Fields
        public List<VendorView> vendorsList { get; set; } = new List<VendorView>();
        public Vendor vendor { get; set; } = new Vendor();

        #endregion

        #region Feedback and Error Messaging

        private string feedbackMessage;
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
                vendorsList = PurchasingServices.getVendors();
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
    }
}

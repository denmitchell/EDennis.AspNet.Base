using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net;
using System.Threading.Tasks;
using M = EDennis.Samples.ColorApp;

namespace EDennis.Samples.ColorApp.Client.Pages.Rgb {
    public partial class CreateBase : ComponentBase {

        [Inject] public RgbApiClient Client { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public M.Rgb Rgb { get; set; } = new M.Rgb();


        [Inject] IJSRuntime JsRuntime { get; set; }

        [Parameter] public string Id { get; set; }
        [Parameter] public bool Editable { get; set; }


        public bool Dirty { get; set; }

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        public string InputClass => Editable ? "form-control" : "form-control-plaintext";



        protected async Task HandleValidSubmit() {
            if (Rgb.Id == default) {
                var addedRecord = await Client.CreateAsync(Rgb);
                if (addedRecord.StatusCode < 300) {
                    StatusClass = "alert-success";
                    Message = "New person added successfully.";
                    Rgb = addedRecord.TypedValue;
                    Dirty = false;
                    Saved = true;
                } else {
                    StatusClass = "alert-danger";
                    Message = "Something went wrong adding the new person.  Please try again.";
                    Saved = false;
                }
            } else {
                var updatedRecord = await Client.UpdateAsync(Rgb.Id.ToString(),Rgb);
                StatusClass = "alert-success";
                Message = "Person updated successfully.";
                Rgb = updatedRecord.TypedValue;
                Dirty = false;
                Saved = true;
            }
        }

        protected void HandleInvalidSubmit() {
            StatusClass = "alert-danger";
            Message = "There are some validation errors.  Please try again.";
        }

        protected async Task DeleteRecord() {
            bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", $"Do you really want to delete the record for {Rgb.Name}?");
            if (confirmed) {
                await Client.DeleteAsync(Rgb.Id.ToString());
                Dirty = false;
                NavigationManager.NavigateTo("/Rgb/Index");
            }
        }

        protected async Task Close() {
            if (Dirty) {
                bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", $"The current record is not saved.  Do you want to discard all changes?");
                if (!confirmed)
                    return;
            }
            NavigationManager.NavigateTo("/Rgb/Index");
        }


    }
}

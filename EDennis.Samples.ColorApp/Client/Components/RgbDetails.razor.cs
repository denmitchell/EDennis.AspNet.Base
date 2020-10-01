using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using M = EDennis.Samples.ColorApp;

namespace EDennis.Samples.ColorApp.Client.Components {
    public partial class RgbDetailsBase : ComponentBase {

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

        protected override async Task OnInitializedAsync() {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("editable", out var strEditable)
                && bool.TryParse(strEditable, out bool booEditable))
                Editable = booEditable;

            int.TryParse(Id, out var id);
            if (id == default)
                Rgb = new Rgb();
            else {
                if (id != default) {
                    var result = await Client.GetByIdAsync(Id);
                    if (result.StatusCode < 299)
                        Rgb = result.TypedValue;
                }
            }
        }

        protected async Task HandleValidSubmit() {
            ObjectResult<M.Rgb> result;
            string verb;

            if (Rgb.Id == default)
                (result, verb) = (await Client.CreateAsync(Rgb), "added");
            else
                (result, verb) = (await Client.UpdateAsync(Rgb.Id.ToString(), Rgb), "updated");

            if (result.StatusCode < 300) {
                StatusClass = "alert-success";
                Message = $"New RGB record {verb} successfully.";
                Rgb = result.TypedValue;
                Dirty = false;
                Saved = true;
            } else {
                StatusClass = "alert-danger";
                Message = $"New Rgb could not be {verb}.  Please try again.";
                Saved = false;
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

using Hr.App.Data.Models;
using Hr.App.Data.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hr.App.Components {
    public partial class AddressListBase: ComponentBase {

        [Parameter] public int PersonId { get; set; }
        [CascadingParameter(Name = "Editable")] public bool Editable { get; set; }
        [Inject] public IAddressService AddressService { get; set; }

        public int AddressId { get; set; }

        public IEnumerable<Address> Addresses { get; set; } = new List<Address>();

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                Addresses = await AddressService.GetForPersonAsync(PersonId);
                StateHasChanged();
            }
        }

        protected async Task PopupClosed(bool _) {
            _showEditPopup = false;
            Addresses = await AddressService.GetForPersonAsync(PersonId);
            StateHasChanged();
        }


        protected bool _showEditPopup;

    }
}

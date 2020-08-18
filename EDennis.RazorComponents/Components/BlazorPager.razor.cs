using Microsoft.AspNetCore.Components;

namespace EDennis.RazorComponents.Components {
    public partial class BlazorPager<TEntity> : ComponentBase 
        where TEntity : class {

        [Parameter]
        public string Url { get; set; }

        [Parameter]
        public EDennis.NetStandard.Base.ISearchablePageableModel<TEntity> Model { get; set; }

        public int OperatorValue(int index)
            => (int)Model.SearchTable[index].Operator;


    }
}

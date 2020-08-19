using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Linq;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// This obviates the need for adding an [Authorize(SomePolicy)] attribute
    /// to the controller and actions
    /// </summary>
    public class DefaultAuthorizationPolicyConvention : IControllerModelConvention, IPageApplicationModelConvention {

        private readonly string _appName;

        public DefaultAuthorizationPolicyConvention(string appName) {
            _appName = appName;
        }

        public void Apply(ControllerModel controller) {

            //don't add Filter if AllowAnonymousFilter is already added
            if (controller.Filters.Any(f => f.GetType() == typeof(AllowAnonymousFilter)))
                return;

            var controllerPath = _appName + '.' + controller.ControllerName;

            foreach (var action in controller.Actions) {
                var actionPath = controllerPath + '.' + action.ActionMethod.Name;
                action.Filters.Add(new AuthorizeFilter(actionPath));
            }
        }

        public void Apply(PageApplicationModel model) {

            //don't add Filter if AllowAnonymousFilter is already added
            if (model.Filters.Any(f => f.GetType() == typeof(AllowAnonymousFilter)))
                return;

            var pagePath = _appName + model.ViewEnginePath.Replace('/','.');
            model.Filters.Add(new AuthorizeFilter(pagePath));
        }


    }
}

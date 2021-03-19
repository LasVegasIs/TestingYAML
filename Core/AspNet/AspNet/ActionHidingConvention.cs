using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Crey.AspNet
{
    // https://joonasw.net/view/hide-actions-from-swagger-openapi-documentation-in-aspnet-core
    public class AttributeActionHidingConvention<T> : IActionModelConvention where T : Attribute
    {
        public void Apply(ActionModel action)
        {
            // NOTE: ideally would hide params which uses in and only in hidden controllers, but that is risky impl, would retain good enough solution
            action.ApiExplorer.IsVisible =
                    !action.Controller.Attributes
                                    .Concat(action.Attributes)
                                    .Any(x => x.GetType().Equals(typeof(T)) || (x is ApiExplorerSettingsAttribute api && api.IgnoreApi));
        }
    }
}
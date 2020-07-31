using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EDennis.NetStandard.Base {
    public class CachedTransactionSwaggerOperationFilter : IOperationFilter {
        public void Apply(OpenApiOperation operation, OperationFilterContext context) {

            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter {
                Name = CachedTransactionOptions.COOKIE_KEY,
                In = ParameterLocation.Cookie,
                Required = false
            });
        }
    }
}

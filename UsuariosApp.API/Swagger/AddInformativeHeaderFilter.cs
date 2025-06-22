using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class AddInformativeHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Só adiciona no método POST
        if (operation == null || !context.ApiDescription.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            return;

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Permissao",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Informativo: 1 = Operador, 2 = Supervisor, 3 = Gerente.<br/><br/>" +
            "Escreva no campo em branco abaixo, o nome da permissão.<br/><br/>" + 
            "No <strong>Edit Value</strong>, no campo "+" <strong>permissao:</strong> " + "passe o <strong>numero correspondente</strong> a permissao",
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
}

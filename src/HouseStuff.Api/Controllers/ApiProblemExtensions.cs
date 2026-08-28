using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

internal static class ApiProblemExtensions
{
    public static ObjectResult ProblemWithCode(
        this ControllerBase controller,
        int statusCode,
        string? title,
        string? code)
    {
        var result = controller.Problem(statusCode: statusCode, title: title);
        if (result.Value is ProblemDetails details)
        {
            details.Extensions["code"] = code;
        }

        return result;
    }
}

using MediatR;
using Microsoft.AspNetCore.Mvc;
using NeoWallet.Domain.Common;

namespace NeoWallet.Api.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected IActionResult HandleResult<T>(Result<T> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return successStatusCode switch
            {
                StatusCodes.Status201Created => StatusCode(StatusCodes.Status201Created, result.Value),
                StatusCodes.Status204NoContent => NoContent(),
                _ => Ok(result.Value)
            };
        }

        return MapErrorToProblemDetails(result.Error);
    }

    protected IActionResult HandleResult(Result result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return successStatusCode switch
            {
                StatusCodes.Status204NoContent => NoContent(),
                _ => Ok()
            };
        }

        return MapErrorToProblemDetails(result.Error);
    }

    private IActionResult MapErrorToProblemDetails(Error error)
    {
        var (statusCode, title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation Error"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource Not Found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict Error"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            ErrorType.Critical => (StatusCodes.Status500InternalServerError, "Critical System Error"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = error.Description,
            Extensions =
            {
                ["errorCode"] = error.Code,
                ["traceId"] = HttpContext.TraceIdentifier
            }
        };

        return StatusCode(statusCode, problemDetails);
    }
}

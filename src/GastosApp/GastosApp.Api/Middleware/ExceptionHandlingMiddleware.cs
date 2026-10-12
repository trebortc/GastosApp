using GastosApp.Domain.Exceptions;
using System.Text.Json;

namespace GastosApp.Api.Middleware;

/// <summary>
/// Middleware de manejo global de excepciones.
/// Convierte excepciones de dominio en respuestas HTTP estructuradas
/// sin exponer detalles internos en produccion.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException nfe      => (StatusCodes.Status404NotFound, nfe.Message),
            ForbiddenException fe      => (StatusCodes.Status403Forbidden, fe.Message),
            DomainException de         => (StatusCodes.Status400BadRequest, de.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "No autenticado."),
            OperationCanceledException  => (StatusCodes.Status499ClientClosedRequest, "Solicitud cancelada."),
            _                          => (StatusCodes.Status500InternalServerError, "Error interno del servidor.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Error no controlado: {Message}", exception.Message);
        else
            _logger.LogWarning("Excepcion de dominio [{Status}]: {Message}", statusCode, exception.Message);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = new ErrorResponse(
            statusCode,
            message,
            _env.IsDevelopment() ? exception.StackTrace : null);

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

public sealed record ErrorResponse(int Status, string Error, string? Detail = null);

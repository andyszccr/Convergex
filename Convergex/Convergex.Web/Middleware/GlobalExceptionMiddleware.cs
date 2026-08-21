using System.Diagnostics;

namespace Convergex.Web.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        var stopwatch =
            Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var userName =
                context.User.Identity?.Name
                ?? "Anónimo";

            var ipAddress =
                context.Connection.RemoteIpAddress?
                    .ToString()
                ?? "Desconocida";

            _logger.LogError(
                ex,
                """
                Excepción no controlada.
                Método: {Method}
                Ruta: {Path}
                Usuario: {UserName}
                IP: {IpAddress}
                Tiempo: {ElapsedMilliseconds} ms
                """,
                context.Request.Method,
                context.Request.Path,
                userName,
                ipAddress,
                stopwatch.ElapsedMilliseconds);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode =
                    StatusCodes
                        .Status500InternalServerError;

                context.Response.ContentType =
                    "text/html; charset=utf-8";

                await context.Response.WriteAsync(
                    """
                    <!DOCTYPE html>
                    <html lang="es">
                    <head>
                        <meta charset="utf-8" />
                        <title>Error - Convergex</title>
                        <style>
                            body {
                                font-family: Arial, sans-serif;
                                background: #f1f5f9;
                                margin: 0;
                                display: grid;
                                min-height: 100vh;
                                place-items: center;
                            }

                            .error-box {
                                background: white;
                                padding: 2rem;
                                border-radius: 14px;
                                box-shadow:
                                    0 8px 24px
                                    rgba(15, 23, 42, 0.08);
                                max-width: 520px;
                                text-align: center;
                            }

                            h1 {
                                color: #1e293b;
                            }

                            p {
                                color: #64748b;
                            }

                            a {
                                display: inline-block;
                                margin-top: 1rem;
                                background: #2563eb;
                                color: white;
                                padding:
                                    0.65rem 1rem;
                                border-radius: 8px;
                                text-decoration: none;
                            }
                        </style>
                    </head>

                    <body>
                        <div class="error-box">
                            <h1>Ocurrió un error inesperado</h1>

                            <p>
                                El incidente fue registrado
                                en los logs del sistema.
                            </p>

                            <a href="/">
                                Volver al inicio
                            </a>
                        </div>
                    </body>
                    </html>
                    """);
            }
        }
        finally
        {
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning(
                    "Request lento detectado: " +
                    "{Method} {Path} tardó " +
                    "{ElapsedMilliseconds} ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}

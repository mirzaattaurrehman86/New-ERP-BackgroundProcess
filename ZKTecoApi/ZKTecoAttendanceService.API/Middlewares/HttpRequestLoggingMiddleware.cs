//using System.Diagnostics;
//using System.Text;
//using System.Text.Json;

//namespace ZKTecoAttendanceService.API.Middlewares
//{
//    /// <summary>
//    /// Middleware for logging HTTP requests and responses with complete details.
//    /// Captures:
//    /// - Request method, path, query string, body
//    /// - Response status, body, execution time
//    /// - User information (if authenticated)
//    /// - IP address, user agent
//    /// - Error messages (if applicable)
//    /// </summary>
//    public class HttpRequestLoggingMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly ILogger<HttpRequestLoggingMiddleware> _logger;

//        public HttpRequestLoggingMiddleware(RequestDelegate next, ILogger<HttpRequestLoggingMiddleware> logger)
//        {
//            _next = next;
//            _logger = logger;
//        }

//        public async Task InvokeAsync(HttpContext context, ApplicationDbContext applicationDbContext)
//        {
//            var stopwatch = Stopwatch.StartNew();

//            // Read request body
//            context.Request.EnableBuffering();
//            var requestBody = await ReadRequestBodyAsync(context.Request);
//            context.Request.Body.Position = 0;

//            // Log incoming request
//            var request = context.Request;
//            var fullUrl = $"{request.Path}{request.QueryString}";

//            _logger.LogInformation("HTTP {Method} {Url} started | QueryString: {QueryString}", request.Method, fullUrl, request.QueryString);

//            // Capture original response body stream
//            var originalBodyStream = context.Response.Body;
//            var responseBody = new MemoryStream();
//            context.Response.Body = responseBody;

//            try
//            {
//                await _next(context);

//                stopwatch.Stop();

//                // Log response
//                _logger.LogInformation("HTTP {Method} {Path} completed | StatusCode: {StatusCode} | Duration: {Duration}ms", request.Method, fullUrl, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);

//                // Read response body
//                responseBody.Seek(0, SeekOrigin.Begin);
//                var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
//                responseBody.Seek(0, SeekOrigin.Begin);

//                // Copy response to original stream
//                await responseBody.CopyToAsync(originalBodyStream);

//                // Log to database with complete request details including query string
//                await LogToDatabase(context, requestBody, responseBodyText, (int)stopwatch.ElapsedMilliseconds, null);
//            }
//            catch (Exception ex)
//            {
//                stopwatch.Stop();

//                var fullUrlError = $"{request.Path}{request.QueryString}";

//                _logger.LogError(ex, "HTTP {Method} {Url} failed after {Duration}ms", request.Method, fullUrlError, stopwatch.ElapsedMilliseconds);

//                // Log exception to database
//                await LogToDatabase(context, requestBody, null, (int)stopwatch.ElapsedMilliseconds, ex.Message);

//                throw;
//            }
//            finally
//            {
//                context.Response.Body = originalBodyStream;
//            }
//        }

//        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
//        {
//            // Skip logging body for GET/HEAD requests (no body)
//            if (request.Method == "GET" || request.Method == "HEAD")
//                return string.Empty;

//            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
//            var body = await reader.ReadToEndAsync();
//            request.Body.Position = 0;
//            return body;
//        }

//        private async Task LogToDatabase(HttpContext context, string requestBody, string? responseBody, int executionTime, string? errorMessage)
//        {
//            try
//            {
//                // Resolve a new DbContext from DI
//                var scopeFactory = context.RequestServices.GetRequiredService<IServiceScopeFactory>();
//                using var scope = scopeFactory.CreateScope(); // creates a new DI scope
//                var logDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//                //DbContextOptions<ApplicationDbContext> options = new DbContextOptions<ApplicationDbContext>();
//                //using (var dbLogContext = new ApplicationDbContext(options))
//                //{
//                // Get user ID and email from claims
//                int? userId = null;
//                string? userEmail = null;

//                if (context.User.Identity?.IsAuthenticated == true)
//                {
//                    var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
//                    if (int.TryParse(userIdClaim, out var parsedUserId))
//                    {
//                        userId = parsedUserId;
//                    }

//                    userEmail = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
//                }

//                // Mask sensitive data
//                var maskedRequestBody = MaskSensitiveData(requestBody);
//                var maskedResponseBody = responseBody != null ? MaskSensitiveData(responseBody) : null;

//                var queryString = context.Request.QueryString.HasValue
//                    ? context.Request.QueryString.Value
//                    : null;

//                var auditLog = new AuditLog
//                {
//                    UserId = userId,
//                    UserEmail = userEmail,
//                    Action = $"{context.Request.Method} {context.Request.Path}",
//                    HttpMethod = context.Request.Method,
//                    Endpoint = context.Request.Path.Value,
//                    QueryString = queryString,
//                    RequestBody = maskedRequestBody,
//                    ResponseStatus = context.Response.StatusCode,
//                    ResponseBody = maskedResponseBody,
//                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
//                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
//                    ExecutionTimeMs = executionTime,
//                    IsSuccess = context.Response.StatusCode < 400,
//                    ErrorMessage = errorMessage,
//                    CreatedAt = DateTime.Now
//                };

//                //dbContext.AuditLogs.Add(auditLog);
//                //await dbContext.SaveChangesAsync();

//                logDbContext.AuditLogs.Add(auditLog);
//                await logDbContext.SaveChangesAsync();

//                //}
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to log audit trail to database");
//            }
//        }

//        private string MaskSensitiveData(string content)
//        {
//            if (string.IsNullOrEmpty(content))
//                return content;

//            try
//            {
//                // Try to validate if it's JSON
//                JsonDocument.Parse(content);

//                var sensitiveFields = new[]
//                {
//                "password",
//                "currentPassword",
//                "newPassword",
//                "confirmPassword",
//                "newPasswordConfirm",
//                "token",
//                "refreshToken",
//                "accessToken",
//                "jti",
//                "sub",
//                "secret",
//                "apiKey",
//                "apiSecret",
//                "authorizationCode"
//            };

//                foreach (var field in sensitiveFields)
//                {
//                    if (content.Contains($"\"{field}\"", StringComparison.OrdinalIgnoreCase))
//                    {
//                        content = System.Text.RegularExpressions.Regex.Replace(
//                            content,
//                            $"\"{field}\"\\s*:\\s*\"[^\"]*\"",
//                            $"\"{field}\":\"***MASKED***\"",
//                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
//                    }
//                }

//                return content;
//            }
//            catch
//            {
//                return content;
//            }
//        }
//    }

//}

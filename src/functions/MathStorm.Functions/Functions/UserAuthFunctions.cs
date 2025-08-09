namespace MathStorm.Functions.Functions;

public class UserAuthFunctions
{
    private readonly ILogger _logger;
    private readonly ICosmosDbService _cosmosDbService;

    public UserAuthFunctions(ILoggerFactory loggerFactory, ICosmosDbService cosmosDbService)
    {
        _logger = loggerFactory.CreateLogger<UserAuthFunctions>();
        _cosmosDbService = cosmosDbService;
    }

    [Function("AuthenticateUser")]
    [OpenApiOperation(operationId: "AuthenticateUser", tags: new[] { "User" }, Summary = "Authenticate user", Description = "Authenticates a user with username and optional PIN, creating a new user if they don't exist.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UserAuthRequestDto), Required = true, Description = "User authentication request with username and optional PIN")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UserAuthResponseDto), Summary = "Authentication result", Description = "Returns authentication status and user information.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Summary = "Bad request", Description = "Invalid request body or missing required fields.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "text/plain", bodyType: typeof(string), Summary = "Internal server error", Description = "An error occurred while processing the authentication request.")]
    public async Task<HttpResponseData> AuthenticateUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/auth")] HttpRequestData req)
    {
        _logger.LogInformation("AuthenticateUser function triggered.");

        try
        {
            // Parse request body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<UserAuthRequestDto>(body);

            if (request == null || string.IsNullOrWhiteSpace(request.Username))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body or missing username");
                return badRequest;
            }

            // Validate username format
            if (request.Username.Length < 2 || request.Username.Length > 20)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Username must be between 2 and 20 characters");
                return badRequest;
            }

            // Check if user exists
            var existingUser = await _cosmosDbService.GetUserByUsernameAsync(request.Username);
            var response = new UserAuthResponseDto
            {
                Username = request.Username
            };

            if (existingUser == null)
            {
                // Create new user
                var newUser = await _cosmosDbService.CreateUserAsync(request.Username, request.Pin);
                response.IsAuthenticated = true;
                response.IsNewUser = true;
                response.UserId = newUser.Id;
            }
            else
            {
                // Validate existing user
                var isValid = await _cosmosDbService.ValidateUserAsync(request.Username, request.Pin);
                if (isValid)
                {
                    response.IsAuthenticated = true;
                    response.IsNewUser = false;
                    response.UserId = existingUser.Id;
                }
                else
                {
                    response.IsAuthenticated = false;
                    response.IsNewUser = false;
                    response.ErrorMessage = "Invalid PIN for existing user";
                }
            }

            var httpResponse = req.CreateResponse(HttpStatusCode.OK);
            httpResponse.Headers.Add("Content-Type", "application/json");

            var jsonResponse = JsonConvert.SerializeObject(response);
            await httpResponse.WriteStringAsync(jsonResponse);
            return httpResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AuthenticateUser function");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }
}
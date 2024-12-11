using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services;

public class GoogleMeetService : IGoogleMeetService
{
    private static string[] Scopes = { CalendarService.Scope.Calendar };
    private static string ApplicationName = "StartedIn";

    private readonly IConfiguration _configuration;

    public GoogleMeetService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public CalendarService GetCalendarService()
    {
        UserCredential credential;

        // Get client ID and secret from user secrets
        var clientId = _configuration["GOOGLE_CLIENT_ID"];
        var clientSecret = _configuration["GOOGLE_CLIENT_SECRET"];

        // Create the secrets object
        var secrets = new ClientSecrets
        {
            ClientId = clientId,
            ClientSecret = clientSecret
        };

        // Use the secrets for authorization
        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets,
            Scopes,
            "user",
            CancellationToken.None,
            new FileDataStore("token.json", true)).Result;

        return new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }
}
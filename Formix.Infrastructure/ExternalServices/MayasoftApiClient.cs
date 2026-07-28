using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Formix.Domain.Dtos.Mayasoft;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Formix.Infrastructure.ExternalServices;

// Cliente que se autentica contra Salesforce (OAuth2 client credentials) y consulta MayasoftAPI
public class MayasoftApiClient : IMayasoftApiClient
{
    private readonly HttpClient _http;
    private readonly MayasoftApiOptions _options;
    private readonly ILogger<MayasoftApiClient> _logger;

    // Configura la deserialización para que los campos snake_case del JSON coincidan con las propiedades PascalCase de los DTOs
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public MayasoftApiClient(HttpClient http, IOptions<MayasoftApiOptions> options, ILogger<MayasoftApiClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    // Pide un access_token nuevo al endpoint OAuth2 de Salesforce usando client credentials
    private async Task<string> ObtenerTokenAsync(CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl)
        {
            Content = new FormUrlEncodedContent(form)
        };

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<MayasoftAuthResponseDto>(cancellationToken: ct);
        return auth?.access_token ?? throw new InvalidOperationException("No se obtuvo access_token de Mayasoft.");
    }

    // Obtiene el listado completo de trámites activos desde MayasoftAPI
    public async Task<List<TramiteMayasoftResponseDto>> ObtenerTramitesAsync(CancellationToken ct = default)
    {
        var token = await ObtenerTokenAsync(ct);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.BaseUrl}{_options.Endpoint}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Error al consultar MayasoftAPI: {Status} - {Body}", response.StatusCode, body);
            throw new HttpRequestException($"MayasoftAPI respondió {response.StatusCode}: {body}");
        }

        var tramites = await response.Content.ReadFromJsonAsync<List<TramiteMayasoftResponseDto>>(JsonOptions, ct);
        return tramites ?? new List<TramiteMayasoftResponseDto>();
    }
}
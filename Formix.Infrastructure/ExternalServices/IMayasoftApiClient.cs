using Formix.Domain.Dtos.Mayasoft;

namespace Formix.Infrastructure.ExternalServices;

// Contrato del cliente que se autentica y consulta MayasoftAPI
public interface IMayasoftApiClient
{
    Task<List<TramiteMayasoftResponseDto>> ObtenerTramitesAsync(CancellationToken ct = default);
}

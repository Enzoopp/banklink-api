using System.Threading;
using System.Threading.Tasks;
using BankLink.Api.Domain; // ExternalBank (entidad)
using BankLink.Api.Dtos;   // ExternalTransferRequest/Response

namespace BankLink.Api.Interfaces
{
    public interface IBancoExternoService
    {
        Task<ExternalTransferResponse> EnviarTransferenciaAsync(
            ExternalBank bank,
            ExternalTransferRequest payload,
            CancellationToken ct = default);

        bool ValidarFirma(string firma, string cuerpoPlano);
    }
}

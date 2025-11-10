using System.Threading.Tasks;
using BankLink.Api.Dtos;

namespace BankLink.Api.Interfaces
{
    /// <summary>
    /// Coordina la lógica de transferencias: débito/crédito + invocación a banco externo (outbound)
    /// y acreditación por webhook (inbound). Debe usar transacciones de EF.
    /// </summary>
    public interface ITransferenciaService
    {
        /// <returns>MovementResponseDto de la transferencia enviada (debitada) si todo salió OK.</returns>
        Task<MovementResponseDto> EnviarAsync(TransferSendDto dto);

        /// <returns>MovementResponseDto de la transferencia recibida (acreditada).</returns>
        Task<MovementResponseDto> RecibirAsync(TransferReceiveDto dto, string? firma = null);
    }
}

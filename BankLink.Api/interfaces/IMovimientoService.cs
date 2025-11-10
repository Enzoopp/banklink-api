using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankLink.Api.Dtos;
using BankLink.Api.Domain;
namespace BankLink.Api.Interfaces
{
    public interface IMovimientoService
    {
        Task<MovementResponseDto> RegistrarAsync(int accountId, MovementType tipo, decimal monto, string? descripcion = null);
        Task<IEnumerable<MovementResponseDto>> ListarPorCuentaAsync(int accountId, DateTime? desde = null, DateTime? hasta = null);
    }
}

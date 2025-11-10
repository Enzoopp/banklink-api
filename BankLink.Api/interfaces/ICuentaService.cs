using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankLink.Api.Dtos;

namespace BankLink.Api.Interfaces
{
    public interface ICuentaService
    {
        Task<AccountResponseDto> CrearAsync(AccountCreateDto dto);
        Task<AccountResponseDto?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<AccountResponseDto>> ListarPorClienteAsync(int clientId);
        Task<bool> ActivarAsync(int id);
        Task<bool> DesactivarAsync(int id);

        // Operaciones de caja (registran Movement internamente y usan transacción)
        Task<decimal> DepositarAsync(int accountId, decimal monto, string? descripcion = null);
        Task<decimal> RetirarAsync(int accountId, decimal monto, string? descripcion = null);

        Task<IEnumerable<MovementResponseDto>> ListarMovimientosAsync(int accountId, DateTime? desde = null, DateTime? hasta = null);
    }
}

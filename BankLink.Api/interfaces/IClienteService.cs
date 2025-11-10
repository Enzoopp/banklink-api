using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankLink.Api.Dtos;

namespace BankLink.Api.Interfaces
{
    public interface IClienteService
    {
        Task<ClientResponseDto> CrearAsync(ClientCreateDto dto);
        Task<ClientResponseDto?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<ClientResponseDto>> ListarAsync(string? filtro = null);
        Task ActualizarAsync(int id, ClientUpdateDto dto);
        Task EliminarAsync(int id);
    }
}

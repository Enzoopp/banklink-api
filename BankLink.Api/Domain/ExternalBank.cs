namespace BankLink.Api.Domain;

public class ExternalBank
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string TransferEndpoint { get; set; } = string.Empty;
    public string? ValidationEndpoint { get; set; } // NUEVO: endpoint para validar cuentas
    public string ApiKey { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true; // NUEVO: para activar/desactivar bancos
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // NUEVO: fecha de registro
}

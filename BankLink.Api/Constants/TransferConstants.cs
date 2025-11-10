namespace BankLink.Api.Constants;

public static class TransferConstants
{
    public const string BankLinkCode = "BANKLINK";
    public const string ApiKeyHeader = "X-Api-Key";
    public const string IdempotencyKeyHeader = "Idempotency-Key";
    
    public const decimal MaxTransferAmount = 1000000m;
    public const int HttpTimeoutSeconds = 30;
    
    public static class Messages
    {
        public const string AccountNotFoundOrInactive = "Cuenta origen inexistente o inactiva";
        public const string InsufficientBalance = "Saldo insuficiente";
        public const string DestinationBankNotRegistered = "Banco destino no registrado";
        public const string BadlyConfiguredBank = "Banco destino mal configurado (URL/endpoint)";
        public const string MissingApiKey = "Banco destino sin API key";
        public const string TransferRejected = "Banco externo rechazó la transferencia";
        public const string NetworkError = "Error de red al contactar banco externo";
        public const string OriginBankNotRegistered = "Banco origen no registrado";
        public const string InvalidApiKey = "API key inválida";
        public const string DestinationAccountNotFound = "Cuenta destino inexistente o inactiva";
        public const string TransferSent = "Transferencia Enviada";
        public const string TransferReceived = "Transferencia Recibida";
    }
}
using FluentValidation;
using BankLink.Api.Dtos;

namespace BankLink.Api.Validators;

public class TransferReceiveDtoValidator : AbstractValidator<TransferReceiveDto>
{
    public TransferReceiveDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto debe ser mayor a cero")
            .LessThanOrEqualTo(1000000)
            .WithMessage("El monto excede el límite permitido");

        RuleFor(x => x.OriginBankCode)
            .NotEmpty()
            .WithMessage("Código de banco origen requerido")
            .MaximumLength(30);

        RuleFor(x => x.OriginAccountNumber)
            .NotEmpty()
            .WithMessage("Número de cuenta origen requerido")
            .MaximumLength(20);

        RuleFor(x => x.DestinationAccountNumber)
            .NotEmpty()
            .WithMessage("Número de cuenta destino requerido")
            .MaximumLength(20);

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Clave de idempotencia requerida")
            .MaximumLength(100);

        RuleFor(x => x.Concept)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Concept));
    }
}
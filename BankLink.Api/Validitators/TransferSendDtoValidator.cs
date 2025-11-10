using FluentValidation;
using BankLink.Api.Dtos;

namespace BankLink.Api.Validators;

public class TransferSendDtoValidator : AbstractValidator<TransferSendDto>
{
    public TransferSendDtoValidator()
    {
        RuleFor(x => x.OriginAccountId)
            .GreaterThan(0)
            .WithMessage("ID de cuenta origen inválido");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto debe ser mayor a cero")
            .LessThanOrEqualTo(1000000)
            .WithMessage("El monto excede el límite permitido");

        RuleFor(x => x.DestinationBankCode)
            .NotEmpty()
            .WithMessage("Código de banco destino requerido")
            .MaximumLength(30)
            .WithMessage("Código de banco demasiado largo");

        RuleFor(x => x.DestinationAccountNumber)
            .NotEmpty()
            .WithMessage("Número de cuenta destino requerido")
            .MaximumLength(20)
            .WithMessage("Número de cuenta demasiado largo");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Clave de idempotencia requerida")
            .MaximumLength(100)
            .WithMessage("Clave de idempotencia demasiado larga");

        RuleFor(x => x.Concept)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Concept))
            .WithMessage("Concepto demasiado largo");
    }
}
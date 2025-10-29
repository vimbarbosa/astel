using ASTEL.Api.DTOs;
using FluentValidation;

namespace ASTEL.Api.Validators
{
    public class DadosCadastraisValidator : AbstractValidator<DadosCadastraisDTO>
    {
        public DadosCadastraisValidator()
        {
            // 🔹 Validação de chaves primárias obrigatórias
            RuleFor(x => x.MatriculaSistel)
                .NotNull().WithMessage("O campo MatriculaSistel é obrigatório.")
                .GreaterThan(0).WithMessage("O campo MatriculaSistel deve ser maior que zero.");

            RuleFor(x => x.MatriculaAstel)
                .NotNull().WithMessage("O campo MatriculaAstel é obrigatório.")
                .GreaterThan(0).WithMessage("O campo MatriculaAstel deve ser maior que zero.");

            // 🔹 Nome obrigatório
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O campo Nome é obrigatório.")
                .MaximumLength(120).WithMessage("O campo Nome deve ter no máximo 120 caracteres.");

            // 🔹 Campos opcionais, mas com limite se preenchidos
            RuleFor(x => x.Endereco)
                .MaximumLength(255).WithMessage("O campo Endereço deve ter no máximo 255 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Endereco));

            RuleFor(x => x.EstadoCivil)
                .MaximumLength(50).WithMessage("O campo Estado Civil deve ter no máximo 50 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.EstadoCivil));

            RuleFor(x => x.Telefone)
                .MaximumLength(20).WithMessage("O campo Telefone deve ter no máximo 20 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.Telefone));

            RuleFor(x => x.NomeEsposa)
                .MaximumLength(120).WithMessage("O campo Nome da Esposa deve ter no máximo 120 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.NomeEsposa));

            RuleFor(x => x.CPF)
                .MaximumLength(14).WithMessage("O campo CPF deve ter no máximo 14 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.CPF));

            RuleFor(x => x.RG)
                .MaximumLength(20).WithMessage("O campo RG deve ter no máximo 20 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.RG));

            // 🔹 Validação numérica opcional e tolerante
            RuleFor(x => x.Situacao)
                .InclusiveBetween(0, 9999)
                .When(x => x.Situacao.HasValue)
                .WithMessage("O campo Situação deve estar entre 0 e 9999.");

            RuleFor(x => x.ValorBeneficio)
                .GreaterThanOrEqualTo(0)
                .When(x => x.ValorBeneficio.HasValue)
                .WithMessage("O campo Valor Benefício deve ser positivo.");

            // 🔹 Booleanos são opcionais, então sem regra obrigatória
        }
    }
}

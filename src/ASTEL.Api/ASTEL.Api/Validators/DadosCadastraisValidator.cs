using ASTEL.Api.DTOs;
using FluentValidation;

namespace ASTEL.Api.Validators
{
    public class DadosCadastraisValidator : AbstractValidator<DadosCadastraisDTO>
    {
        public DadosCadastraisValidator()
        {
            // 🔹 ID deve ser válido apenas para update (POST ignora)
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .When(x => x.Id != 0)
                .WithMessage("O campo Id deve ser maior que zero.");

            // 🔹 Matrícula SISTEL (opcional, mas se enviada deve ser válida)
            RuleFor(x => x.MatriculaSistel)
                .GreaterThan(0)
                .When(x => x.MatriculaSistel.HasValue)
                .WithMessage("O campo MatriculaSistel deve ser maior que zero quando informado.");

            // 🔹 Matrícula ASTEL (opcional, mas se enviada deve ser válida)
            RuleFor(x => x.MatriculaAstel)
                .GreaterThan(0)
                .When(x => x.MatriculaAstel.HasValue)
                .WithMessage("O campo MatriculaAstel deve ser maior que zero quando informado.");

            // 🔹 Nome obrigatório
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O campo Nome é obrigatório.")
                .MaximumLength(255).WithMessage("O campo Nome deve ter no máximo 255 caracteres.");

            // 🔹 Endereco
            RuleFor(x => x.Endereco)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Endereco))
                .WithMessage("O campo Endereco deve ter no máximo 500 caracteres.");

            // 🔹 Estado civil
            RuleFor(x => x.EstadoCivil)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.EstadoCivil))
                .WithMessage("O campo EstadoCivil deve ter no máximo 50 caracteres.");

            // 🔹 Telefone
            RuleFor(x => x.Telefone)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.Telefone))
                .WithMessage("O campo Telefone deve ter no máximo 50 caracteres.");

            // 🔹 NomeEsposa
            RuleFor(x => x.NomeEsposa)
                .MaximumLength(255)
                .When(x => !string.IsNullOrWhiteSpace(x.NomeEsposa))
                .WithMessage("O campo NomeEsposa deve ter no máximo 255 caracteres.");

            // 🔹 CPF
            RuleFor(x => x.CPF)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.CPF))
                .WithMessage("O campo CPF deve ter no máximo 50 caracteres.");

            // 🔹 RG
            RuleFor(x => x.RG)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.RG))
                .WithMessage("O campo RG deve ter no máximo 50 caracteres.");

            // 🔹 Situacao é string agora — apenas tamanho
            RuleFor(x => x.Situacao)
                .MaximumLength(255)
                .When(x => !string.IsNullOrWhiteSpace(x.Situacao))
                .WithMessage("O campo Situacao deve ter no máximo 255 caracteres.");

            // 🔹 ValorBeneficio
            RuleFor(x => x.ValorBeneficio)
                .GreaterThanOrEqualTo(0)
                .When(x => x.ValorBeneficio.HasValue)
                .WithMessage("O campo ValorBeneficio deve ser positivo.");
        }
    }
}

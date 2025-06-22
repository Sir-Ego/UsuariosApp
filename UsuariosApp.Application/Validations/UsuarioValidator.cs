using FluentValidation;
using UsuariosApp.Application.Dtos;

namespace UsuariosApp.Application.Validations
{
    public class UsuarioValidator : AbstractValidator<UsuarioRequestDto>
    {
        public UsuarioValidator()
        {
            RuleFor(u => u.Nome)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .Length(3, 100).WithMessage("O nome deve ter entre 3 e 100 caracteres.")
                .Matches(@"^[A-Za-zÀ-ÿ\s]+$")
                .WithMessage("O nome deve conter apenas letras.");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("O email é obrigatório.")
                .EmailAddress().WithMessage("Formato de email inválido.")
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("O domínio do email está incompleto.");


            RuleFor(u => u.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
                .WithMessage("A senha deve ter pelo menos 8 caracteres, " +
                "deve possuir ao menos uma letra Maiúscula, " +
                "uma letra minúscula " +
                "e um caracter especial.");

            RuleFor(u => u.Permissao)
               .IsInEnum()
               .WithMessage("Permissão inválida. Valores válidos: Operador, Supervisor, Gerente.");
        }
    }
}

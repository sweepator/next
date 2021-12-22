using FluentValidation;
using Next.HomeBanking.Application.Commands;
using Next.Validation.Fluent;

namespace Next.HomeBanking.Infrastructure.FluentValidation
{
    public class CreateBankAccountCommandValidator : FluentValidator<CreateAccountCommand>
    {
        public CreateBankAccountCommandValidator()
        {
            RuleFor(o => o.Owner)
                .NotEmpty()
                .WithErrorCode("InvalidOwner");
            RuleFor(o => o.Iban)
                .NotEmpty()
                .WithErrorCode("InvalidIban");
        }
    }
}
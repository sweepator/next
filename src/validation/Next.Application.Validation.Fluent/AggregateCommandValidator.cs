using FluentValidation;
using Next.Cqrs.Commands;
using Next.Validation.Fluent;

namespace Next.Application.Validation.Fluent
{
    public class AggregateCommandValidator : FluentValidator<IAggregateCommand>
    {
        public AggregateCommandValidator()
        {
            RuleFor(o => o.Id)
                .NotNull()
                .WithErrorCode("InvalidEntityId")
                .WithMessage("Invalid aggregate identifier.");
            
            RuleFor(o => o.Id.Value)
                .NotEmpty()
                .WithErrorCode("InvalidEntityId")
                .WithMessage("Invalid aggregate identifier.");
        }
    }
}
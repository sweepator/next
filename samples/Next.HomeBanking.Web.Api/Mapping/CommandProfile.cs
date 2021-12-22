using AutoMapper;
using Next.Abstractions.Domain;
using Next.Application.Contracts;
using Next.HomeBanking.Application.Commands;
using Next.HomeBanking.Domain.Aggregates;
using Next.HomeBanking.Domain.Events;

namespace Next.HomeBanking.Web.Api.Mapping
{
    public class CommandProfile : Profile
    {
        public CommandProfile()
        {
            CreateMap<Notification<DomainEvent<BankAccountAggregate, BankAccountId, BankAccountCreated>>, ProcessAccountCommand>()
                .ConvertUsing(o => o.ToProcessProductCommand());
        }
    }
}
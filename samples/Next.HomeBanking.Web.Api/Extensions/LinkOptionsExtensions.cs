using Next.Cqrs.Commands;
using Next.Cqrs.Queries;
using Next.HomeBanking.Application.Commands;
using Next.HomeBanking.Application.Queries;
using Next.HomeBanking.Domain.Aggregates;
using Next.Web.Application.Extensions;
using Next.Web.Hypermedia;

namespace Next.HomeBanking.Web.Api.Extensions
{
    public static class LinkOptionsExtensions
    {
        public static void ConfigureHypermedia(this LinksOptions linksOptions)
        {
            linksOptions.ConfigureGetAccountDetails();
            linksOptions.ConfigureCreateAccount();
            linksOptions.ConfigureGetAccounts();
            linksOptions.ConfigureDeposit();
            linksOptions.ConfigureDebit();
            linksOptions.ConfigureTransfer();
            linksOptions.ConfigureGetAccountTransactions();
        }

        private static void ConfigureCreateAccount(this LinksOptions linksOptions)
        {
            linksOptions.AddPolicy<CreateAccountCommandResponse>(
                RouteNames.CreateAccount,
                b =>
                    b
                        .RequireRoutedLink(
                            "details",
                            RouteNames.GetAccountDetails,
                            (_, context) =>
                                new
                                {
                                    id = context.GetAggregateRoot<BankAccountAggregate>().Id.Value
                                }
                        ));
        }

        private static void ConfigureGetAccountDetails(this LinksOptions linksOptions)
        {
            linksOptions.AddPolicy<BankAccountDetailsProjection>(
                b =>
                    b.RequireRoutedLink(
                            "enable",
                            RouteNames.EnableAccount,
                            (response, _) =>
                                new
                                {
                                    id = response.Id
                                },
                            (response, _) =>
                                new
                                {
                                    enable = !response.Enabled
                                }
                        )
                        .RequireRoutedLink(
                            "details",
                            RouteNames.GetAccountDetails,
                            (response, _) =>
                                new
                                {
                                    id = response.Id
                                }
                        )
                        .RequireRoutedLink(
                            "transactions",
                            RouteNames.GetAccountTransactions,
                            (response, _) =>
                                new
                                {
                                    id = response.Id
                                }
                        )
                        .RequireRoutedLink(
                            "deposit",
                            RouteNames.Deposit,
                            (response, _) =>
                                new
                                {
                                    id = response.Id
                                },
                            (_, _) => new
                            {
                                value = 0
                            },
                            cb => cb.Assert(
                                (response, _) => response.Enabled)
                        )
                        .RequireRoutedLink(
                            "debit",
                            RouteNames.Debit,
                            (response, _) =>
                                new
                                {
                                    id = response.Id
                                },
                            (_, _) => new
                            {
                                value = 0
                            },
                            cb => cb.Assert(
                                (response, _) => response.Enabled && response.Balance > 0)
                        )
                        .RequireRoutedLink(
                            "transfer",
                            RouteNames.Transfer,
                            (response, _) =>
                                new
                                {
                                    id = response.Id
                                },
                            (_, _) => new
                            {
                                value = 0.0d,
                                to = "{{to}}"
                            },
                            cb => cb.Assert(
                                (response, _) => response.Enabled && response.Balance > 0)
                        )
                        .RequireRoutedLink(
                            "cancel",
                            RouteNames.CancelAccount,
                            (response, _) =>
                                new
                                {
                                    id = response.Id
                                }));
        }

        private static void ConfigureGetAccounts(this LinksOptions linksOptions)
        {
            linksOptions.AddPolicy<QueryResponse<BankAccountProjection>>(
                RouteNames.GetAccounts,
                b =>
                    b.RequireRoutedLink(
                        "create",
                        RouteNames.CreateAccount,
                        getParameters: (_, _) =>
                            new
                            {
                                owner = "{{owner}}",
                                iban = "{{iban}}"
                            }));

            linksOptions.AddPolicy<BankAccountProjection>(
                b =>
                    b.RequireRoutedLink(
                        "details",
                        RouteNames.GetAccountDetails,
                        (response, _) =>
                            new
                            {
                                id = response.Id
                            }));
        }

        private static void ConfigureGetAccountTransactions(this LinksOptions linksOptions)
        {
            linksOptions.AddPolicy<BankAccountTransactionProjection>(
                b =>
                    b.RequireRoutedLink(
                        "cancel-transaction",
                        RouteNames.CancelTransaction,
                        getValues: (o, _) =>
                            new
                            {
                                id = o.AccountId,
                                transactionId = o.Id
                            }));
        }

        private static void ConfigureDeposit(this LinksOptions linksOptions)
        {
            linksOptions.AddPolicy<DepositCommandResponse>(
                b =>
                    b.RequireRoutedLink(
                            "details",
                            RouteNames.GetAccountDetails,
                            (_, context) =>
                                new
                                {
                                    id = context.GetAggregateRoot<BankAccountAggregate>().Id.Value
                                }
                        )
                        .RequireRoutedLink(
                            "transactions",
                            RouteNames.GetAccountTransactions,
                            (_, context) =>
                                new
                                {
                                    id = context.GetAggregateRoot<BankAccountAggregate>().Id.Value
                                }
                        )
                        .RequireRoutedLink(
                            "cancel-transaction",
                            RouteNames.CancelTransaction,
                            (response, context) =>
                                new
                                {
                                    id = context.GetAggregateRoot<BankAccountAggregate>().Id.Value,
                                    transactionId = response.TransactionId.Value
                                }
                        ));
        }

        private static void ConfigureDebit(this LinksOptions linksOptions)
        {
            linksOptions.AddPolicy<DebitCommandResponse>(
                b =>
                    b.RequireRoutedLink(
                            "details",
                            RouteNames.GetAccountDetails,
                            (_, context) =>
                                new
                                {
                                    id = context.GetAggregateRoot<BankAccountAggregate>().Id.Value
                                }
                        )
                        .RequireRoutedLink(
                            "transactions",
                            RouteNames.GetAccountTransactions,
                            (_, context) =>
                                new
                                {
                                    id = context.GetAggregateRoot<BankAccountAggregate>().Id.Value
                                }
                        )
                        .RequireRoutedLink(
                            "cancel-transaction",
                            RouteNames.CancelTransaction,
                            (response, context) =>
                                new
                                {
                                    id = context.GetAggregateRoot<BankAccountAggregate>().Id.Value,
                                    transactionId = response.TransactionId.Value
                                }
                        ));
        }

        private static void ConfigureTransfer(this LinksOptions linksOptions)
        {
            linksOptions.AddPolicy<TransferCommandResponse>(
                b =>
                    b.RequireRoutedLink(
                            "details",
                            RouteNames.GetAccountDetails,
                            (_, context) =>
                                new
                                {
                                    id = context.GetAggregateRoot<BankAccountAggregate>().Id.Value
                                }
                        )
                        .RequireRoutedLink(
                            "transactions",
                            RouteNames.GetAccountTransactions,
                            (_, context) =>
                                new
                                {
                                    id = context.GetAggregateRoot<BankAccountAggregate>().Id.Value
                                }
                        )
                        .RequireRoutedLink(
                            "cancel-transaction",
                            RouteNames.CancelTransaction,
                            (response, context) =>
                                new
                                {
                                    id = context.GetAggregateRoot<BankAccountAggregate>().Id.Value,
                                    transactionId = response.TransactionId.Value
                                }
                        ));
        }
    }
}
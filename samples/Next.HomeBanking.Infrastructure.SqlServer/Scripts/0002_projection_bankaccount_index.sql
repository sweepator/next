CREATE TABLE [dbo].[Projection-BankAccountIndex]
(
	[id] [nvarchar](256) not null,
    [iban] [nvarchar](256) not null,
    [version] [int] not null
 )

CREATE UNIQUE NONCLUSTERED INDEX [ix_projection_bankaccount_index_iban] ON [dbo].[Projection-BankAccountIndex]
(
    [iban] ASC
)	
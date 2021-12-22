CREATE TABLE [dbo].[Projection-BankAccount]
(
	[id] [nvarchar](256) not null,
    [owner] [nvarchar](256) not null,
    [iban] [nvarchar](256) not null,
    [balance] decimal not null,
    [enabled] [bit] not null,
    [version] [bigint] not null,
    createdate [datetime2] NOT NULL,
    updatedate [datetime2] NOT NULL,
    constraint pk_projection_bankaccount primary key clustered([id] asc)
)

CREATE UNIQUE NONCLUSTERED INDEX [ix_projection_bankaccount_id] ON [dbo].[Projection-BankAccount]
(
	[id] ASC
)	
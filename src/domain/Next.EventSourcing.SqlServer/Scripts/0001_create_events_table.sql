CREATE TABLE [dbo].[Events]
(
	[aggregate_id] [nvarchar](256) not null,
    [aggregate_name] [nvarchar](256) not null,
    [event_seq] [int] not null,
    [timestamp] [datetime2] not null,
    [data] [nvarchar](max) not null,
    [metadata] [nvarchar](max) not null,
    [committed] [bit] not null default 0,
    [transaction_id] [uniqueidentifier] not null,
    constraint [PK_Events] primary key clustered
         ([aggregate_id] asc, [event_seq] asc)
	)

CREATE UNIQUE NONCLUSTERED INDEX [ix_events_aggregate_id_event_seq] ON [dbo].[Events]
(
	[aggregate_id] ASC,
	[event_seq] ASC
)	
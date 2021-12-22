CREATE TABLE [dbo].[Snapshots]
(
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[aggregate_id] [nvarchar](128) NOT NULL,
	[aggregate_name] [nvarchar](128) NOT NULL,
	[aggregate_event_seq] [int] NOT NULL,
	[data] [nvarchar](MAX) NOT NULL,
	[metadata] [nvarchar](MAX) NOT NULL,
	CONSTRAINT [PK_Snapshots] PRIMARY KEY CLUSTERED
	(
		[id] ASC
	)
)

CREATE UNIQUE NONCLUSTERED INDEX [ix_snapshots_aggregate_id_aggregate_event_seq] ON [dbo].[Snapshots]
(
	[aggregate_name] ASC,
	[aggregate_id] ASC,
	[aggregate_event_seq] ASC
)
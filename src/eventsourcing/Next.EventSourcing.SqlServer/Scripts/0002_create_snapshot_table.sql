CREATE TABLE $SchemaName$.$SnapshotTableName$
(
	[aggregate_id] [nvarchar](128) not null,
	[aggregate_name] [nvarchar](128) not null,
	[aggregate_version] [int] not null,
    [name] [nvarchar](256) not null,
    [data] [nvarchar](max) not null,
    [metadata] [nvarchar](max) not null,
    [timestamp] [datetime2] not null,
    constraint pk_$SnapshotTableName$ primary key CLUSTERED
	(
        [aggregate_name] asc,
        [aggregate_id] asc,
        [aggregate_version] asc
	)
)

CREATE UNIQUE NONCLUSTERED INDEX ix_$SnapshotTableName$_aggregate_id_aggregate_version ON $SchemaName$.$SnapshotTableName$
(
	[aggregate_name] asc,
	[aggregate_id] asc,
	[aggregate_version] asc
)
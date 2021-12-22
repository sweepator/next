IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '$SchemaName$')
BEGIN
EXEC('CREATE SCHEMA $SchemaName$ AUTHORIZATION dbo');
END

CREATE TABLE $SchemaName$.$EventTableName$
(
	[aggregate_id] [nvarchar](256) not null,
    [aggregate_name] [nvarchar](256) not null,
    [name] [nvarchar](256) not null,
    [version] [int] not null,
    [timestamp] [datetime2] not null,
    [data] [nvarchar](max) not null,
    [metadata] [nvarchar](max) not null,
    [committed] [bit] not null default 0,
    [transaction_id] [uniqueidentifier] not null,
    [committed_timestamp] [datetime2] null,
    constraint pk_$EventTableName$ primary key clustered
         ([aggregate_id] asc, [version] asc)
	)

CREATE UNIQUE NONCLUSTERED INDEX [ix_$EventTableName$_aggregate_id_version] ON $SchemaName$.$EventTableName$
(
	[aggregate_id] ASC,
	[version] ASC
)	
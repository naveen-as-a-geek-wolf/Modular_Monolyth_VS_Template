IF OBJECT_ID(N'[game].[__EF_MigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'game') IS NULL EXEC(N'CREATE SCHEMA [game];');
    CREATE TABLE [game].[__EF_MigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EF_MigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF SCHEMA_ID(N'game') IS NULL EXEC(N'CREATE SCHEMA [game];');

IF SCHEMA_ID(N'game_HT') IS NULL EXEC(N'CREATE SCHEMA [game_HT];');

CREATE TABLE [game].[Game] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Genere] nvarchar(max) NOT NULL,
    [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    CONSTRAINT [PK_Game] PRIMARY KEY ([Id]),
    PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [game_HT].[Game]));

INSERT INTO [game].[__EF_MigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250801093907_Inital_Game_Migration', N'9.0.7');

COMMIT;
GO


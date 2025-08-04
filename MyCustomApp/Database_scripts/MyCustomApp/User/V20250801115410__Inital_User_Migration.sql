IF OBJECT_ID(N'[user].[__EF_MigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'user') IS NULL EXEC(N'CREATE SCHEMA [user];');
    CREATE TABLE [user].[__EF_MigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EF_MigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF SCHEMA_ID(N'user') IS NULL EXEC(N'CREATE SCHEMA [user];');

IF SCHEMA_ID(N'user_HT') IS NULL EXEC(N'CREATE SCHEMA [user_HT];');

CREATE TABLE [user].[User] (
    [UserId] int NOT NULL IDENTITY,
    [Name] nvarchar(120) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [CreatedBy] int NULL,
    [CreatedOn] datetimeoffset NOT NULL,
    [ModifiedBy] int NULL,
    [ModifiedOn] datetimeoffset NULL,
    [Rowversion] rowversion NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY ([UserId]),
    PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [user_HT].[User]));

INSERT INTO [user].[__EF_MigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250801115410_Inital_User_Migration', N'9.0.7');

COMMIT;
GO


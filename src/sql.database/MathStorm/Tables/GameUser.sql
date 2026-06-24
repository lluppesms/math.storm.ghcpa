-- =============================================
-- Table: mathstorm.GameUser
-- Description: Stores MathStorm player profiles
-- =============================================
CREATE TABLE [mathstorm].[GameUser] (
    [Id]           NVARCHAR(50)     NOT NULL,
    [Username]     NVARCHAR(100)    NOT NULL,
    [GamesPlayed]  INT              NOT NULL CONSTRAINT [DF_GameUser_GamesPlayed] DEFAULT (0),
    [TotalScore]   FLOAT            NOT NULL CONSTRAINT [DF_GameUser_TotalScore]  DEFAULT (0),
    [BestScore]    FLOAT            NOT NULL CONSTRAINT [DF_GameUser_BestScore]   DEFAULT (0),
    [CreatedAt]    DATETIME2        NOT NULL CONSTRAINT [DF_GameUser_CreatedAt]   DEFAULT (SYSUTCDATETIME()),
    [LastPlayedAt] DATETIME2        NOT NULL CONSTRAINT [DF_GameUser_LastPlayedAt] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_GameUser] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE UNIQUE INDEX [UX_GameUser_Username] ON [mathstorm].[GameUser] ([Username] ASC);
GO

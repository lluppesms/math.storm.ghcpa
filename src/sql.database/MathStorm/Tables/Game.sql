-- =============================================
-- Table: mathstorm.Game
-- Description: Stores completed MathStorm game records
-- Questions are stored as JSON in QuestionsJson
-- =============================================
CREATE TABLE [mathstorm].[Game] (
    [Id]            NVARCHAR(50)     NOT NULL,
    [UserId]        NVARCHAR(50)     NOT NULL,
    [Username]      NVARCHAR(100)    NOT NULL,
    [Difficulty]    NVARCHAR(50)     NOT NULL,
    [TotalScore]    FLOAT            NOT NULL CONSTRAINT [DF_Game_TotalScore]  DEFAULT (0),
    [CompletedAt]   DATETIME2        NOT NULL CONSTRAINT [DF_Game_CompletedAt] DEFAULT (SYSUTCDATETIME()),
    [Analysis]      NVARCHAR(MAX)    NULL,
    [QuestionsJson] NVARCHAR(MAX)    NOT NULL CONSTRAINT [DF_Game_QuestionsJson] DEFAULT ('[]'),
    CONSTRAINT [PK_Game] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE INDEX [IX_Game_UserId]   ON [mathstorm].[Game] ([UserId] ASC);
GO
CREATE INDEX [IX_Game_Username] ON [mathstorm].[Game] ([Username] ASC);
GO

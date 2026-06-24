-- =============================================
-- Table: mathstorm.LeaderboardEntry
-- Description: Stores top leaderboard scores per difficulty
-- Max 10 entries per difficulty, max 3 entries per user per difficulty
-- =============================================
CREATE TABLE [mathstorm].[LeaderboardEntry] (
    [Id]         NVARCHAR(50)    NOT NULL,
    [Difficulty] NVARCHAR(50)    NOT NULL,
    [Username]   NVARCHAR(100)   NOT NULL,
    [UserId]     NVARCHAR(50)    NOT NULL,
    [GameId]     NVARCHAR(50)    NOT NULL,
    [Score]      FLOAT           NOT NULL,
    [AchievedAt] DATETIME2       NOT NULL CONSTRAINT [DF_LeaderboardEntry_AchievedAt] DEFAULT (SYSUTCDATETIME()),
    [Rank]       INT             NOT NULL CONSTRAINT [DF_LeaderboardEntry_Rank] DEFAULT (0),
    CONSTRAINT [PK_LeaderboardEntry] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE INDEX [IX_LeaderboardEntry_Difficulty] ON [mathstorm].[LeaderboardEntry] ([Difficulty] ASC, [Score] ASC);
GO
CREATE INDEX [IX_LeaderboardEntry_Username]   ON [mathstorm].[LeaderboardEntry] ([Username] ASC, [Difficulty] ASC);
GO

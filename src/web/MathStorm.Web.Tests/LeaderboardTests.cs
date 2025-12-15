using MathStorm.Core.Services;

namespace MathStorm.Tests;

[TestClass]
public class LeaderboardTests
{
    private MockCosmosDbService _cosmosDbService = null!;

    [TestInitialize]
    public void Initialize()
    {
        _cosmosDbService = new MockCosmosDbService();
    }

    [TestMethod]
    public async Task AddToLeaderboard_FirstEntry_ShouldAddSuccessfully()
    {
        // Arrange
        var userId = "user1";
        var username = "TestUser";
        var gameId = "game1";
        var difficulty = "Expert";
        var score = 50.0;

        // Act
        var result = await _cosmosDbService.AddToLeaderboardAsync(userId, username, gameId, difficulty, score);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(username, result.Username);
        Assert.AreEqual(score, result.Score);
        Assert.AreEqual(difficulty, result.Difficulty);
    }

    [TestMethod]
    public async Task AddToLeaderboard_SecondEntry_ShouldAddSuccessfully()
    {
        // Arrange
        var userId = "user1";
        var username = "TestUser";
        var difficulty = "Expert";

        // Add first entry
        await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game1", difficulty, 50.0);

        // Act - Add second entry with better score
        var result = await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game2", difficulty, 40.0);

        // Assert
        Assert.IsNotNull(result);
        var leaderboard = await _cosmosDbService.GetLeaderboardAsync(difficulty, 10);
        var userEntries = leaderboard.Where(e => e.Username.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.AreEqual(2, userEntries.Count);
    }

    [TestMethod]
    public async Task AddToLeaderboard_ThirdEntry_ShouldAddSuccessfully()
    {
        // Arrange
        var userId = "user1";
        var username = "TestUser";
        var difficulty = "Expert";

        // Add three entries
        await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game1", difficulty, 50.0);
        await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game2", difficulty, 40.0);

        // Act - Add third entry
        var result = await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game3", difficulty, 45.0);

        // Assert
        Assert.IsNotNull(result);
        var leaderboard = await _cosmosDbService.GetLeaderboardAsync(difficulty, 10);
        var userEntries = leaderboard.Where(e => e.Username.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.AreEqual(3, userEntries.Count);
    }

    [TestMethod]
    public async Task AddToLeaderboard_FourthEntryWorse_ShouldNotAdd()
    {
        // Arrange
        var userId = "user1";
        var username = "TestUser";
        var difficulty = "Expert";

        // Add three entries (scores: 30, 40, 50)
        await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game1", difficulty, 30.0);
        await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game2", difficulty, 40.0);
        await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game3", difficulty, 50.0);

        // Act - Try to add fourth entry with worse score (60 > 50)
        var result = await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game4", difficulty, 60.0);

        // Assert
        Assert.IsNull(result);
        var leaderboard = await _cosmosDbService.GetLeaderboardAsync(difficulty, 10);
        var userEntries = leaderboard.Where(e => e.Username.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.AreEqual(3, userEntries.Count);
        
        // Verify the scores are still 30, 40, 50
        var scores = userEntries.Select(e => e.Score).OrderBy(s => s).ToList();
        Assert.AreEqual(30.0, scores[0]);
        Assert.AreEqual(40.0, scores[1]);
        Assert.AreEqual(50.0, scores[2]);
    }

    [TestMethod]
    public async Task AddToLeaderboard_FourthEntryBetter_ShouldReplaceWorst()
    {
        // Arrange
        var userId = "user1";
        var username = "TestUser";
        var difficulty = "Expert";

        // Add three entries (scores: 30, 40, 50)
        await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game1", difficulty, 30.0);
        await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game2", difficulty, 40.0);
        await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game3", difficulty, 50.0);

        // Act - Add fourth entry with better score (35 < 50)
        var result = await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game4", difficulty, 35.0);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(35.0, result.Score);
        
        var leaderboard = await _cosmosDbService.GetLeaderboardAsync(difficulty, 10);
        var userEntries = leaderboard.Where(e => e.Username.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.AreEqual(3, userEntries.Count);
        
        // Verify the worst score (50) was removed and new score (35) was added
        var scores = userEntries.Select(e => e.Score).OrderBy(s => s).ToList();
        Assert.AreEqual(30.0, scores[0]);
        Assert.AreEqual(35.0, scores[1]);
        Assert.AreEqual(40.0, scores[2]);
    }

    [TestMethod]
    public async Task AddToLeaderboard_CaseInsensitiveUsername_ShouldEnforceLimit()
    {
        // Arrange
        var userId = "user1";
        var difficulty = "Expert";

        // Add three entries with different case variations of username
        await _cosmosDbService.AddToLeaderboardAsync(userId, "TestUser", "game1", difficulty, 30.0);
        await _cosmosDbService.AddToLeaderboardAsync(userId, "testuser", "game2", difficulty, 40.0);
        await _cosmosDbService.AddToLeaderboardAsync(userId, "TESTUSER", "game3", difficulty, 50.0);

        // Act - Try to add fourth entry with different case (should still count towards limit)
        var result = await _cosmosDbService.AddToLeaderboardAsync(userId, "TeStUsEr", "game4", difficulty, 60.0);

        // Assert
        Assert.IsNull(result);
        var leaderboard = await _cosmosDbService.GetLeaderboardAsync(difficulty, 10);
        var userEntries = leaderboard.Where(e => e.Username.Equals("TestUser", StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.AreEqual(3, userEntries.Count);
    }

    [TestMethod]
    public async Task AddToLeaderboard_MultipleUsers_ShouldEnforceLimitPerUser()
    {
        // Arrange
        var difficulty = "Expert";

        // User 1: Add three entries
        await _cosmosDbService.AddToLeaderboardAsync("user1", "Alice", "game1", difficulty, 30.0);
        await _cosmosDbService.AddToLeaderboardAsync("user1", "Alice", "game2", difficulty, 40.0);
        await _cosmosDbService.AddToLeaderboardAsync("user1", "Alice", "game3", difficulty, 50.0);

        // User 2: Add two entries
        await _cosmosDbService.AddToLeaderboardAsync("user2", "Bob", "game4", difficulty, 35.0);
        await _cosmosDbService.AddToLeaderboardAsync("user2", "Bob", "game5", difficulty, 45.0);

        // Act - User 2 should be able to add third entry
        var bobResult = await _cosmosDbService.AddToLeaderboardAsync("user2", "Bob", "game6", difficulty, 55.0);
        
        // User 1 should not be able to add fourth worse entry
        var aliceResult = await _cosmosDbService.AddToLeaderboardAsync("user1", "Alice", "game7", difficulty, 60.0);

        // Assert
        Assert.IsNotNull(bobResult);
        Assert.IsNull(aliceResult);
        
        var leaderboard = await _cosmosDbService.GetLeaderboardAsync(difficulty, 10);
        var aliceEntries = leaderboard.Where(e => e.Username.Equals("Alice", StringComparison.OrdinalIgnoreCase)).ToList();
        var bobEntries = leaderboard.Where(e => e.Username.Equals("Bob", StringComparison.OrdinalIgnoreCase)).ToList();
        
        Assert.AreEqual(3, aliceEntries.Count);
        Assert.AreEqual(3, bobEntries.Count);
    }

    [TestMethod]
    public async Task AddToLeaderboard_DifferentDifficulties_ShouldAllowThreePerDifficulty()
    {
        // Arrange
        var userId = "user1";
        var username = "TestUser";

        // Add three entries for "Expert" difficulty
        await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game1", "Expert", 30.0);
        await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game2", "Expert", 40.0);
        await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game3", "Expert", 50.0);

        // Act - Add three entries for "Beginner" difficulty (should be allowed)
        var result1 = await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game4", "Beginner", 35.0);
        var result2 = await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game5", "Beginner", 45.0);
        var result3 = await _cosmosDbService.AddToLeaderboardAsync(userId, username, "game6", "Beginner", 55.0);

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.IsNotNull(result3);
        
        var expertLeaderboard = await _cosmosDbService.GetLeaderboardAsync("Expert", 10);
        var beginnerLeaderboard = await _cosmosDbService.GetLeaderboardAsync("Beginner", 10);
        
        var expertEntries = expertLeaderboard.Where(e => e.Username.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();
        var beginnerEntries = beginnerLeaderboard.Where(e => e.Username.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();
        
        Assert.AreEqual(3, expertEntries.Count);
        Assert.AreEqual(3, beginnerEntries.Count);
    }
}

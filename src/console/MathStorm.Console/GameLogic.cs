namespace MathStorm.Console;

public class GameLogic
{
    private readonly IConsoleMathStormService _mathStormService;
    private readonly ILogger<GameLogic> _logger;

    public GameLogic(IConsoleMathStormService mathStormService, ILogger<GameLogic> logger)
    {
        _mathStormService = mathStormService;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        // Game state
        var continuePlayingGames = true;
        var username = string.Empty;

        // Display Banner
        AnsiConsole.Write(new FigletText("Math Storm").LeftJustified().Color(Color.Blue));

        // Display Welcome Message
        AnsiConsole.MarkupLine("\n[blue]Welcome to Math Storm![/] ⚡ Test your mathematical skills with our lightning-fast math game!");
        AnsiConsole.MarkupLine("[grey]Answer math questions as quickly and accurately as possible to earn the highest score![/]\n");

        // Get username
        username = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]What's your name?[/]")
                .PromptStyle("blue")
                .ValidationErrorMessage("[red]Please enter a valid name![/]")
                .Validate(name =>
                {
                    return name.Length >= 1 ? ValidationResult.Success() : ValidationResult.Error("[red]Name cannot be empty![/]");
                }));

        AnsiConsole.MarkupLine($"\n[green]Hello {username}![/] Let's start playing! 🎮\n");

        // Main game loop
        while (continuePlayingGames)
        {
            // Show main menu options
            var mainMenuOption = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What would you like to do?")
                    .AddChoices([
                        "🎮 Play a Game",
                        "🏆 View Leaderboard",
                        "🚪 Exit"
                    ]));

            switch (mainMenuOption)
            {
                case "🎮 Play a Game":
                    await PlayGameAsync(username);
                    break;
                case "🏆 View Leaderboard":
                    await ShowLeaderboardAsync();
                    break;
                case "🚪 Exit":
                    continuePlayingGames = false;
                    break;
            }

            if (continuePlayingGames)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
                System.Console.ReadKey();
                AnsiConsole.Clear();
                AnsiConsole.Write(new FigletText("Math Storm").LeftJustified().Color(Color.Blue));
                AnsiConsole.MarkupLine($"\n[green]Welcome back {username}![/] 🎮\n");
            }
        }

        AnsiConsole.MarkupLine("\n[aqua]Thanks for playing Math Storm![/] 👋 See you next time!");
    }

    private async Task PlayGameAsync(string username)
    {
        var gameModeChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose [green]play mode[/]:")
                .AddChoices([
                    "⚡ Classic - direct equations",
                    "📖 Story Time - word-based math problems"
                ]));

        var gameMode = gameModeChoice.Contains("Story Time") ? GameMode.StoryTime : GameMode.Classic;

        // Select difficulty
        var difficultyChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select [green]difficulty level[/]:")
                .AddChoices([
                    "🌱 Beginner - 5 questions, 2-digit numbers, Addition & Subtraction only",
                    "🚀 Novice - 5 questions, 2-digit numbers, All operations",
                    "🔥 Intermediate - 10 questions, 3-digit numbers, All operations",
                    "⚡ Expert - 10 questions, 4-digit numbers, All operations"
                ]));

        var difficulty = difficultyChoice switch
        {
            var s when s.Contains("Beginner") => Difficulty.Beginner,
            var s when s.Contains("Novice") => Difficulty.Novice,
            var s when s.Contains("Intermediate") => Difficulty.Intermediate,
            var s when s.Contains("Expert") => Difficulty.Expert,
            _ => Difficulty.Expert
        };

        var modeDisplay = gameMode == GameMode.StoryTime ? "Story Time" : "Classic";
        AnsiConsole.MarkupLine($"\n[green]Starting {modeDisplay} - {difficulty}...[/] 🎯");

        // Get game using direct service call
        var gameData = _mathStormService.GetGame(difficulty, gameMode);
        GameSession? gameSession = null;
        string gameId = string.Empty;

        if (gameData != null)
        {
            // Service call successful - use game data
            gameId = gameData.GameId;
            var sessionGameMode = Enum.TryParse<GameMode>(gameData.GameMode, out var parsedGameMode)
                ? parsedGameMode
                : GameMode.Classic;

            gameSession = new GameSession
            {
                Difficulty = difficulty,
                GameMode = sessionGameMode,
                Questions = gameData.Questions.Select(q => new MathQuestion
                {
                    Id = q.Id,
                    Number1 = q.Number1,
                    Number2 = q.Number2,
                    Operation = Enum.Parse<MathOperation>(q.Operation),
                    CorrectAnswer = q.CorrectAnswer,
                    PromptText = q.QuestionText
                }).ToList()
            };
            AnsiConsole.MarkupLine($"[yellow]Game ID: {gameId}[/]");
        }
        else
        {
            // Service unavailable - should not happen but handle gracefully
            AnsiConsole.MarkupLine("[red]Error: Unable to create game. Please check configuration.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[blue]You will answer {gameSession.Questions.Count} questions. Good luck![/]\n");

        // Track game results
        var gameResults = new List<QuestionResultDto>();
        var totalScore = 0.0;

        AnsiConsole.MarkupLine("[grey]Press any key when you're ready to start...[/]");
        System.Console.ReadKey();
        AnsiConsole.Clear();

        // Play through questions
        for (int i = 0; i < gameSession.Questions.Count; i++)
        {
            var question = gameSession.Questions[i];
            var questionNumber = i + 1;

            AnsiConsole.Write(new Rule($"Question {questionNumber} of {gameSession.Questions.Count}").RuleStyle("blue"));
            AnsiConsole.WriteLine();

            var startTime = DateTime.UtcNow;

            // Display the question prominently
            var panel = new Panel($"[white bold]{question.QuestionText}[/]")
                .Header(gameMode == GameMode.StoryTime ? "[blue]Story Time[/]" : "[blue]Math Problem[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Blue);

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();

            // Get user answer
            var userAnswer = AnsiConsole.Prompt(
                new TextPrompt<double>("[green]Your answer:[/]")
                    .PromptStyle("yellow")
                    .ValidationErrorMessage("[red]Please enter a valid number![/]"));

            var endTime = DateTime.UtcNow;
            var timeInSeconds = (endTime - startTime).TotalSeconds;

            // Calculate score for this question using same logic as web app
            var correctAnswer = question.CorrectAnswer;
            var difference = Math.Abs(correctAnswer - userAnswer);
            var percentageDifference = correctAnswer == 0 ?
                (userAnswer == 0 ? 0 : Math.Abs(userAnswer) * 100) :
                Math.Round((difference / Math.Abs(correctAnswer)) * 100, 1);

            // Use same scoring formula as web app
            var timeFactor = 10.0;
            if (timeInSeconds <= 1) { timeFactor = 100.0; }

            var questionScore = Math.Round(
                (percentageDifference * timeInSeconds) +
                (timeInSeconds * timeFactor), 1);

            totalScore += questionScore;

            // Show immediate feedback
            var isCorrect = Math.Abs(userAnswer - question.CorrectAnswer) < 0.01;
            if (isCorrect)
            {
                AnsiConsole.MarkupLine($"[green bold]✓ Correct![/] Time: {timeInSeconds:F1}s, Score: {questionScore:F1}");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]✗ Incorrect.[/] Correct answer: [yellow]{question.CorrectAnswer}[/]");
                AnsiConsole.MarkupLine($"Time: {timeInSeconds:F1}s, Score: {questionScore:F1}");
            }

            // Record result
            gameResults.Add(new QuestionResultDto
            {
                Id = question.Id,
                Number1 = question.Number1,
                Number2 = question.Number2,
                Operation = question.Operation.ToString(),
                CorrectAnswer = question.CorrectAnswer,
                UserAnswer = userAnswer,
                TimeInSeconds = timeInSeconds,
                PercentageDifference = percentageDifference,
                Score = questionScore,
                QuestionText = question.QuestionText
            });

            if (i < gameSession.Questions.Count - 1)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Press any key for the next question...[/]");
                System.Console.ReadKey();
                AnsiConsole.Clear();
            }
        }

        // Game completed - show final results
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("Game Complete!").RuleStyle("green"));
        AnsiConsole.WriteLine();

        var resultsTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green);

        resultsTable.AddColumn("📊 [blue bold]Game Summary[/]");
        resultsTable.AddColumn("[white bold]Results[/]");

        resultsTable.AddRow("Player", $"[yellow]{username}[/]");
        resultsTable.AddRow("Mode", $"[blue]{modeDisplay}[/]");
        resultsTable.AddRow("Difficulty", $"[blue]{difficulty}[/]");
        resultsTable.AddRow("Questions", $"[white]{gameSession.Questions.Count}[/]");
        resultsTable.AddRow("Total Score", $"[green bold]{totalScore:F1}[/]");

        var correctAnswers = gameResults.Count(r => Math.Abs(r.PercentageDifference) < 1);
        var accuracy = (correctAnswers / (double)gameResults.Count) * 100;
        resultsTable.AddRow("Accuracy", $"[yellow]{accuracy:F1}%[/] ({correctAnswers}/{gameResults.Count})");

        var avgTime = gameResults.Average(r => r.TimeInSeconds);
        resultsTable.AddRow("Average Time", $"[cyan]{avgTime:F1}s[/]");

        AnsiConsole.Write(resultsTable);

        // Submit results to API if we have connection
        AnsiConsole.WriteLine();
        if (gameData != null)
        {
            AnsiConsole.MarkupLine("[blue]Submitting your results...[/] 📤");

            var submitRequest = new GameResultsRequestDto
            {
                GameId = gameId,
                Username = username,
                Difficulty = difficulty.ToString(),
                GameMode = gameSession.GameMode.ToString(),
                Questions = gameResults
            };

            var submitResponse = await _mathStormService.SubmitGameResultsAsync(submitRequest);
            if (submitResponse != null)
            {
                AnsiConsole.MarkupLine("[green]✓ Results submitted successfully![/] 🎉");
                if (submitResponse.AddedToLeaderboard)
                {
                    var rankText = submitResponse.LeaderboardRank.HasValue ? $" (Rank #{submitResponse.LeaderboardRank.Value})" : "";
                    AnsiConsole.MarkupLine($"[gold1 bold]🏆 ADDED TO LEADERBOARD!{rankText} Congratulations! 🏆[/]");
                }
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]📱 Local game completed - scores not submitted due to API unavailability[/]");
        }
    }

    private async Task ShowLeaderboardAsync()
    {
        // Option to filter by difficulty
        var filterChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Show leaderboard for:")
                .AddChoices([
                    "🌟 All Difficulties",
                    "🌱 Beginner Only",
                    "🚀 Novice Only",
                    "⚡ Expert Only",
                    "🔥 Intermediate Only"
                ]));

        string? difficultyFilter = filterChoice switch
        {
            var s when s.Contains("Beginner") => "Beginner",
            var s when s.Contains("Novice") => "Novice",
            var s when s.Contains("Expert") => "Expert",
            var s when s.Contains("Intermediate") => "Intermediate",
            _ => null
        };

        AnsiConsole.MarkupLine("\n[blue]Loading leaderboard...[/] 📊");

        var leaderboard = await _mathStormService.GetLeaderboardAsync(difficultyFilter, 15);
        if (leaderboard == null) return;

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("🏆 Leaderboard 🏆").RuleStyle("gold1"));
        AnsiConsole.WriteLine();

        if (!leaderboard.Entries.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No scores found for the selected difficulty.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Gold1);

        table.AddColumn("Rank", column => column.Centered());
        table.AddColumn("Player", column => column.LeftAligned());
        table.AddColumn("Score", column => column.RightAligned());
        table.AddColumn("Difficulty", column => column.Centered());
        table.AddColumn("Date", column => column.RightAligned());

        for (int i = 0; i < leaderboard.Entries.Count; i++)
        {
            var entry = leaderboard.Entries[i];
            var rank = i + 1;

            var rankDisplay = rank switch
            {
                1 => "[gold1]🥇 1st[/]",
                2 => "[silver]🥈 2nd[/]",
                3 => "[orange1]🥉 3rd[/]",
                _ => $"[white]{rank}[/]"
            };

            var difficultyEmoji = entry.Difficulty switch
            {
                "Beginner" => "🌱",
                "Novice" => "🚀",
                "Expert" => "⚡",
                "Intermediate" => "🔥",
                _ => "❓"
            };

            table.AddRow(
                rankDisplay,
                $"[cyan]{entry.Username}[/]",
                $"[green]{entry.Score:F1}[/]",
                $"{difficultyEmoji} [blue]{entry.Difficulty}[/]",
                $"[grey]{entry.AchievedAt:MM/dd}[/]"
            );
        }

        AnsiConsole.Write(table);
    }
}

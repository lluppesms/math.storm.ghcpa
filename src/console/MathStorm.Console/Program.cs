Console.OutputEncoding = Encoding.UTF8;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Setup dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConfiguration(configuration.GetSection("Logging"));
    builder.AddConsole();
});

// Add HttpClient
var baseUrl = configuration.GetValue<string>("FunctionService:BaseUrl") ?? "https://localhost:7071";
services.AddSingleton(new HttpClient
{
    BaseAddress = new Uri(baseUrl)
});

// Add our service
services.AddScoped<IConsoleMathStormService, ConsoleMathStormService>();

var serviceProvider = services.BuildServiceProvider();
var mathStormService = serviceProvider.GetRequiredService<IConsoleMathStormService>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

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
            await PlayGameAsync(mathStormService, username);
            break;
        case "🏆 View Leaderboard":
            await ShowLeaderboardAsync(mathStormService);
            break;
        case "🚪 Exit":
            continuePlayingGames = false;
            break;
    }

    if (continuePlayingGames)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey();
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Math Storm").LeftJustified().Color(Color.Blue));
        AnsiConsole.MarkupLine($"\n[green]Welcome back {username}![/] 🎮\n");
    }
}

AnsiConsole.MarkupLine("\n[aqua]Thanks for playing Math Storm![/] 👋 See you next time!");

// Game Playing Method
static async Task PlayGameAsync(IConsoleMathStormService mathStormService, string username)
{
    // Select difficulty
    var difficultyChoice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Select [green]difficulty level[/]:")
            .AddChoices([
                "🌱 Beginner - 5 questions, 2-digit numbers, Addition & Subtraction only",
                "🚀 Novice - 5 questions, 2-digit numbers, All operations", 
                "⚡ Expert - 10 questions, 3-digit numbers, All operations",
                "🔥 Intermediate - 10 questions, 3-digit numbers, All operations"
            ]));

    var difficulty = difficultyChoice switch
    {
        var s when s.Contains("Beginner") => Difficulty.Beginner,
        var s when s.Contains("Novice") => Difficulty.Novice, 
        var s when s.Contains("Expert") => Difficulty.Expert,
        var s when s.Contains("Intermediate") => Difficulty.Intermediate,
        _ => Difficulty.Expert
    };

    AnsiConsole.MarkupLine($"\n[green]Starting {difficulty} game...[/] 🎯");

    // Get game from API
    var gameData = await mathStormService.GetGameAsync(difficulty);
    if (gameData == null) return;

    AnsiConsole.MarkupLine($"[yellow]Game ID: {gameData.GameId}[/]");
    AnsiConsole.MarkupLine($"[blue]You will answer {gameData.Questions.Count} questions. Good luck![/]\n");

    // Track game results
    var gameResults = new List<QuestionResultDto>();
    var totalScore = 0.0;

    AnsiConsole.MarkupLine("[grey]Press any key when you're ready to start...[/]");
    Console.ReadKey();
    AnsiConsole.Clear();

    // Play through questions
    for (int i = 0; i < gameData.Questions.Count; i++)
    {
        var question = gameData.Questions[i];
        var questionNumber = i + 1;

        AnsiConsole.Write(new Rule($"Question {questionNumber} of {gameData.Questions.Count}").RuleStyle("blue"));
        AnsiConsole.WriteLine();

        var startTime = DateTime.UtcNow;

        // Display the question prominently
        var panel = new Panel($"[white bold]{question.QuestionText}[/]")
            .Header("[blue]Math Problem[/]")
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

        // Calculate score for this question
        var percentageDifference = Math.Abs((userAnswer - question.CorrectAnswer) / question.CorrectAnswer) * 100;
        var accuracyScore = Math.Max(0, 100 - percentageDifference);
        var timeBonus = Math.Max(0, 50 - timeInSeconds); // Bonus for speed (up to 50 points)
        var questionScore = (accuracyScore + timeBonus) / 1.5; // Scale to reasonable range

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
            Operation = question.Operation,
            CorrectAnswer = question.CorrectAnswer,
            UserAnswer = userAnswer,
            TimeInSeconds = timeInSeconds,
            PercentageDifference = percentageDifference,
            Score = questionScore
        });

        if (i < gameData.Questions.Count - 1)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key for the next question...[/]");
            Console.ReadKey();
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
    resultsTable.AddRow("Difficulty", $"[blue]{difficulty}[/]");
    resultsTable.AddRow("Questions", $"[white]{gameData.Questions.Count}[/]");
    resultsTable.AddRow("Total Score", $"[green bold]{totalScore:F1}[/]");

    var correctAnswers = gameResults.Count(r => Math.Abs(r.PercentageDifference) < 1);
    var accuracy = (correctAnswers / (double)gameResults.Count) * 100;
    resultsTable.AddRow("Accuracy", $"[yellow]{accuracy:F1}%[/] ({correctAnswers}/{gameResults.Count})");

    var avgTime = gameResults.Average(r => r.TimeInSeconds);
    resultsTable.AddRow("Average Time", $"[cyan]{avgTime:F1}s[/]");

    AnsiConsole.Write(resultsTable);

    // Submit results to API
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[blue]Submitting your results...[/] 📤");

    var submitRequest = new GameResultsRequestDto
    {
        GameId = gameData.GameId,
        Username = username,
        Difficulty = difficulty.ToString(),
        Questions = gameResults
    };

    var submitResponse = await mathStormService.SubmitGameResultsAsync(submitRequest);
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

// Leaderboard Method
static async Task ShowLeaderboardAsync(IConsoleMathStormService mathStormService)
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

    var leaderboard = await mathStormService.GetLeaderboardAsync(difficultyFilter, 15);
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
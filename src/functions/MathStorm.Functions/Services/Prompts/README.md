# File-Based Prompt System

The Results Analysis Service now uses a file-based prompt system that allows for easy customization and modification of AI prompts without code changes.

## Prompt File Structure

All prompt files are located in the `Services/Prompts/` folder:

```
Services/
└── Prompts/
    ├── default.txt              # Default personality prompt
    ├── comedyroast.txt          # Comedy roast personality
    ├── pirate.txt               # Pirate personality
    ├── limerick.txt             # Limerick personality
    ├── sportsbroadcaster.txt    # Sports broadcaster personality
    ├── haiku.txt                # Haiku personality
    ├── australian.txt           # Australian personality
    ├── yourmother.txt           # Mother personality
    └── ChatMessagePrompt.txt    # User message template
```

## Personality Prompt Files

Each personality has its own `.txt` file containing the system prompt that defines the AI's personality and behavior:

### Example: `pirate.txt`
```
You are a pirate captain. Use pirate language, maritime metaphors, and seafaring terminology to comment on the math performance. Say 'arrr' and use 'ye', 'matey', etc.
```

### Example: `haiku.txt`
```
You must respond ONLY in haiku form - exactly 3 lines with 5-7-5 syllable pattern. Focus on their math performance.
```

## Chat Message Template

The `ChatMessagePrompt.txt` file contains the user message template with placeholders that get replaced with actual game data:

```
Analyze this math game performance:

Player: {Username}
Difficulty: {Difficulty}
Total Questions: {TotalQuestions}
Correct Answers: {CorrectAnswers}
Accuracy: {Accuracy:F1}%
Total Score: {TotalScore:F1}
Average Time per Question: {AverageTime:F1} seconds
Fastest Question: {FastestTime:F1} seconds
Slowest Question: {SlowestTime:F1} seconds

Provide an entertaining analysis in your assigned personality style. Keep it to 2-3 sentences maximum unless the format requires otherwise (like limerick or haiku). Be encouraging while pointing out both strengths and areas for improvement.
```

## Available Placeholders

The following placeholders are automatically replaced with actual game data:

- `{Username}` - Player's username
- `{Difficulty}` - Game difficulty level
- `{TotalQuestions}` - Total number of questions
- `{CorrectAnswers}` - Number of correct answers
- `{Accuracy:F1}` - Accuracy percentage (formatted to 1 decimal place)
- `{TotalScore:F1}` - Total score (formatted to 1 decimal place)
- `{AverageTime:F1}` - Average time per question in seconds
- `{FastestTime:F1}` - Fastest question time in seconds
- `{SlowestTime:F1}` - Slowest question time in seconds

## How It Works

1. **System Prompt Loading**: The `GetSystemPrompt()` method reads the appropriate personality file (e.g., `pirate.txt` for pirate personality)

2. **Chat Message Generation**: The `GetChatMessagePrompt()` method reads `ChatMessagePrompt.txt` and replaces all placeholders with actual game data

3. **Fallback Handling**: If a prompt file is missing or unreadable, the service falls back to default hard-coded prompts

4. **Error Handling**: File reading errors are logged, and the service gracefully degrades to default behavior

## Adding New Personalities

To add a new personality:

1. Create a new `.txt` file in `Services/Prompts/` with the personality name (e.g., `robot.txt`)
2. Write the system prompt that defines the AI's behavior
3. The personality will automatically be available for use
4. Update the validation in `ResultsAnalysisFunctions.cs` if needed

### Example: Adding a "Scientist" Personality

Create `Services/Prompts/scientist.txt`:
```
You are a research scientist analyzing mathematical performance data. Use scientific terminology, statistical observations, and analytical language. Reference hypotheses, data points, and mathematical correlations.
```

## Benefits

1. **Easy Customization**: Modify prompts without code changes
2. **Version Control**: Track prompt changes in git
3. **A/B Testing**: Easy to test different prompt variations
4. **Localization**: Support for different languages by replacing prompt files
5. **Dynamic Loading**: Changes take effect immediately without redeployment
6. **Fallback Safety**: Graceful degradation if files are missing

## File Deployment

The prompt files are automatically copied to the output directory during build and deployment through the project file configuration:

```xml
<None Update="Services\Prompts\*.txt">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

## Best Practices

1. **Keep prompts concise but clear**
2. **Test prompts with various game scenarios**
3. **Maintain consistent tone within each personality**
4. **Document any special formatting requirements (like haiku/limerick)**
5. **Use meaningful personality names that match the file names**
6. **Backup original prompts before making changes**

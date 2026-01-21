namespace DroneScriptParser.AST;

/// <summary>
/// Base class for all DroneScript statements
/// </summary>
public abstract record Statement
{
    public int Line { get; init; }
}

/// <summary>
/// Represents an IF-THEN conditional statement
/// Example: IF battery < 20 THEN goto_charger
/// </summary>
public record ConditionalStatement(
    Condition Condition,
    Command ThenCommand,
    int Line
) : Statement;

/// <summary>
/// Represents an ELSE statement (fallback if previous conditions failed)
/// Example: ELSE mine_nearest(any)
/// </summary>
public record ElseStatement(
    Command ElseCommand,
    int Line
) : Statement;

/// <summary>
/// Represents a simple command statement (no condition)
/// Example: goto_charger
/// </summary>
public record CommandStatement(
    Command Command,
    int Line
) : Statement;

namespace DroneScriptParser.AST;

/// <summary>
/// Represents a command in DroneScript
/// Examples: goto_charger, mine_nearest(Uranium), goto_location(10, 20)
/// </summary>
public record Command(
    string Name,                    // Command name (e.g., "goto_charger", "mine_nearest")
    List<CommandArgument> Arguments // Arguments (can be empty for commands like "goto_charger")
);

/// <summary>
/// Represents a command argument (can be an identifier or a number)
/// </summary>
public abstract record CommandArgument;

/// <summary>
/// Identifier argument (resource types, destination names, etc.)
/// Example: Uranium in mine_nearest(Uranium)
/// </summary>
public record IdentifierArgument(string Value) : CommandArgument;

/// <summary>
/// Numeric argument (coordinates, thresholds, etc.)
/// Example: 10 and 20 in goto_location(10, 20)
/// </summary>
public record NumberArgument(string Value) : CommandArgument;

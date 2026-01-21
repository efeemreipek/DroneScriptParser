namespace DroneScriptParser.AST;

/// <summary>
/// Base class for all conditions used in IF statements
/// </summary>
public abstract record Condition;

/// <summary>
/// Represents a comparison condition
/// Examples: battery < 20, hp >= 50, cargo_full == true
/// </summary>
public record ComparisonCondition(
    string Left,           // Variable name (e.g., "battery", "hp")
    ComparisonOperator Operator,
    string Right           // Value or variable name (e.g., "20", "100")
) : Condition;

/// <summary>
/// Represents a simple boolean query
/// Examples: cargo_full, storm_active, in_hazard_zone
/// </summary>
public record QueryCondition(
    string QueryName       // e.g., "cargo_full", "storm_active"
) : Condition;

/// <summary>
/// Represents a logical AND/OR combination of conditions
/// Examples: battery < 20 AND cargo_full, hp < 50 OR in_hazard_zone
/// </summary>
public record LogicalCondition(
    Condition Left,
    LogicalOperator Operator,
    Condition Right
) : Condition;

/// <summary>
/// Comparison operators for conditions
/// </summary>
public enum ComparisonOperator
{
    LessThan,           // <
    LessThanEqual,      // <=
    GreaterThan,        // >
    GreaterThanEqual,   // >=
    Equal,              // ==
    NotEqual            // !=
}

/// <summary>
/// Logical operators for combining conditions
/// </summary>
public enum LogicalOperator
{
    And,    // AND
    Or      // OR
}

namespace DroneScriptParser.AST;

/// <summary>
/// Represents a complete DroneScript program
/// Contains a list of statements that execute in order
/// </summary>
public record Script(
    List<Statement> Statements
)
{
    public override string ToString()
    {
        return $"Script with {Statements.Count} statements";
    }
}

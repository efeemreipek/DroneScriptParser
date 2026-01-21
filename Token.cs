namespace DroneScriptParser;

/// <summary>
/// Represents the type of a token in DroneScript
/// </summary>
public enum TokenType
{
    // Literals
    Identifier,    // goto_charger, battery, Iron, etc.
    Number,        // 20, 100, 45.5

    // Keywords
    If,
    Then,
    Else,
    And,
    Or,

    // Operators
    LessThan,           // <
    LessThanEqual,      // <=
    GreaterThan,        // >
    GreaterThanEqual,   // >=
    Equal,              // ==
    NotEqual,           // !=

    // Punctuation
    LeftParen,          // (
    RightParen,         // )
    Comma,              // ,

    // Structure
    Newline,            // End of line (important for line-based execution)
    Eof                 // End of file
}

/// <summary>
/// Represents a single token in the DroneScript source
/// </summary>
public record Token(
    TokenType Type,
    string Lexeme,      // The actual text (e.g., "goto_charger", "20", "<")
    int Line,           // Line number for error reporting
    int Column          // Column number for error reporting
)
{
    public override string ToString()
    {
        // Escape special characters for better display
        var displayLexeme = Lexeme
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");

        return $"{Type}('{displayLexeme}') at {Line}:{Column}";
    }
}

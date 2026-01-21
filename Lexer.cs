namespace DroneScriptParser;

/// <summary>
/// Lexical analyzer that converts DroneScript source text into tokens
/// </summary>
public class Lexer
{
    private readonly string _source;
    private readonly List<Token> _tokens = new();
    private readonly List<string> _errors = new();

    private int _start = 0;      // Start of current lexeme
    private int _current = 0;    // Current character position
    private int _line = 1;       // Current line number
    private int _column = 1;     // Current column number

    public bool HasErrors => _errors.Count > 0;
    public IReadOnlyList<string> Errors => _errors;

    private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "if", TokenType.If },
        { "then", TokenType.Then },
        { "else", TokenType.Else },
        { "and", TokenType.And },
        { "or", TokenType.Or }
    };

    public Lexer(string source)
    {
        _source = source;
    }

    /// <summary>
    /// Tokenizes the entire source string
    /// </summary>
    public List<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        // Add EOF token
        _tokens.Add(new Token(TokenType.Eof, "", _line, _column));
        return _tokens;
    }

    /// <summary>
    /// Scans a single token from the source
    /// </summary>
    private void ScanToken()
    {
        char c = Advance();

        switch (c)
        {
            // Single-character tokens
            case '(':
                AddToken(TokenType.LeftParen);
                break;
            case ')':
                AddToken(TokenType.RightParen);
                break;
            case ',':
                AddToken(TokenType.Comma);
                break;

            // Comparison operators (with potential two-character variants)
            case '>':
                AddToken(Match('=') ? TokenType.GreaterThanEqual : TokenType.GreaterThan);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessThanEqual : TokenType.LessThan);
                break;

            // Equality operators (must be two characters)
            case '=':
                if (Match('='))
                    AddToken(TokenType.Equal);
                else
                    Error($"Unexpected character '='. Did you mean '==' for equality comparison?");
                break;
            case '!':
                if (Match('='))
                    AddToken(TokenType.NotEqual);
                else
                    Error($"Unexpected character '!'. Did you mean '!=' for not-equal comparison?");
                break;

            // Comments - skip everything until end of line
            case '#':
                while (Peek() != '\n' && !IsAtEnd())
                    Advance();
                break;

            // Whitespace
            case ' ':
            case '\r':
            case '\t':
                // Skip whitespace
                break;

            // Newline - important for line-based execution
            case '\n':
                AddToken(TokenType.Newline);
                _line++;
                _column = 1;
                break;

            default:
                // Identifiers and keywords
                if (IsAlpha(c))
                {
                    ScanIdentifierOrKeyword();
                }
                // Numbers
                else if (IsDigit(c))
                {
                    ScanNumber();
                }
                // Unexpected character
                else
                {
                    Error($"Unexpected character '{c}'");
                }
                break;
        }
    }

    /// <summary>
    /// Scans an identifier or keyword
    /// Reads alphanumeric characters (including underscores) and determines if it's a keyword
    /// </summary>
    private void ScanIdentifierOrKeyword()
    {
        // Continue reading while we see alphanumeric characters or underscores
        // Note: First character was already consumed by ScanToken() via Advance()
        while (IsAlphaNumeric(Peek()))
            Advance();

        // Extract the word from source
        string text = _source.Substring(_start, _current - _start);

        // Check if it's a keyword (case-insensitive lookup)
        if (Keywords.TryGetValue(text, out TokenType keywordType))
        {
            AddToken(keywordType);
        }
        else
        {
            // Not a keyword, so it's an identifier (command name, resource type, etc.)
            AddToken(TokenType.Identifier);
        }
    }

    /// <summary>
    /// Scans a numeric literal (integer or float)
    /// Handles both integers (20, 100) and floats (45.5, 0.25)
    /// </summary>
    private void ScanNumber()
    {
        // Read all integer digits
        // Note: First digit was already consumed by ScanToken() via Advance()
        while (IsDigit(Peek()))
            Advance();

        // Check for decimal point followed by at least one digit (for floats)
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            // Consume the '.'
            Advance();

            // Read fractional digits
            while (IsDigit(Peek()))
                Advance();
        }

        // Create the number token
        // Note: We store it as a string - the parser/interpreter will convert to float when needed
        AddToken(TokenType.Number);
    }

    // Helper methods (these are implemented for you)

    /// <summary>
    /// Checks if we've consumed all characters in the source
    /// </summary>
    private bool IsAtEnd() => _current >= _source.Length;

    /// <summary>
    /// Consumes the current character and returns it, advancing both position and column
    /// </summary>
    private char Advance()
    {
        _column++;
        return _source[_current++];
    }

    /// <summary>
    /// Returns the current character without consuming it (lookahead)
    /// Returns '\0' if at end of source
    /// </summary>
    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _source[_current];
    }

    /// <summary>
    /// Returns the next character (one ahead of current) without consuming
    /// Useful for two-character lookahead (e.g., checking for '==' or '!=')
    /// Returns '\0' if beyond end of source
    /// </summary>
    private char PeekNext()
    {
        if (_current + 1 >= _source.Length) return '\0';
        return _source[_current + 1];
    }

    /// <summary>
    /// Conditionally consumes the current character if it matches expected
    /// Returns true if matched and consumed, false otherwise
    /// Useful for two-character operators like '==', '!=', '<=', '>='
    /// </summary>
    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;

        _current++;
        _column++;
        return true;
    }

    /// <summary>
    /// Creates a token from the current lexeme (from _start to _current)
    /// and adds it to the tokens list with proper line/column tracking
    /// </summary>
    private void AddToken(TokenType type)
    {
        string lexeme = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, lexeme, _line, _column - lexeme.Length));
    }

    /// <summary>
    /// Checks if a character is a letter (a-z, A-Z) or underscore
    /// Used for identifier/keyword recognition
    /// </summary>
    private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';

    /// <summary>
    /// Checks if a character is a digit (0-9)
    /// </summary>
    private bool IsDigit(char c) => c >= '0' && c <= '9';

    /// <summary>
    /// Checks if a character is alphanumeric (letter, digit, or underscore)
    /// Used for continuing identifier/keyword scanning
    /// </summary>
    private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

    /// <summary>
    /// Reports a lexical error with line and column information
    /// </summary>
    private void Error(string message)
    {
        _errors.Add($"[Line {_line}, Column {_column}] Error: {message}");
    }
}

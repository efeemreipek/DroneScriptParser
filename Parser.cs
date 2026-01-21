using DroneScriptParser.AST;

namespace DroneScriptParser;

/// <summary>
/// Parses tokens from the Lexer into an Abstract Syntax Tree (AST)
/// </summary>
public class Parser
{
    private readonly List<Token> _tokens;
    private readonly List<string> _errors = new();
    private int _current = 0;

    public bool HasErrors => _errors.Count > 0;
    public IReadOnlyList<string> Errors => _errors;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    /// <summary>
    /// Parses the token stream into a Script AST
    /// </summary>
    public Script Parse()
    {
        var statements = new List<Statement>();

        // Skip leading newlines
        while (Match(TokenType.Newline)) { }

        while (!IsAtEnd())
        {
            try
            {
                var statement = ParseStatement();
                if (statement != null)
                {
                    statements.Add(statement);
                }

                // Consume newlines after statement
                while (Match(TokenType.Newline)) { }
            }
            catch (ParseException ex)
            {
                Error(ex.Message);
                Synchronize(); // Skip to next statement
            }
        }

        return new Script(statements);
    }

    /// <summary>
    /// Parses a single statement (conditional, else, or command)
    /// </summary>
    private Statement? ParseStatement()
    {
        // Route to appropriate statement parser based on current token
        if (Match(TokenType.If))
        {
            return ParseConditionalStatement();
        }
        else if (Match(TokenType.Else))
        {
            return ParseElseStatement();
        }
        else if (Check(TokenType.Identifier))
        {
            // Simple command statement
            int line = Peek().Line;
            var command = ParseCommand();
            return new CommandStatement(command, line);
        }
        else
        {
            throw new ParseException($"Expected statement (IF, ELSE, or command), got {Peek().Type}");
        }
    }

    /// <summary>
    /// Parses a conditional statement: IF condition THEN command
    /// </summary>
    private ConditionalStatement ParseConditionalStatement()
    {
        int line = Previous().Line;

        // Parse the condition
        var condition = ParseCondition();

        // Expect THEN keyword
        Consume(TokenType.Then, "Expected THEN after condition");

        // Parse the command
        var command = ParseCommand();

        return new ConditionalStatement(condition, command, line);
    }

    /// <summary>
    /// Parses an else statement: ELSE command
    /// </summary>
    private ElseStatement ParseElseStatement()
    {
        int line = Previous().Line;

        // Parse the command after ELSE
        var command = ParseCommand();

        return new ElseStatement(command, line);
    }

    /// <summary>
    /// Parses a condition (handles comparisons, queries, and logical operators)
    /// </summary>
    private Condition ParseCondition()
    {
        // Parse the left side (comparison or query)
        var left = ParseComparisonOrQuery();

        // Check for logical operators (AND/OR)
        while (Match(TokenType.And, TokenType.Or))
        {
            var operatorToken = Previous();
            var logicalOp = operatorToken.Type == TokenType.And
                ? LogicalOperator.And
                : LogicalOperator.Or;

            var right = ParseComparisonOrQuery();
            left = new LogicalCondition(left, logicalOp, right);
        }

        return left;
    }

    /// <summary>
    /// Parses a comparison condition or a simple query
    /// Examples: battery < 20 OR cargo_full
    /// </summary>
    private Condition ParseComparisonOrQuery()
    {
        // Must start with an identifier
        var identifierToken = Consume(TokenType.Identifier, "Expected identifier in condition");
        string leftSide = identifierToken.Lexeme;

        // Check if this is a comparison or just a query
        if (Match(TokenType.LessThan, TokenType.LessThanEqual,
                  TokenType.GreaterThan, TokenType.GreaterThanEqual,
                  TokenType.Equal, TokenType.NotEqual))
        {
            // It's a comparison
            var operatorToken = Previous();
            var comparisonOp = operatorToken.Type switch
            {
                TokenType.LessThan => ComparisonOperator.LessThan,
                TokenType.LessThanEqual => ComparisonOperator.LessThanEqual,
                TokenType.GreaterThan => ComparisonOperator.GreaterThan,
                TokenType.GreaterThanEqual => ComparisonOperator.GreaterThanEqual,
                TokenType.Equal => ComparisonOperator.Equal,
                TokenType.NotEqual => ComparisonOperator.NotEqual,
                _ => throw new ParseException($"Unknown comparison operator: {operatorToken.Type}")
            };

            // Parse right side (can be identifier or number)
            if (Match(TokenType.Identifier, TokenType.Number))
            {
                string rightSide = Previous().Lexeme;
                return new ComparisonCondition(leftSide, comparisonOp, rightSide);
            }
            else
            {
                throw new ParseException($"Expected identifier or number after {operatorToken.Lexeme}");
            }
        }
        else
        {
            // It's just a query (like "cargo_full" or "storm_active")
            return new QueryCondition(leftSide);
        }
    }

    /// <summary>
    /// Parses a command with optional arguments
    /// Examples: goto_charger, mine_nearest(Uranium), goto_location(10, 20)
    /// </summary>
    private Command ParseCommand()
    {
        // Get command name
        var nameToken = Consume(TokenType.Identifier, "Expected command name");
        string commandName = nameToken.Lexeme;

        var arguments = new List<CommandArgument>();

        // Check for arguments (parentheses)
        if (Match(TokenType.LeftParen))
        {
            // Parse arguments until we hit right paren
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    // Argument can be identifier or number
                    if (Match(TokenType.Identifier))
                    {
                        arguments.Add(new IdentifierArgument(Previous().Lexeme));
                    }
                    else if (Match(TokenType.Number))
                    {
                        arguments.Add(new NumberArgument(Previous().Lexeme));
                    }
                    else
                    {
                        throw new ParseException($"Expected argument (identifier or number), got {Peek().Type}");
                    }
                } while (Match(TokenType.Comma)); // Continue if we see a comma
            }

            Consume(TokenType.RightParen, "Expected ')' after command arguments");
        }

        return new Command(commandName, arguments);
    }

    // Helper methods (implemented for you)

    /// <summary>
    /// Checks if current token matches any of the given types, and consumes it if so
    /// </summary>
    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if current token is of the given type (without consuming)
    /// </summary>
    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    /// <summary>
    /// Consumes and returns the current token
    /// </summary>
    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    /// <summary>
    /// Checks if we've consumed all tokens
    /// </summary>
    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.Eof;
    }

    /// <summary>
    /// Returns the current token without consuming
    /// </summary>
    private Token Peek()
    {
        return _tokens[_current];
    }

    /// <summary>
    /// Returns the previously consumed token
    /// </summary>
    private Token Previous()
    {
        return _tokens[_current - 1];
    }

    /// <summary>
    /// Consumes a token of the expected type, or throws an error
    /// </summary>
    private Token Consume(TokenType type, string errorMessage)
    {
        if (Check(type)) return Advance();
        throw new ParseException(errorMessage + $" at line {Peek().Line}");
    }

    /// <summary>
    /// Reports a parse error
    /// </summary>
    private void Error(string message)
    {
        _errors.Add($"[Line {Peek().Line}] Parse Error: {message}");
    }

    /// <summary>
    /// Synchronizes parser state after an error (skip to next statement)
    /// </summary>
    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            // Stop at newline (next statement) or specific keywords
            if (Previous().Type == TokenType.Newline) return;
            if (Check(TokenType.If)) return;
            if (Check(TokenType.Else)) return;

            Advance();
        }
    }
}

/// <summary>
/// Exception thrown during parsing
/// </summary>
public class ParseException : Exception
{
    public ParseException(string message) : base(message) { }
}

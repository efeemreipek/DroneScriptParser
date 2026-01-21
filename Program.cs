using DroneScriptParser;
using DroneScriptParser.AST;

void TestParser(string name, string script)
{
    Console.WriteLine($"\n{'='*60}");
    Console.WriteLine($" {name}");
    Console.WriteLine($"{'='*60}");
    Console.WriteLine("\nInput:");
    Console.WriteLine(script);

    // Lex
    var lexer = new Lexer(script);
    var tokens = lexer.Tokenize();

    if (lexer.HasErrors)
    {
        Console.WriteLine("\n❌ Lexer Errors:");
        foreach (var error in lexer.Errors)
        {
            Console.WriteLine($"  {error}");
        }
        return;
    }

    // Parse
    var parser = new Parser(tokens);
    var ast = parser.Parse();

    if (parser.HasErrors)
    {
        Console.WriteLine("\n❌ Parser Errors:");
        foreach (var error in parser.Errors)
        {
            Console.WriteLine($"  {error}");
        }
    }
    else
    {
        Console.WriteLine("\n✓ Parse successful!");
        Console.WriteLine($"\nAST: {ast}");
        Console.WriteLine("\nStatements:");
        foreach (var statement in ast.Statements)
        {
            PrintStatement(statement, indent: 2);
        }
    }
}

void PrintStatement(Statement statement, int indent)
{
    var prefix = new string(' ', indent);

    switch (statement)
    {
        case ConditionalStatement conditional:
            Console.WriteLine($"{prefix}IF {FormatCondition(conditional.Condition)}");
            Console.WriteLine($"{prefix}  THEN {FormatCommand(conditional.ThenCommand)}");
            break;
        case ElseStatement elseStmt:
            Console.WriteLine($"{prefix}ELSE {FormatCommand(elseStmt.ElseCommand)}");
            break;
        case CommandStatement cmdStmt:
            Console.WriteLine($"{prefix}{FormatCommand(cmdStmt.Command)}");
            break;
    }
}

string FormatCondition(Condition condition)
{
    return condition switch
    {
        ComparisonCondition comp => $"{comp.Left} {FormatOperator(comp.Operator)} {comp.Right}",
        QueryCondition query => query.QueryName,
        LogicalCondition logical => $"({FormatCondition(logical.Left)} {logical.Operator.ToString().ToUpper()} {FormatCondition(logical.Right)})",
        _ => condition.ToString()
    };
}

string FormatOperator(ComparisonOperator op)
{
    return op switch
    {
        ComparisonOperator.LessThan => "<",
        ComparisonOperator.LessThanEqual => "<=",
        ComparisonOperator.GreaterThan => ">",
        ComparisonOperator.GreaterThanEqual => ">=",
        ComparisonOperator.Equal => "==",
        ComparisonOperator.NotEqual => "!=",
        _ => op.ToString()
    };
}

string FormatCommand(Command command)
{
    if (command.Arguments.Count == 0)
        return command.Name;

    var args = string.Join(", ", command.Arguments.Select(arg => arg switch
    {
        IdentifierArgument id => id.Value,
        NumberArgument num => num.Value,
        _ => arg.ToString()
    }));

    return $"{command.Name}({args})";
}

void TestLexer(string name, string script)
{
    Console.WriteLine($"\n{'='} {name} {'='}");
    Console.WriteLine("\nInput:");
    Console.WriteLine(script);

    var lexer = new Lexer(script);
    var tokens = lexer.Tokenize();

    if (lexer.HasErrors)
    {
        Console.WriteLine("\n❌ Errors:");
        foreach (var error in lexer.Errors)
        {
            Console.WriteLine($"  {error}");
        }
    }
    else
    {
        Console.WriteLine("\n✓ No errors");
    }

    Console.WriteLine($"\nTokens ({tokens.Count}):");
    foreach (var token in tokens)
    {
        Console.WriteLine($"  {token}");
    }
}

Console.WriteLine("\n🚀 DroneScript Parser Tests\n");

// Test 1: Simple conditional
TestParser("Test 1: Simple Conditional", @"
IF battery < 20 THEN goto_charger
");

// Test 2: Multiple conditionals with ELSE
TestParser("Test 2: Conditionals with ELSE", @"
IF battery < 20 THEN goto_charger
IF cargo_full THEN goto_outpost
ELSE mine_nearest(Uranium)
");

// Test 3: Logical operators
TestParser("Test 3: Logical AND/OR", @"
IF battery < 20 AND cargo_full THEN goto_outpost
IF hp < 50 OR in_hazard_zone THEN goto_outpost
");

// Test 4: Commands with arguments
TestParser("Test 4: Commands with Arguments", @"
mine_nearest(Uranium)
goto_location(10, 20)
patrol(0, 0, 100, 100)
wait(30)
");

// Test 5: Query conditions
TestParser("Test 5: Query Conditions", @"
IF storm_active THEN goto_outpost
IF cargo_full THEN deposit
");

// Test 6: Complex script
TestParser("Test 6: Complex Script", @"
# Defensive miner
IF battery < 15 THEN goto_charger
IF hp < 40 THEN goto_outpost
IF storm_active THEN goto_outpost
IF in_hazard_zone AND hp < 80 THEN goto_outpost
IF cargo_full THEN goto_outpost
mine_nearest(Uranium)
ELSE mine_nearest(Titanium)
ELSE mine_nearest(any)
");

// Test 7: All comparison operators
TestParser("Test 7: All Operators", @"
IF battery < 20 THEN goto_charger
IF battery <= 20 THEN goto_charger
IF battery > 80 THEN mine_nearest(Iron)
IF battery >= 80 THEN mine_nearest(Iron)
IF battery == 50 THEN wait(10)
IF battery != 100 THEN goto_charger
");

// Test 8: Error - Missing THEN
TestParser("Test 8: Error - Missing THEN", @"
IF battery < 20 goto_charger
");
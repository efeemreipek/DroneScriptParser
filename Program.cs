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

void TestInterpreter(string name, string script, DroneState state)
{
    Console.WriteLine($"\n{'='*60}");
    Console.WriteLine($" {name}");
    Console.WriteLine($"{'='*60}");
    Console.WriteLine($"\nDrone State: {state}");
    Console.WriteLine("\nScript:");
    Console.WriteLine(script);

    // Lex and Parse
    var lexer = new Lexer(script);
    var tokens = lexer.Tokenize();

    if (lexer.HasErrors)
    {
        Console.WriteLine("\n❌ Lexer Errors:");
        foreach (var error in lexer.Errors)
            Console.WriteLine($"  {error}");
        return;
    }

    var parser = new Parser(tokens);
    var ast = parser.Parse();

    if (parser.HasErrors)
    {
        Console.WriteLine("\n❌ Parser Errors:");
        foreach (var error in parser.Errors)
            Console.WriteLine($"  {error}");
        return;
    }

    // Execute
    var interpreter = new Interpreter(state);
    interpreter.ExecuteScript(ast);

    Console.WriteLine("\n📋 Execution Log:");
    foreach (var log in interpreter.ExecutionLog)
    {
        Console.WriteLine($"  {log}");
    }
}

Console.WriteLine("\n🚀 DroneScript Interpreter Tests\n");

var defensiveMinerScript = @"
# Defensive miner script
IF battery < 15 THEN goto_charger
IF hp < 40 THEN goto_outpost
IF storm_active THEN goto_outpost
IF in_hazard_zone AND hp < 80 THEN goto_outpost
IF cargo_full THEN goto_outpost
mine_nearest(Uranium)
ELSE mine_nearest(Titanium)
ELSE mine_nearest(any)
";

// Test 1: Low battery scenario
TestInterpreter("Test 1: Low Battery", defensiveMinerScript, new DroneState
{
    Battery = 12,  // Low battery!
    HP = 100,
    CargoAmount = 3,
    MaxCargo = 10
});

// Test 2: Low HP scenario
TestInterpreter("Test 2: Low HP", defensiveMinerScript, new DroneState
{
    Battery = 80,
    HP = 35,  // Low HP!
    CargoAmount = 5,
    MaxCargo = 10
});

// Test 3: Storm active scenario
TestInterpreter("Test 3: Storm Active", defensiveMinerScript, new DroneState
{
    Battery = 80,
    HP = 100,
    StormActive = true  // Storm!
});

// Test 4: In hazard zone with medium HP
TestInterpreter("Test 4: Hazard Zone", defensiveMinerScript, new DroneState
{
    Battery = 80,
    HP = 75,  // Below 80 while in hazard
    InHazardZone = true
});

// Test 5: Cargo full scenario
TestInterpreter("Test 5: Cargo Full", defensiveMinerScript, new DroneState
{
    Battery = 80,
    HP = 100,
    CargoAmount = 10,  // Full cargo!
    MaxCargo = 10
});

// Test 6: Normal operation - should mine
TestInterpreter("Test 6: Normal Mining", defensiveMinerScript, new DroneState
{
    Battery = 80,
    HP = 100,
    CargoAmount = 3,
    MaxCargo = 10
});

// Test 7: No Uranium available - should fall back to Titanium
var state7 = new DroneState
{
    Battery = 80,
    HP = 100,
    CargoAmount = 3,
    MaxCargo = 10
};
state7.NearbyResources["Uranium"] = false;  // No Uranium!
state7.NearbyResources["Titanium"] = true;

TestInterpreter("Test 7: Resource Fallback", defensiveMinerScript, state7);

// Test 8: Logical operators
TestInterpreter("Test 8: Logical AND", @"
IF battery < 20 AND cargo_full THEN goto_outpost
mine_nearest(Iron)
", new DroneState
{
    Battery = 15,      // < 20
    CargoAmount = 10,  // Full
    MaxCargo = 10
});

// Test 9: Logical OR
TestInterpreter("Test 9: Logical OR", @"
IF hp < 50 OR in_hazard_zone THEN goto_outpost
mine_nearest(Iron)
", new DroneState
{
    Battery = 80,
    HP = 100,          // > 50
    InHazardZone = true  // But in hazard!
});

// Test 10: All comparison operators
TestInterpreter("Test 10: Comparison Operators", @"
IF battery < 20 THEN goto_charger
IF battery > 80 THEN mine_nearest(Iron)
IF battery >= 80 THEN wait(10)
IF battery == 80 THEN deposit
", new DroneState
{
    Battery = 80  // Exactly 80
});
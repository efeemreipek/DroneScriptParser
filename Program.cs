using DroneScriptParser;

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

// Test 1: Valid script with all features
TestLexer("Test 1: Valid Script", @"
# Defensive miner script
IF battery < 20 THEN goto_charger
IF cargo_full THEN goto_outpost
mine_nearest(Uranium)
");

// Test 2: All operators
TestLexer("Test 2: All Operators", @"
IF battery < 20 THEN goto_charger
IF battery <= 20 THEN goto_charger
IF battery > 80 THEN mine_nearest(Iron)
IF battery >= 80 THEN mine_nearest(Iron)
IF battery == 50 THEN wait(10)
IF battery != 100 THEN goto_charger
");

// Test 3: Floating point numbers
TestLexer("Test 3: Float Numbers", @"
goto_location(10.5, 20.75)
patrol(0.0, 0.0, 100.0, 100.0)
");

// Test 4: Error - Single equals sign
TestLexer("Test 4: Single Equals Error", @"
IF battery = 20 THEN goto_charger
");

// Test 5: Error - Single exclamation mark
TestLexer("Test 5: Single Exclamation Error", @"
IF !cargo_full THEN goto_outpost
");

// Test 6: Error - Invalid characters
TestLexer("Test 6: Invalid Characters", @"
mine_nearest(Iron)
@invalid $variable %test
");

// Test 7: Case insensitive keywords
TestLexer("Test 7: Case Insensitive Keywords", @"
if battery < 20 then goto_charger
IF battery < 20 THEN goto_charger
If battery < 20 Then goto_charger
");
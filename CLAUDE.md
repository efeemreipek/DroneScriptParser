# DroneScriptParser

## Project Overview

This is a C# console application prototype for the **DroneScript** parsing and interpretation system that will be used in the **Mining Swarm** game. This standalone parser serves as a mockup/proof-of-concept to validate the DroneScript language design before integrating it into the Unity game engine.

## About Mining Swarm

**Mining Swarm** is an automation-focused resource management game where players command a fleet of autonomous mining drones on a hostile alien planet. The core gameplay mechanic is programming drone behavior using a simple scripting language rather than direct control.

## What is DroneScript?

DroneScript is a simple, code-lite scripting language that allows players to define autonomous drone behavior. It uses natural language commands with programming-like control flow.

### Design Philosophy

- **For Coders**: Familiar IF-THEN logic, optimization challenges
- **For Non-Coders**: Natural language commands, no complex syntax
- **Simple Structure**: One command per line, no variables, no functions, no nested loops
- **Clear Execution**: Top-to-bottom evaluation once per second per drone

### Example DroneScript

```dronescript
# Defensive Miner Script
MINER:
  # Safety first
  IF battery < 15 THEN goto_charger
  IF hp < 40 THEN goto_outpost
  IF storm_active THEN goto_outpost
  IF in_hazard_zone AND hp < 80 THEN goto_outpost

  # Mining priorities
  IF cargo_full THEN goto_outpost
  MINE nearest(Uranium)
  ELSE MINE nearest(Titanium)
  ELSE MINE nearest(any)
```

### Language Features

**Movement Commands:**
- `goto_charger` - Path to nearest charging station
- `goto_outpost` - Path to nearest outpost
- `goto_location(x, y)` - Path to specific coordinates
- `patrol(x1, y1, x2, y2)` - Move between waypoints in loop
- `explore` - Auto-explore unseen areas

**Action Commands:**
- `mine_nearest(resource_type)` - Mine closest deposit (Iron, Copper, Silicon, Carbon, Titanium, Uranium)
- `mine_nearest(any)` - Mine any nearby resource
- `haul_to(destination)` - Transport cargo to outpost/refinery
- `repair(target)` - Repair specified drone
- `repair_nearest` - Repair closest damaged drone
- `wait(seconds)` - Idle for specified time
- `deposit` - Deposit cargo at current outpost

**Query Commands (used in conditions):**
- `battery < X` - Check battery level (0-100)
- `hp < X` - Check health (0-100)
- `cargo_full` - Check if cargo at capacity
- `storm_active` - Check if dust storm occurring
- `in_hazard_zone` - Check if inside toxic/solar zone
- `nearby_resource(type)` - Check if resource within 20 units
- `nearby_charger` - Check if charger within range
- `nearby_damaged_drone` - Check if damaged drone within 20 units

**Control Flow:**
- `IF condition THEN command` - Conditional execution
- `ELSE command` - Fallback if condition false
- `AND` / `OR` - Logical operators for complex conditions
- Comments start with `#`
- Implicit loop (script restarts from top after reaching end)

## Parser Goals

This console application aims to:

1. **Validate Syntax Design**: Ensure the language is easy to parse and unambiguous
2. **Implement Core Components**:
   - **Lexer**: Tokenize script text into commands, keywords, operators, values
   - **Parser**: Validate syntax and build Abstract Syntax Tree (AST) or execution structure
   - **Interpreter**: Execute commands and evaluate conditions
   - **Error Handling**: Provide clear, user-friendly error messages
3. **Test Edge Cases**: Invalid syntax, nested conditionals, command combinations
4. **Performance Testing**: Ensure script execution is fast enough for 50+ drones running scripts simultaneously
5. **Prototype Features**:
   - Syntax validation
   - Real-time error checking
   - Command execution simulation
   - Script state tracking

## Technical Requirements

### Core Components

**1. Lexer (Tokenizer)**
- Splits input text into tokens (keywords, identifiers, operators, literals)
- Ignores whitespace (except line breaks)
- Strips comments (`#` to end of line)
- Recognizes:
  - Keywords: `IF`, `THEN`, `ELSE`, `AND`, `OR`
  - Commands: `goto_charger`, `mine_nearest`, etc.
  - Operators: `<`, `>`, `==`, `!=`
  - Values: Numbers, resource types, coordinates
  - Drone types: `MINER`, `HAULER`, `SCOUT`, `REPAIR`

**2. Parser (Syntax Validator)**
- Validates token sequences against grammar rules
- Builds execution tree or command list
- Detects syntax errors:
  - Invalid command names
  - Missing THEN after IF
  - Malformed function calls (missing parentheses, wrong argument count)
  - Invalid conditional operators
- Enforces constraints:
  - Max 100 lines per script
  - Max 10 nested IFs
  - One command per line

**3. Interpreter (Executor)**
- Executes parsed commands in sequence
- Evaluates IF conditions (short-circuit evaluation)
- Simulates drone state:
  - Battery level (0-100)
  - HP (0-100)
  - Cargo amount
  - Position (x, y)
  - Environmental flags (storm_active, in_hazard_zone, etc.)
- Execution model:
  - Top-to-bottom evaluation
  - First matching condition executes
  - After action command, script evaluation ends (waits for next tick)
  - Implicit loop back to top

**4. Error Handler**
- Line number tracking
- Clear error messages:
  - "Syntax Error on line 5: Expected THEN after IF condition"
  - "Unknown command 'min_nearest' on line 8. Did you mean 'mine_nearest'?"
  - "Invalid argument 'Gol' for mine_nearest on line 12. Valid resources: Iron, Copper, Silicon, Carbon, Titanium, Uranium, any"
- Suggestion system for typos (Levenshtein distance for command matching)

### Execution Model

**Per-Drone State:**
```csharp
class DroneState {
    float Battery;         // 0-100
    float HP;             // 0-100
    float CargoAmount;    // 0-max_capacity
    float MaxCargo;       // 10 for Miner Mk1, 30 for Hauler Mk1
    Vector2 Position;     // (x, y) coordinates
    bool StormActive;
    bool InHazardZone;
    // ... other state
}
```

**Script Execution Loop:**
```csharp
// Once per second (in Unity, this would be in Update with timer)
void ExecuteScript(DroneState state, Script script) {
    foreach (var line in script.Lines) {
        if (line is ConditionalLine conditional) {
            if (EvaluateCondition(conditional.Condition, state)) {
                ExecuteCommand(conditional.Command, state);
                return; // Stop after first command executes
            }
        } else if (line is CommandLine command) {
            ExecuteCommand(command.Command, state);
            return;
        }
    }
    // If reached end without executing anything, loop back (but that happens next second)
}
```

## Implementation Phases

### Phase 1: Basic Lexer
- Tokenize simple commands (`goto_charger`, `mine_nearest(Iron)`)
- Handle comments
- Output token stream for debugging

### Phase 2: Simple Parser
- Parse single commands (no conditionals)
- Validate command names and arguments
- Build command objects

### Phase 3: Conditional Logic
- Add IF-THEN-ELSE parsing
- Support comparison operators (`<`, `>`, `==`)
- Handle AND/OR logical operators

### Phase 4: Interpreter
- Simulate drone state
- Execute commands (print simulation output)
- Evaluate conditions against state

### Phase 5: Error Handling
- Line number tracking
- Error messages with suggestions
- Handle edge cases (empty scripts, only comments, malformed syntax)

### Phase 6: Testing & Validation
- Test all 20 commands
- Test complex conditionals
- Test error cases
- Performance testing (parse 1000 scripts, measure time)

## Success Criteria

This prototype is successful if:

1. **Syntax is Unambiguous**: No cases where script can be interpreted multiple ways
2. **Error Messages are Clear**: Non-programmers can understand what went wrong
3. **Performance is Adequate**: Can parse and execute scripts for 50 drones at 60 FPS (< 16ms total for all scripts)
4. **Edge Cases Handled**: Malformed scripts don't crash, they return useful errors
5. **Ready for Unity Integration**: Parser can be ported to Unity with minimal changes

## Next Steps (After Prototype)

Once this console parser is validated:

1. Port to Unity (C# scripts in Unity project)
2. Build text editor UI with syntax highlighting (TextMeshPro with color tags)
3. Add auto-complete system (suggest commands as user types)
4. Integrate with drone AI state machine (parser output drives drone behavior)
5. Add script export/import functionality
6. Build script execution trace/debugger UI

## Development Guidelines

### Code Style
- Use modern C# features (pattern matching, records, nullable reference types)
- Prefer immutability where possible
- Use descriptive variable names
- Add XML documentation comments for public APIs

### Testing
- Unit test each component (Lexer, Parser, Interpreter) independently
- Integration tests for full script parsing
- Test error cases as thoroughly as happy paths

### Performance
- Avoid LINQ in hot paths (script execution happens every frame)
- Cache parsed scripts (don't re-parse on every execution)
- Use spans/memory-efficient string operations where possible

## Project Structure (Suggested)

```
DroneScriptParser/
├── Core/
│   ├── Lexer.cs           # Tokenization
│   ├── Parser.cs          # Syntax validation, AST building
│   ├── Interpreter.cs     # Command execution
│   ├── Token.cs           # Token types and data
│   ├── AST/               # Abstract Syntax Tree node types
│   │   ├── Command.cs
│   │   ├── Condition.cs
│   │   └── Expression.cs
│   └── Errors/
│       └── ScriptError.cs # Error types and formatting
├── DroneState/
│   └── DroneState.cs      # Simulated drone state for testing
├── Commands/
│   ├── ICommand.cs        # Command interface
│   ├── MovementCommands.cs
│   ├── ActionCommands.cs
│   └── QueryCommands.cs
├── Tests/
│   ├── LexerTests.cs
│   ├── ParserTests.cs
│   ├── InterpreterTests.cs
│   └── IntegrationTests.cs
└── Program.cs             # Console app entry point
```

## Example Usage (Console App)

```csharp
// In Program.cs
var scriptText = @"
MINER:
  IF battery < 20 THEN goto_charger
  IF cargo_full THEN goto_outpost
  MINE nearest(Uranium)
  ELSE MINE nearest(any)
";

var lexer = new Lexer(scriptText);
var tokens = lexer.Tokenize();

var parser = new Parser(tokens);
var script = parser.Parse();

if (parser.HasErrors) {
    foreach (var error in parser.Errors) {
        Console.WriteLine(error);
    }
    return;
}

var droneState = new DroneState {
    Battery = 45,
    HP = 100,
    CargoAmount = 0,
    MaxCargo = 10,
    Position = new Vector2(10, 10)
};

var interpreter = new Interpreter(droneState);
interpreter.ExecuteScript(script);

// Output: "Executing: MINE nearest(Uranium)"
// Or: "No Uranium nearby, executing: MINE nearest(any)"
```

## Notes

- This is a **prototype** - focus is on validating design, not production-quality code
- Performance optimization can wait until Unity integration
- Keep the language simple - resist feature creep (no variables, no functions, no loops beyond implicit loop)
- Test with the example scripts from the GDD to ensure they parse correctly

---

## Extending DroneScript

The parser is designed to be extensible. Here's how to add new features:

### Adding New Commands (Easy - 1 file)

Commands like `goto_warehouse`, `scan_area`, `broadcast_message` are the easiest to add.

**Changes needed: `Interpreter.cs` only**

Add a new case in the `ExecuteCommand()` method's switch statement:

```csharp
case "goto_warehouse":
    Log($"    [Simulation] Drone pathfinding to nearest warehouse...");
    break;
```

**Example:** After adding this, the script `IF cargo_full THEN goto_warehouse` works immediately.

**No changes needed to:**
- Lexer (commands are just identifiers)
- Parser (parses any identifier as a command)
- Token types

---

### Adding New Query Conditions (2 files)

Queries are boolean checks like `cargo_full`, `storm_active`, `low_fuel`.

**Changes needed:**

**1. `DroneState.cs` - Add the query logic:**

```csharp
// Add computed property
public bool LowFuel => Battery < 20;

// Update GetQueryValue() method
public bool GetQueryValue(string queryName)
{
    return queryName.ToLower() switch
    {
        "cargo_full" => CargoFull,
        "low_fuel" => LowFuel,  // ← Add this line
        "storm_active" => StormActive,
        // ... rest
    };
}
```

**2. (Optional) Add state property if needed:**

```csharp
public float FuelLevel { get; set; } = 100f;
```

**Example:** Now `IF low_fuel THEN goto_refuel` works!

---

### Adding New Variables (2 files)

Variables are values used in comparisons like `battery`, `hp`, `cargo`.

**Changes needed:**

**1. `DroneState.cs` - Add property:**

```csharp
public float FuelLevel { get; set; } = 100f;
```

**2. `DroneState.cs` - Update `GetVariableValue()`:**

```csharp
public float GetVariableValue(string variableName)
{
    return variableName.ToLower() switch
    {
        "battery" => Battery,
        "hp" => HP,
        "cargo" => CargoAmount,
        "fuel" => FuelLevel,  // ← Add this line
        _ => throw new InvalidOperationException($"Unknown variable: {variableName}")
    };
}
```

**Example:** Now `IF fuel < 10 THEN goto_refuel` works!

---

### Adding New Operators (Complex - 5 files)

**Example: Adding modulo operator `%`**

This is more involved because operators are part of the grammar.

**1. `Token.cs` - Add token type:**
```csharp
public enum TokenType
{
    // ... existing operators
    Modulo,  // %
}
```

**2. `Lexer.cs` - Tokenize the operator:**
```csharp
case '%':
    AddToken(TokenType.Modulo);
    break;
```

**3. `AST/Condition.cs` - Add to ComparisonOperator enum:**
```csharp
public enum ComparisonOperator
{
    // ... existing
    Modulo  // %
}
```

**4. `Parser.cs` - Handle in `ParseComparisonOrQuery()`:**
```csharp
if (Match(TokenType.LessThan, TokenType.LessThanEqual,
          TokenType.GreaterThan, TokenType.GreaterThanEqual,
          TokenType.Equal, TokenType.NotEqual,
          TokenType.Modulo))  // ← Add this
{
    var operatorToken = Previous();
    var comparisonOp = operatorToken.Type switch
    {
        // ... existing mappings
        TokenType.Modulo => ComparisonOperator.Modulo,
        // ...
    };
}
```

**5. `Interpreter.cs` - Implement evaluation in `EvaluateComparison()`:**
```csharp
bool result = comparison.Operator switch
{
    // ... existing operators
    ComparisonOperator.Modulo => (int)leftValue % (int)rightValue == 0,
    // ...
};
```

**6. `Interpreter.cs` - Add to `FormatOperator()` for display:**
```csharp
private string FormatOperator(ComparisonOperator op)
{
    return op switch
    {
        // ... existing
        ComparisonOperator.Modulo => "%",
        // ...
    };
}
```

---

### Adding New Keywords (Very Complex - 5+ files)

**Example: Adding `WHILE` loops**

Adding keywords like `WHILE`, `FOR`, `BREAK` requires extensive changes.

**1. `Token.cs` - Add keyword token:**
```csharp
public enum TokenType
{
    // ... existing keywords
    While,  // WHILE
}
```

**2. `Lexer.cs` - Register keyword:**
```csharp
private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.OrdinalIgnoreCase)
{
    // ... existing
    { "while", TokenType.While }
};
```

**3. `AST/Statement.cs` - Add new statement type:**
```csharp
public record WhileStatement(
    Condition Condition,
    List<Command> Body,
    int Line
) : Statement;
```

**4. `Parser.cs` - Add parsing logic:**
```csharp
private Statement? ParseStatement()
{
    if (Match(TokenType.While))
    {
        return ParseWhileStatement();
    }
    // ... rest
}

private WhileStatement ParseWhileStatement()
{
    int line = Previous().Line;
    var condition = ParseCondition();
    // Parse body commands...
    return new WhileStatement(condition, body, line);
}
```

**5. `Interpreter.cs` - Add execution logic:**
```csharp
private bool ExecuteStatement(Statement statement)
{
    switch (statement)
    {
        // ... existing cases
        case WhileStatement whileStmt:
            while (EvaluateCondition(whileStmt.Condition))
            {
                foreach (var cmd in whileStmt.Body)
                    ExecuteCommand(cmd);
            }
            return true;
    }
}
```

**Note:** Adding loops violates the GDD's "no loops beyond implicit loop" design principle. This is just an example of the complexity involved.

---

### Quick Reference: Complexity Chart

| Feature Type | Files to Change | Difficulty | Est. Time |
|--------------|-----------------|------------|-----------|
| **New Command** | `Interpreter.cs` (1) | ⭐ Easy | 5 min |
| **New Query** | `DroneState.cs` (1) | ⭐ Easy | 10 min |
| **New Variable** | `DroneState.cs` (1) | ⭐ Easy | 10 min |
| **New Operator** | 5 files | ⭐⭐⭐ Hard | 30 min |
| **New Keyword** | 5+ files | ⭐⭐⭐⭐ Very Hard | 1-2 hours |
| **New Argument Type** | 3 files | ⭐⭐ Medium | 20 min |

---

### Best Practices for Extensions

**✅ DO:**
- Add new commands freely - they're designed to be extensible
- Test new features with similar test patterns from `Program.cs`
- Add helpful log messages in the Interpreter for debugging
- Keep commands lowercase with underscores (`goto_warehouse`, not `gotoWarehouse`)
- Update test scripts to verify new features work correctly
- Add XML documentation comments for new public methods

**❌ DON'T:**
- Add operators/keywords unless absolutely necessary (they're complex to maintain)
- Hardcode command names in the Parser (keep it generic)
- Forget to handle argument counts in Interpreter (`command.Arguments.Count` checks)
- Skip error handling (queries/variables should throw helpful errors for unknown names)
- Break case-insensitivity (always use `.ToLower()` in switch statements)

---

### Design Philosophy

**Data-Driven Design:** DroneScript separates *syntax* (handled by Lexer/Parser) from *semantics* (handled by Interpreter). This means:
- Commands are just identifiers to the parser - it doesn't validate if `goto_warehouse` is a "real" command
- Only the Interpreter knows what commands do
- Adding new commands = teaching the Interpreter, not changing the language grammar

This design makes the system highly extensible for game development where new drone behaviors are frequently added.

**Why This Matters:** In Unity, you can add 50 new drone commands without touching the parser. The parser's job is to understand structure (`IF condition THEN command`), not to validate game logic.

using DroneScriptParser.AST;

namespace DroneScriptParser;

/// <summary>
/// Interprets and executes DroneScript AST
/// </summary>
public class Interpreter
{
    private readonly DroneState _state;
    private readonly List<string> _executionLog = new();

    public IReadOnlyList<string> ExecutionLog => _executionLog;

    public Interpreter(DroneState state)
    {
        _state = state;
    }

    /// <summary>
    /// Executes a script by evaluating statements in order
    /// Following DroneScript semantics: execute first matching condition, then stop
    /// </summary>
    public void ExecuteScript(Script script)
    {
        _executionLog.Clear();
        Log($"=== Executing script with {_state} ===");

        foreach (var statement in script.Statements)
        {
            if (ExecuteStatement(statement))
            {
                // Statement executed an action, stop here (until next tick)
                Log("--- Execution complete (waiting for next tick) ---");
                return;
            }
        }

        Log("--- No command executed (end of script) ---");
    }

    /// <summary>
    /// Executes a single statement
    /// Returns true if an action was executed (to stop script evaluation)
    /// </summary>
    private bool ExecuteStatement(Statement statement)
    {
        switch (statement)
        {
            case ConditionalStatement conditional:
                Log($"Evaluating: IF {FormatCondition(conditional.Condition)}");
                if (EvaluateCondition(conditional.Condition))
                {
                    Log($"  ✓ Condition TRUE");
                    ExecuteCommand(conditional.ThenCommand);
                    return true; // Action executed, stop
                }
                else
                {
                    Log($"  ✗ Condition FALSE");
                }
                break;

            case ElseStatement elseStmt:
                Log($"Executing: ELSE {FormatCommand(elseStmt.ElseCommand)}");
                ExecuteCommand(elseStmt.ElseCommand);
                return true; // Action executed, stop

            case CommandStatement cmdStmt:
                Log($"Executing: {FormatCommand(cmdStmt.Command)}");
                ExecuteCommand(cmdStmt.Command);
                return true; // Action executed, stop
        }

        return false; // No action executed, continue to next statement
    }

    /// <summary>
    /// Evaluates a condition to true or false
    /// </summary>
    private bool EvaluateCondition(Condition condition)
    {
        return condition switch
        {
            ComparisonCondition comp => EvaluateComparison(comp),
            QueryCondition query => EvaluateQuery(query),
            LogicalCondition logical => EvaluateLogical(logical),
            _ => throw new InvalidOperationException($"Unknown condition type: {condition.GetType()}")
        };
    }

    /// <summary>
    /// Evaluates a comparison condition (e.g., battery < 20)
    /// </summary>
    private bool EvaluateComparison(ComparisonCondition comparison)
    {
        // Get left side value (should be a variable like "battery", "hp")
        float leftValue = _state.GetVariableValue(comparison.Left);

        // Get right side value (could be a number or another variable)
        float rightValue;
        if (float.TryParse(comparison.Right, out float parsedValue))
        {
            rightValue = parsedValue;
        }
        else
        {
            // It's a variable name
            rightValue = _state.GetVariableValue(comparison.Right);
        }

        // Perform comparison
        bool result = comparison.Operator switch
        {
            ComparisonOperator.LessThan => leftValue < rightValue,
            ComparisonOperator.LessThanEqual => leftValue <= rightValue,
            ComparisonOperator.GreaterThan => leftValue > rightValue,
            ComparisonOperator.GreaterThanEqual => leftValue >= rightValue,
            ComparisonOperator.Equal => Math.Abs(leftValue - rightValue) < 0.001f, // Float equality with epsilon
            ComparisonOperator.NotEqual => Math.Abs(leftValue - rightValue) >= 0.001f,
            _ => throw new InvalidOperationException($"Unknown operator: {comparison.Operator}")
        };

        Log($"    Comparison: {comparison.Left}({leftValue}) {FormatOperator(comparison.Operator)} {comparison.Right}({rightValue}) = {result}");
        return result;
    }

    /// <summary>
    /// Evaluates a query condition (e.g., cargo_full, storm_active)
    /// </summary>
    private bool EvaluateQuery(QueryCondition query)
    {
        bool result = _state.GetQueryValue(query.QueryName);
        Log($"    Query: {query.QueryName} = {result}");
        return result;
    }

    /// <summary>
    /// Evaluates a logical condition (AND/OR)
    /// </summary>
    private bool EvaluateLogical(LogicalCondition logical)
    {
        bool leftResult = EvaluateCondition(logical.Left);

        // Short-circuit evaluation
        if (logical.Operator == LogicalOperator.And && !leftResult)
        {
            Log($"    Logical: AND short-circuit (left is false)");
            return false;
        }

        if (logical.Operator == LogicalOperator.Or && leftResult)
        {
            Log($"    Logical: OR short-circuit (left is true)");
            return true;
        }

        bool rightResult = EvaluateCondition(logical.Right);

        bool result = logical.Operator switch
        {
            LogicalOperator.And => leftResult && rightResult,
            LogicalOperator.Or => leftResult || rightResult,
            _ => throw new InvalidOperationException($"Unknown logical operator: {logical.Operator}")
        };

        Log($"    Logical: {leftResult} {logical.Operator.ToString().ToUpper()} {rightResult} = {result}");
        return result;
    }

    /// <summary>
    /// Executes a command (simulated for now)
    /// In the real game, this would trigger actual drone behavior
    /// </summary>
    private void ExecuteCommand(Command command)
    {
        string formattedCmd = FormatCommand(command);
        Log($"  → Executing command: {formattedCmd}");

        // Simulate command effects
        switch (command.Name.ToLower())
        {
            case "goto_charger":
                Log($"    [Simulation] Drone pathfinding to nearest charger...");
                break;

            case "goto_outpost":
                Log($"    [Simulation] Drone pathfinding to nearest outpost...");
                break;

            case "goto_location":
                if (command.Arguments.Count >= 2)
                {
                    var x = GetArgumentValue(command.Arguments[0]);
                    var y = GetArgumentValue(command.Arguments[1]);
                    Log($"    [Simulation] Drone pathfinding to ({x}, {y})...");
                }
                break;

            case "mine_nearest":
                if (command.Arguments.Count > 0)
                {
                    var resource = GetArgumentValue(command.Arguments[0]);
                    Log($"    [Simulation] Searching for nearest {resource} deposit...");

                    if (_state.NearbyResources.GetValueOrDefault(resource, false))
                    {
                        Log($"    [Simulation] Found {resource}! Starting mining...");
                    }
                    else
                    {
                        Log($"    [Simulation] No {resource} deposits nearby.");
                    }
                }
                break;

            case "patrol":
                if (command.Arguments.Count >= 4)
                {
                    Log($"    [Simulation] Starting patrol route...");
                }
                break;

            case "wait":
                if (command.Arguments.Count > 0)
                {
                    var seconds = GetArgumentValue(command.Arguments[0]);
                    Log($"    [Simulation] Waiting for {seconds} seconds...");
                }
                break;

            case "deposit":
                Log($"    [Simulation] Depositing {_state.CargoAmount} units at outpost...");
                _state.CargoAmount = 0;
                break;

            case "explore":
                Log($"    [Simulation] Exploring unseen areas...");
                break;

            case "repair_nearest":
                Log($"    [Simulation] Repairing nearest damaged drone...");
                break;

            default:
                Log($"    [Warning] Unknown command: {command.Name}");
                break;
        }
    }

    /// <summary>
    /// Gets the string value of a command argument
    /// </summary>
    private string GetArgumentValue(CommandArgument arg)
    {
        return arg switch
        {
            IdentifierArgument id => id.Value,
            NumberArgument num => num.Value,
            _ => arg.ToString() ?? ""
        };
    }

    // Formatting helpers
    private string FormatCondition(Condition condition)
    {
        return condition switch
        {
            ComparisonCondition comp => $"{comp.Left} {FormatOperator(comp.Operator)} {comp.Right}",
            QueryCondition query => query.QueryName,
            LogicalCondition logical => $"({FormatCondition(logical.Left)} {logical.Operator.ToString().ToUpper()} {FormatCondition(logical.Right)})",
            _ => condition.ToString() ?? ""
        };
    }

    private string FormatOperator(ComparisonOperator op)
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

    private string FormatCommand(Command command)
    {
        if (command.Arguments.Count == 0)
            return command.Name;

        var args = string.Join(", ", command.Arguments.Select(GetArgumentValue));
        return $"{command.Name}({args})";
    }

    private void Log(string message)
    {
        _executionLog.Add(message);
    }
}

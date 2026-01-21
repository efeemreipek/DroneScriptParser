namespace DroneScriptParser;

/// <summary>
/// Represents the state of a drone for script execution simulation
/// </summary>
public class DroneState
{
    // Core state
    public float Battery { get; set; } = 100f;        // 0-100
    public float HP { get; set; } = 100f;             // 0-100
    public float CargoAmount { get; set; } = 0f;
    public float MaxCargo { get; set; } = 10f;        // Miner Mk1 default
    public (float X, float Y) Position { get; set; } = (0, 0);

    // Environmental state
    public bool StormActive { get; set; } = false;
    public bool InHazardZone { get; set; } = false;

    // Query states
    public bool CargoFull => CargoAmount >= MaxCargo;
    public bool NearbyCharger { get; set; } = false;
    public bool NearbyDamagedDrone { get; set; } = false;

    // Resource tracking (for simulation)
    public Dictionary<string, bool> NearbyResources { get; set; } = new();

    public DroneState()
    {
        // Initialize with some nearby resources for testing
        NearbyResources["Iron"] = true;
        NearbyResources["Copper"] = true;
        NearbyResources["any"] = true;
    }

    /// <summary>
    /// Gets a query value by name
    /// </summary>
    public bool GetQueryValue(string queryName)
    {
        return queryName.ToLower() switch
        {
            "cargo_full" => CargoFull,
            "storm_active" => StormActive,
            "in_hazard_zone" => InHazardZone,
            "nearby_charger" => NearbyCharger,
            "nearby_damaged_drone" => NearbyDamagedDrone,
            _ => throw new InvalidOperationException($"Unknown query: {queryName}")
        };
    }

    /// <summary>
    /// Gets a variable value by name (for comparisons)
    /// </summary>
    public float GetVariableValue(string variableName)
    {
        return variableName.ToLower() switch
        {
            "battery" => Battery,
            "hp" => HP,
            "cargo" => CargoAmount,
            _ => throw new InvalidOperationException($"Unknown variable: {variableName}")
        };
    }

    public override string ToString()
    {
        return $"Drone[Battery:{Battery:F1}% HP:{HP:F1}% Cargo:{CargoAmount}/{MaxCargo} Pos:{Position} Storm:{StormActive} Hazard:{InHazardZone}]";
    }
}

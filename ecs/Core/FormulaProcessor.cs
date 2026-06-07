using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace Core;

public static class FormulaProcessor
{
    private static Dictionary<string, FormulaDto> _rawFormulas = new();

    /// <summary>
    /// Loads formulas from a JSON file.
    /// </summary>
    public static void Initialize(string jsonFilePath)
    {
        string json = File.ReadAllText(jsonFilePath);
        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            foreach (JsonProperty property in doc.RootElement.EnumerateObject())
            {
                // Only process entries that have an "Operations" list
                if (property.Value.TryGetProperty("Operations", out _))
                {
                    FormulaDto? dto = JsonSerializer.Deserialize<FormulaDto>(property.Value.GetRawText());
                    if (dto != null)
                    {
                        _rawFormulas[property.Name] = dto;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Executes a formula using indexed array lookups.
    /// This eliminates reflection, improving performance and scalability.
    /// </summary>
    public static float Execute(string formulaName, in CharacterStats stats, StatRegistry registry, int weaponDmg)
    {
        if (!_rawFormulas.TryGetValue(formulaName, out var formula))
            return weaponDmg;

        float result = 0;

        foreach (var op in formula.Operations)
        {
            float statValue = 0;

            // Resolve the stat value from the flat array using the registry index
            if (!string.IsNullOrEmpty(op.Stat))
            {
                int index = registry.GetIndex(op.Stat);
                if (index != -1)
                {
                    statValue = stats.Values[index];
                }
            }

            // Apply operations based on the data-driven type
            switch (op.Type)
            {
                case "Add":
                    result += statValue + op.Value;
                    break;
                case "Multiply":
                    result += (statValue * op.Value);
                    break;
                // You can add further logic (e.g., "Set", "Subtract") here as needed
            }
        }
        
        return result + weaponDmg;
    }
}

// Data Transfer Objects for JSON deserialization
public record FormulaDto(List<OperationDto> Operations);
public record OperationDto(string Type, string? Stat, string? Target, string? Source, float Value);

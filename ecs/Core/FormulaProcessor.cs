using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace Core;

public static class FormulaProcessor
{
    private static Dictionary<string, FormulaDto> _rawFormulas = new();

    public static void Initialize(string jsonFilePath)
    {
        string json = File.ReadAllText(jsonFilePath);
        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            foreach (JsonProperty property in doc.RootElement.EnumerateObject())
            {
                if (property.Value.TryGetProperty("Operations", out _))
                {
                    FormulaDto? dto = JsonSerializer.Deserialize<FormulaDto>(property.Value.GetRawText());
                    if (dto != null) _rawFormulas[property.Name] = dto;
                }
            }
        }
    }

    public static float Execute(string formulaName, in CharacterStats stats, int weaponDmg)
    {
        if (!_rawFormulas.TryGetValue(formulaName, out var formula))
            return weaponDmg;

        float result = 0;
        foreach (var op in formula.Operations)
        {
            float statValue = 0;
            if (!string.IsNullOrEmpty(op.Stat))
            {
                // Parse the string directly to the Enum
                if (Enum.TryParse<StatType>(op.Stat, out var type))
                {
                    statValue = stats.Values[(int)type];
                }
            }

            switch (op.Type)
            {
                case "Add": result += statValue + op.Value; break;
                case "Multiply": result += (statValue * op.Value); break;
            }
        }
        return result + weaponDmg;
    }
}
public record FormulaDto(List<OperationDto> Operations);
public record OperationDto(string Type, string? Stat, string? Target, string? Source, float Value);

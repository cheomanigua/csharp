using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Reflection;

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

    // --- Combat Execution ---
    public static float Execute(string formulaName, in CharacterStats stats, int weaponDmg)
    {
        if (!_rawFormulas.TryGetValue(formulaName, out var formula))
            return weaponDmg;

        float result = 0;
        foreach (var op in formula.Operations)
        {
            float statValue = 0;
            if (!string.IsNullOrEmpty(op.Stat) && Enum.TryParse<StatType>(op.Stat, out var type))
            {
                statValue = stats.Values[(int)type];
            }

            switch (op.Type)
            {
                case "Add": result += statValue + op.Value; break;
                case "Multiply": result += (statValue * op.Value); break;
            }
        }
        return result + weaponDmg;
    }

    // --- Initialization Execution ---
    public static void ExecuteUpdate(string formulaName, ref CharacterStats stats, FormulaContext ctx)
    {
        if (!_rawFormulas.TryGetValue(formulaName, out var formula)) return;
    
        // 1. IMPORTANT: Reset to Base Stats so we don't stack item bonuses infinitely
        // Assume you have a way to reset to initial Class/Race values
        ResetToBaseStats(ref stats, ctx); 
    
        foreach (var op in formula.Operations)
        {
            float inputValue = !string.IsNullOrEmpty(op.Source) ? ResolveSource(op.Source, stats, ctx) : op.Value;
    
            if (Enum.TryParse<StatType>(op.Target, out var targetType))
            {
                switch (op.Type)
                {
                    case "Add":
                        stats.Values[(int)targetType] += (int)inputValue;
                        break;
                    case "Multiply":
                        stats.Values[(int)targetType] *= (int)inputValue;
                        break;
                    case "Set":
                        stats.Values[(int)targetType] = (int)inputValue;
                        break;
                }
            }
        }
    }


    private static void ResetToBaseStats(ref CharacterStats stats, FormulaContext ctx)
    {
        // 1. Clear current stats
        Array.Clear(stats.Values, 0, stats.Values.Length);
    }


    private static float ResolveSource(string source, CharacterStats stats, FormulaContext ctx)
    {
        // Try to get from ClassData
        var classProp = ctx.Class?.GetType().GetProperty(source);
        if (classProp != null) return Convert.ToSingle(classProp.GetValue(ctx.Class));
    
        // Try to get from RaceData
        var raceProp = ctx.Race?.GetType().GetProperty(source);
        if (raceProp != null) return Convert.ToSingle(raceProp.GetValue(ctx.Race));
    
        // Try to get from StatType enum
        if (Enum.TryParse<StatType>(source, out var type))
            return stats.Values[(int)type];
    
        return 0;
    }


}

public record FormulaDto(List<OperationDto> Operations);
public record OperationDto(string Type, string? Stat, string? Target, string? Source, float Value, float Modifier = 1.0f);

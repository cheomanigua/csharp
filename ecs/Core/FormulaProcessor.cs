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

    public static unsafe float Execute(string formulaName, in EntityHotData stats, int weaponDmg)
    {
        if (!_rawFormulas.TryGetValue(formulaName, out var formula))
            return weaponDmg;

        float result = 0;
        foreach (var op in formula.Operations)
        {
            float statValue = 0;
            if (!string.IsNullOrEmpty(op.Stat) && Enum.TryParse<StatType>(op.Stat, out var type))
            {
                statValue = stats.Stats[(int)type];
            }

            switch (op.Type)
            {
                case "Add": result += statValue + op.Value; break;
                case "Multiply": result += (statValue * op.Value); break;
            }
        }
        return result + weaponDmg;
    }

    public static unsafe void ExecuteUpdate(string formulaName, ref EntityHotData stats, FormulaContext ctx)
    {
        if (!_rawFormulas.TryGetValue(formulaName, out var formula)) return;
    
        foreach (var op in formula.Operations)
        {
            float inputValue = !string.IsNullOrEmpty(op.Source) 
                ? ResolveSource(op.Source, stats, ctx) 
                : op.Value;
    
            if (Enum.TryParse<StatType>(op.Target, out var targetType))
            {
                switch (op.Type)
                {
                    case "Add":
                        stats.Stats[(int)targetType] += (int)inputValue;
                        break;
                    case "Multiply":
                        stats.Stats[(int)targetType] *= (int)inputValue;
                        break;
                    case "Set":
                        stats.Stats[(int)targetType] = (int)inputValue;
                        break;
                }
            }
        }
    }

    public static unsafe void RecalculateStats(ref EntityHotData stats, FormulaContext ctx)
    {
        for (int i = 0; i < (int)StatType.Count; i++)
            stats.Stats[i] = 0;

        ExecuteUpdate("UpdateStats", ref stats, ctx);
    }

    private static unsafe float ResolveSource(string source, EntityHotData stats, FormulaContext ctx)
    {
        var classProp = ctx.Class?.GetType().GetProperty(source);
        if (classProp != null) return Convert.ToSingle(classProp.GetValue(ctx.Class));
    
        var raceProp = ctx.Race?.GetType().GetProperty(source);
        if (raceProp != null) return Convert.ToSingle(raceProp.GetValue(ctx.Race));
    
        if (Enum.TryParse<StatType>(source, out var type))
            return stats.Stats[(int)type];
    
        return 0;
    }
}

public record FormulaDto(List<OperationDto> Operations);
public record OperationDto(string Type, string? Stat, string? Target, string? Source, float Value, float Modifier = 1.0f);

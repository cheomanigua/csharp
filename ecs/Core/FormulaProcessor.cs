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
    public static void ExecuteInit(string formulaName, ref CharacterStats stats, FormulaContext ctx)
    {
        if (!_rawFormulas.TryGetValue(formulaName, out var formula)) return;

        foreach (var op in formula.Operations)
        {
            if (op.Source == null) continue;
            float sourceValue = ResolveSource(op.Source, ctx);
            
            if (Enum.TryParse<StatType>(op.Target, out var targetType))
            {
                switch (op.Type)
                {
                    case "Add": stats.Values[(int)targetType] += (int)sourceValue; break;
                    //case "Multiply": stats.Values[(int)targetType] = (int)(sourceValue * op.Modifier); break;
										case "Multiply": 
        // Debug check to verify the source value before applying the modifier
        if (op.Target == "Health")
        {
            DebugLog.Log($"DEBUG: Calculating Health. Current Strength is: {ctx.Stats.Values[(int)StatType.Strength]}");
        }
        
        stats.Values[(int)targetType] = (int)(sourceValue * op.Modifier); 
        break;
                    case "Set": stats.Values[(int)targetType] = (int)sourceValue; break;
                }
								ctx.Stats.Values[(int)targetType] = stats.Values[(int)targetType];
            }
        }
				DebugLog.Log($"DEBUG: FormulaProcessor finished. Health: {stats.Values[(int)StatType.Health]}, Mana: {stats.Values[(int)StatType.Mana]}");
    }

		private static float ResolveSource(string source, FormulaContext ctx)
    {
        if (string.IsNullOrEmpty(source)) return 0;
    
        // 1. Check if the source is a StatType (e.g., "Strength")
        if (Enum.TryParse<StatType>(source, out var type))
            return ctx.Stats.Values[(int)type];
    
        // 2. Dynamic Property Lookup via Reflection (for ClassData/RaceData)
        var classProp = ctx.Class?.GetType().GetProperty(source);
        if (classProp != null) return Convert.ToSingle(classProp.GetValue(ctx.Class));
    
        var raceProp = ctx.Race?.GetType().GetProperty(source);
        if (raceProp != null) return Convert.ToSingle(raceProp.GetValue(ctx.Race));
    
        // 3. Complex Source Parsing (e.g., "Strength * 1.5")
        if (source.Contains("*"))
        {
            var parts = source.Split('*');
            if (Enum.TryParse<StatType>(parts[0].Trim(), out var type2))
                return ctx.Stats.Values[(int)type2] * float.Parse(parts[1]);
        }
        
        return 0;
    }
}

public record FormulaDto(List<OperationDto> Operations);
public record OperationDto(string Type, string? Stat, string? Target, string? Source, float Value, float Modifier = 1.0f);

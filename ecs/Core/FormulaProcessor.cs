using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Reflection;

namespace Core;

public static class FormulaProcessor
{
    private static readonly Dictionary<string, Func<CharacterStats, int, float>> _formulas = new();
    private static Dictionary<string, FormulaDto> _rawFormulas = new();

    private static readonly Dictionary<string, FieldInfo> _statFieldCache = new();

    public static void Initialize(string jsonFilePath)
    {
    string json = File.ReadAllText(jsonFilePath);
    using (JsonDocument doc = JsonDocument.Parse(json))
    {
        foreach (JsonProperty property in doc.RootElement.EnumerateObject())
        {
            // Only process properties that are Objects (like InitStats)
            // and ignore the old String-based formulas for now
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                // Check if this object looks like our FormulaDto
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
        // Cache CharacterStats fields for O(1) access
        foreach (var field in typeof(CharacterStats).GetFields()) 
            _statFieldCache[field.Name] = field;

        foreach (var entry in _rawFormulas)
        {
            _formulas[entry.Key] = (stats, weaponDamage) => 
            {
                float result = 0;
                foreach (var step in entry.Value.Operations)
                {
                    float val = 0;
                    if (!string.IsNullOrEmpty(step.Stat) && _statFieldCache.TryGetValue(step.Stat, out var field))
                        val = (int)field.GetValue(stats)!;
                    
                    if (step.Type == "Multiply") result += val * step.Value;
                    else if (step.Type == "Add") result += step.Value;
                }
                return result + weaponDamage;
            };
        }
    }

    public static void ExecuteInitialization(string formulaName, ref CharacterStats stats, ClassData charClass, RaceData race)
    {
        if (!_rawFormulas.TryGetValue(formulaName, out var formula)) return;
    
        foreach (var op in formula.Operations)
        {
            if (op.Type == "Add" && op.Target != null && op.Source != null)
            {
                var targetField = _statFieldCache[op.Target];
                
                // 1. Try to get as Field
                var typeClass = typeof(ClassData);
                var typeRace = typeof(RaceData);
                
                PropertyInfo? sourceProp = typeClass.GetProperty(op.Source) ?? typeRace.GetProperty(op.Source);
                
                if (sourceProp != null)
                {
                    // Determine object
                    object sourceObj = typeClass.GetProperty(op.Source) != null ? (object)charClass : (object)race;
                    int sourceValue = (int)(sourceProp.GetValue(sourceObj) ?? 0);
    
                    int currentValue = (int)targetField.GetValue(stats)!;
                    object boxedStats = (object)stats;
                    targetField.SetValue(boxedStats, currentValue + sourceValue);
                    stats = (CharacterStats)boxedStats;
                }
                else
                {
                    Console.WriteLine($"DEBUG: CRITICAL - Property '{op.Source}' not found in ClassData or RaceData!");
                }
            }
        }
    }

    public static float Execute(string formulaName, in CharacterStats stats, int weaponDmg) 
        => _formulas[formulaName](stats, weaponDmg);
}

public record FormulaDto(List<OperationDto> Operations);
public record OperationDto(string Type, string? Stat, string? Target, string? Source, float Value);

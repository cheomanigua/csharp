#!/usr/bin/env python3
import json
import csv
import os
import sys

DB_FILE = "data.json"
INSTANCE_FILE = "instances.json"

def parse_bool(val, default=False):
    cleaned = str(val).strip().upper()
    if not cleaned:
        return default
    return cleaned in ["TRUE", "1", "YES", "T"]

def json_to_csv():
    """Compiles the dynamic layout from instances.json and static data from data.json 
    back into separate editable flat CSV files."""
    if not os.path.exists(DB_FILE) or not os.path.exists(INSTANCE_FILE):
        print(f"Error: Require both {DB_FILE} and {INSTANCE_FILE} to run an export.")
        return

    with open(DB_FILE, 'r') as f:
        db = json.load(f)
    with open(INSTANCE_FILE, 'r') as f:
        instances = json.load(f)

    classes = db.get("Blueprints", {})
    units = instances.get("Blueprints", {})

    headers = [
        "Name", "Faction", "Type", "ClassKey", "X", "Y", "Heading", "Speed",
        "RadarRange", "SonarRange", "IsActiveEmission", 
        "MaxMissiles", "MaxTorpedos", "MaxCIWS",
        "CurrentMissiles", "CurrentTorpedos", "CurrentCIWS",
        "SpriteKey", "ZIndex", "Doctrine"
    ]

    def export_type(filename, filter_type):
        match_count = 0
        with open(filename, "w", newline="", encoding="utf-8") as f:
            writer = csv.writer(f)
            writer.writerow(headers)
            
            for inst_key, inst in units.items():
                identity = inst.get("Identity", {})
                # Handle lowercase matching safely
                if identity.get("Type", "").strip().lower() != filter_type.lower():
                    continue
                
                class_key = inst.get("ClassKey", "")
                archetype = classes.get(class_key, {})
                
                position = inst.get("Position", {})
                velocity = inst.get("Velocity", {})
                sensors = inst.get("Sensors", {})
                current_wep = inst.get("WeaponInventory", {})
                
                arch_sensors = archetype.get("Sensors", {})
                arch_wep = archetype.get("WeaponInventory", {})
                render = archetype.get("Renderable", {})
                ai = archetype.get("AiIntel", {})

                match_count += 1
                writer.writerow([
                    identity.get("Name", "Unknown"),
                    identity.get("Faction", "Neutral"),
                    identity.get("Type", filter_type),
                    class_key,
                    position.get("X", 0.0),
                    position.get("Y", 0.0),
                    velocity.get("HeadingDegrees", 0.0),
                    velocity.get("SpeedKnots", 0.0),
                    arch_sensors.get("RadarRangeNM", 0.0),
                    arch_sensors.get("SonarRangeNM", 0.0),
                    sensors.get("IsActiveEmission", False),
                    arch_wep.get("AntiShipMissiles", 0),
                    arch_wep.get("Torpedos", 0),
                    arch_wep.get("PointDefenseAmmo", 0),
                    current_wep.get("AntiShipMissiles", 0),
                    current_wep.get("Torpedos", 0),
                    current_wep.get("PointDefenseAmmo", 0),
                    render.get("SpriteKey", "icon_default"),
                    render.get("ZIndex", 1),
                    ai.get("DoctrineKey", "StandardPassive")
                ])
        print(f"-> Created {filename} ({match_count} entries)")

    export_type("surface_ships.csv", "SurfaceShip")
    export_type("aircraft.csv", "Aircraft")
    export_type("submarines.csv", "Submarine")

def csv_to_json():
    """Reads map data from source CSVs, joins them relational style using lookups 
    from the data.json database, and generates a fully assembled instances.json."""
    if not os.path.exists(DB_FILE):
        print(f"Error: Master blueprint database {DB_FILE} is missing! Create it first.")
        return

    with open(DB_FILE, 'r') as f:
        try:
            db_master = json.load(f)
        except Exception:
            print(f"Error: {DB_FILE} contains invalid JSON formatting.")
            return

    existing_classes = db_master.get("Blueprints", {})
    new_instances = {}

    def parse_csv(filename, default_type):
        if not os.path.exists(filename): 
            return
        with open(filename, "r", encoding="utf-8-sig") as f:
            reader = csv.DictReader(f)
            reader.fieldnames = [field.strip() for field in reader.fieldnames] if reader.fieldnames else []
            
            for row in reader:
                name = row.get("Name", "").strip()
                if not name: 
                    continue
                
                inst_key = name.replace(" ", "")
                class_key = row.get("ClassKey", "").strip()
                unit_type = row.get("Type", "").strip() or default_type

                # CRITICAL STEP: Fetch static hull specs from data.json using ClassKey
                if class_key not in existing_classes:
                    print(f"Warning: ClassKey '{class_key}' found in {filename} but missing in {DB_FILE}! Skipping {name}.")
                    continue
                
                archetype = existing_classes[class_key]
                db_weapons = archetype.get("WeaponInventory", {})

                # Fallback rule helper: If CSV ammo cell is blank, fetch full capacity from data.json
                def resolve_ammo(csv_field, db_max_key):
                    val = row.get(csv_field, "").strip()
                    if val: 
                        return int(val)
                    return int(db_weapons.get(db_max_key, 0))

                # Build the fully combined live instance block
                new_instances[inst_key] = {
                    "Identity": {
                        "Name": name,
                        "Faction": row.get("Faction", "Neutral"),
                        "Type": unit_type
                    },
                    "ClassKey": class_key,
                    "Position": {
                        "X": float(row.get("X", 0.0) or 0.0),
                        "Y": float(row.get("Y", 0.0) or 0.0)
                    },
                    "Velocity": {
                        "HeadingDegrees": float(row.get("Heading", 0.0) or 0.0),
                        "SpeedKnots": float(row.get("Speed", 0.0) or 0.0)
                    },
                    "Sensors": {
                        "IsActiveEmission": parse_bool(row.get("IsActiveEmission", "False"))
                    },
                    "WeaponInventory": {
                        "AntiShipMissiles": resolve_ammo("CurrentMissiles", "AntiShipMissiles"),
                        "Torpedos": resolve_ammo("CurrentTorpedos", "Torpedos"),
                        "PointDefenseAmmo": resolve_ammo("CurrentCIWS", "PointDefenseAmmo")
                    }
                }

    # Gather data across all scenario map sheets
    parse_csv("surface_ships.csv", "SurfaceShip")
    parse_csv("aircraft.csv", "Aircraft")
    parse_csv("submarines.csv", "Submarine")

    if not new_instances:
        print("Aborting: No valid spreadsheet tracking rows were found to parse.")
        return

    # Assembling the output tree for instances.json
    inst_master = {"Blueprints": new_instances}

    # Write data matching your exact custom spacing line breaks
    inst_lines = ["{", '  "Blueprints": {']
    i_keys = list(inst_master["Blueprints"].keys())
    for i, ik in enumerate(i_keys):
        inst = inst_master["Blueprints"][ik]
        inst_lines.append(f'    "{ik}": {{')
        inst_lines.append('      "Identity": ' + json.dumps(inst["Identity"], indent=2).replace("\n", "\n      ") + ",")
        inst_lines.append(f'      "ClassKey": "{inst["ClassKey"]}",')
        inst_lines.append('      "Position": ' + json.dumps(inst["Position"]) + ",")
        inst_lines.append('      "Velocity": ' + json.dumps(inst["Velocity"]) + ",")
        inst_lines.append('      "Sensors": ' + json.dumps(inst["Sensors"]) + ",")
        inst_lines.append('      "WeaponInventory": ' + json.dumps(inst["WeaponInventory"]))
        inst_lines.append('    }' + ("," if i < len(i_keys) - 1 else ""))
    inst_lines.append("  }")
    inst_lines.append("}")

    with open(INSTANCE_FILE, 'w', encoding="utf-8") as f: 
        f.write("\n".join(inst_lines) + "\n")
        
    print(f"Success: Combined spreadsheets and {DB_FILE} to freshly create {INSTANCE_FILE}!")


if __name__ == "__main__":
    if len(sys.argv) < 2 or sys.argv[1] not in ["csv", "json"]:
        print("Usage: ./data_pipeline.py [csv|json]")
        sys.exit(1)
    if sys.argv[1] == "csv":
        json_to_csv()
    elif sys.argv[1] == "json":
        csv_to_json()

#!/bin/bash

JSON_FILE="data.json"

echo "Extracting naval data and generating reference tables..."

# 1. GENERATE SURFACE SHIPS TABLE
echo "Name,Faction,X,Y,Heading,Speed(kts),Radar(NM),Sonar(NM),Missiles,Torpedos" > surface_ships.csv
jq -r '.Blueprints[] | select(.Identity.Type == "SurfaceShip") | [
  .Identity.Name,
  .Identity.Faction,
  .Position.X,
  .Position.Y,
  .Velocity.HeadingDegrees,
  .Velocity.SpeedKnots,
  .Sensors.RadarRangeNM,
  .Sensors.SonarRangeNM,
  .WeaponInventory.AntiShipMissiles,
  .WeaponInventory.Torpedos
] | @csv' "$JSON_FILE" >> surface_ships.csv

# 2. GENERATE AIRCRAFT TABLE
echo "Name,Faction,Heading,Speed(kts),Radar(NM),Missiles" > aircraft.csv
jq -r '.Blueprints[] | select(.Identity.Type == "Aircraft") | [
  .Identity.Name,
  .Identity.Faction,
  .Velocity.HeadingDegrees,
  .Velocity.SpeedKnots,
  .Sensors.RadarRangeNM,
  .WeaponInventory.AntiShipMissiles
] | @csv' "$JSON_FILE" >> aircraft.csv

# 3. GENERATE SUBMARINES TABLE
echo "Name,Faction,Speed(kts),Sonar(NM),Torpedos" > submarines.csv
jq -r '.Blueprints[] | select(.Identity.Type == "Submarine") | [
  .Identity.Name,
  .Identity.Faction,
  .Velocity.SpeedKnots,
  .Sensors.SonarRangeNM,
  .WeaponInventory.Torpedos
] | @csv' "$JSON_FILE" >> submarines.csv

echo "Done! Generated surface_ships.csv, aircraft.csv, and submarines.csv."

// Central rule engine for WebGL-safe build placement: checks bounds, money, zones, overlap, dependencies, adjacency, and warnings.
using System.Collections.Generic;
using UnityEngine;
using SkyHubTycoon.Data;
using SkyHubTycoon.Grid;
using SkyHubTycoon.Simulation;

namespace SkyHubTycoon.Build
{
    public class PlacementValidator
    {
        private readonly GridManager grid;
        private readonly AirportState airport;

        public PlacementValidator(GridManager grid, AirportState airport)
        {
            this.grid = grid;
            this.airport = airport;
        }

        public PlacementResult ValidateFloor(FloorDefinition floor, Vector2Int origin, Vector2Int brushSize)
        {
            List<Vector2Int> footprint = grid.GetFootprint(origin, brushSize);
            Vector2Int[] cells = footprint.ToArray();

            for (int i = 0; i < footprint.Count; i++)
            {
                if (!grid.InBounds(footprint[i])) return PlacementResult.Invalid("Outside unlocked land.", cells);
            }

            if (airport.money < floor.cost * footprint.Count) return PlacementResult.Invalid("Not enough money.", cells);

            for (int i = 0; i < footprint.Count; i++)
            {
                GridCell cell = grid.GetCell(footprint[i]);
                if (cell.HasBuildable) return PlacementResult.Invalid("Cannot overlap existing objects.", cells);
                if (floor.zoneType != ZoneType.Airfield && cell.HasFloor && cell.Floor.Definition.zoneType == ZoneType.Airfield)
                {
                    return PlacementResult.Invalid("Cannot overlap runway, taxiway, or airfield pavement.", cells);
                }
            }

            if (!grid.IsFootprintConnectedToExistingFloor(footprint))
            {
                return PlacementResult.Invalid("Floor must connect to an existing airport floor, entrance, service road, or terminal foundation.", cells);
            }

            return PlacementResult.Valid("Paint " + floor.displayName + ".", cells, false);
        }

        public PlacementResult ValidateBuildable(BuildableDefinition definition, Vector2Int origin)
        {
            List<Vector2Int> footprint = grid.GetFootprint(origin, definition.size);
            Vector2Int[] cells = footprint.ToArray();

            for (int i = 0; i < footprint.Count; i++)
            {
                if (!grid.InBounds(footprint[i])) return PlacementResult.Invalid("Outside unlocked land.", cells);
            }

            if (airport.money < definition.cost) return PlacementResult.Invalid("Not enough money.", cells);
            if (airport.level < definition.requiredAirportLevel) return PlacementResult.Invalid("Requires airport level " + definition.requiredAirportLevel + ".", cells);
            if (airport.reputation < definition.requiredReputation) return PlacementResult.Invalid("Requires airport reputation ★" + definition.requiredReputation.ToString("0.0") + ".", cells);
            if (definition.requiresPassengerRoute && !airport.HasPassengerRoute()) return PlacementResult.Invalid("Requires a working passenger route first.", cells);
            if (definition.requiresAirfieldRoute && !airport.HasAirfieldRoute()) return PlacementResult.Invalid("Requires a working airfield route first.", cells);
            if (definition.requiresBaggageRoute && !airport.HasBaggageRoute()) return PlacementResult.Invalid("Requires a working baggage route first.", cells);
            if (definition.requiresStaffRoom && !airport.HasBuildable(BuildableType.StaffRoom)) return PlacementResult.Invalid("Needs a staff room before this can be placed.", cells);

            for (int i = 0; i < footprint.Count; i++)
            {
                GridCell cell = grid.GetCell(footprint[i]);
                if (cell.HasBuildable) return PlacementResult.Invalid("Any object overlapping another object is not allowed.", cells);
                if (!CellHasAllowedFloor(cell, definition)) return PlacementResult.Invalid(definition.invalidPlacementWarning, cells);
            }

            switch (definition.type)
            {
                case BuildableType.Entrance: return ValidateEntrance(definition, origin, cells);
                case BuildableType.CheckIn:
                case BuildableType.LargeCheckIn:
                    return ValidateCheckIn(definition, origin, cells);
                case BuildableType.Kiosk: return ValidateKiosk(definition, origin, cells);
                case BuildableType.Security:
                case BuildableType.MetalDetector:
                    return ValidateSecurity(definition, origin, cells);
                case BuildableType.Seating:
                case BuildableType.Bench:
                case BuildableType.LuxurySeating:
                    return ValidateSeating(definition, origin, cells);
                case BuildableType.SmallGate:
                case BuildableType.MediumGate:
                case BuildableType.LargeGate:
                case BuildableType.InternationalGate:
                    return ValidateGate(definition, origin, cells);
                case BuildableType.Runway:
                case BuildableType.MediumRunway:
                case BuildableType.LargeRunway:
                case BuildableType.InternationalRunway:
                    return ValidateRunway(definition, origin, cells);
                case BuildableType.Taxiway:
                case BuildableType.ServiceRoad:
                    return ValidateTaxiway(definition, origin, cells);
                case BuildableType.AircraftStand: return ValidateAircraftStand(definition, origin, cells);
                case BuildableType.FuelStation: return ValidateFuelStation(definition, origin, cells);
                case BuildableType.MaintenanceHangar: return ValidateMaintenanceHangar(definition, origin, cells);
                case BuildableType.BagDrop: return ValidateBagDrop(definition, origin, cells);
                case BuildableType.Carousel: return ValidateCarousel(definition, origin, cells);
                case BuildableType.PassportControl: return ValidatePassport(definition, origin, cells);
                case BuildableType.CustomsDesk: return ValidateCustoms(definition, origin, cells);
                default: return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
            }
        }

        private PlacementResult ValidateEntrance(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!IsOutdoorEdge(origin, definition.size)) return PlacementResult.Invalid("Entrance must be placed on an exterior wall or terminal edge.", cells);
            if (!HasClearTilesInFront(origin, definition.size.x, 2)) return PlacementResult.Invalid("Entrance must have at least 2 clear tiles in front.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateCheckIn(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!airport.HasBuildable(BuildableType.Entrance)) return PlacementResult.Invalid("Must have a passenger entrance first.", cells);
            if (!HasClearTilesInFront(origin, definition.size.x, 2)) return PlacementResult.Invalid("Must have at least 2 clear queue tiles in front.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateKiosk(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!airport.HasBuildableWithin(BuildableType.Entrance, origin, 6)) return PlacementResult.Invalid("Self check-in kiosk must be near an entrance.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateSecurity(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!airport.HasBuildable(BuildableType.CheckIn) && !airport.HasBuildable(BuildableType.Kiosk)) return PlacementResult.Invalid("Requires check-in before security.", cells);
            if (!HasClearTilesInFront(origin, definition.size.x, 2)) return PlacementResult.Invalid("Security needs queue space before it.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateSeating(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!airport.HasBuildable(BuildableType.Security)) return PlacementResult.Invalid("Waiting area must be placed after security.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateGate(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!airport.HasBuildable(BuildableType.Security)) return PlacementResult.Invalid("Requires security checkpoint first.", cells);
            if (!HasAnyRunway()) return PlacementResult.Invalid("No runway exists.", cells);
            if (!airport.HasBuildable(BuildableType.Taxiway)) return PlacementResult.Invalid("Gate must connect to taxiway.", cells);
            if (!HasAnySeatingWithin(origin, 10)) return PlacementResult.Invalid("Must be placed next to gate seating.", cells);
            if (definition.type == BuildableType.InternationalGate && (!airport.HasBuildable(BuildableType.PassportControl) || !airport.HasBuildable(BuildableType.CustomsDesk)))
            {
                return PlacementResult.Invalid("International gates require passport control and customs.", cells);
            }
            bool inefficient = !HasAnySeatingWithin(origin, 6);
            return PlacementResult.Valid(inefficient ? "Allowed, but inefficient: seating should be within 6 tiles for best boarding." : DefaultValidMessage(definition), cells, inefficient);
        }

        private PlacementResult ValidateRunway(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateTaxiway(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateAircraftStand(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!airport.HasBuildable(BuildableType.Taxiway)) return PlacementResult.Invalid("Aircraft stand must connect to a taxiway.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateFuelStation(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!airport.HasBuildable(BuildableType.ServiceRoad)) return PlacementResult.Invalid("Fuel station requires service road access.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateMaintenanceHangar(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!airport.HasBuildable(BuildableType.ServiceRoad)) return PlacementResult.Invalid("Maintenance hangar requires a service road.", cells);
            if (!airport.HasBuildable(BuildableType.MaintenanceRoom)) return PlacementResult.Invalid("Maintenance hangar requires a maintenance room.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateBagDrop(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!HasAdjacent(BuildableType.CheckIn, origin, definition.size) && !HasAdjacent(BuildableType.LargeCheckIn, origin, definition.size)) return PlacementResult.Invalid("Bag drop must sit next to or behind a check-in desk.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateCarousel(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!airport.HasAnyGate()) return PlacementResult.Invalid("Baggage carousel requires a gate and arrivals path first.", cells);
            if (!HasAdjacent(BuildableType.Conveyor, origin, definition.size) && !HasAdjacent(BuildableType.ConveyorCorner, origin, definition.size)) return PlacementResult.Invalid("Baggage carousel must connect to a conveyor belt.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidatePassport(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (airport.level < definition.requiredAirportLevel) return PlacementResult.Invalid("International processing requires airport level " + definition.requiredAirportLevel + ".", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private PlacementResult ValidateCustoms(BuildableDefinition definition, Vector2Int origin, Vector2Int[] cells)
        {
            if (!airport.HasBuildable(BuildableType.PassportControl)) return PlacementResult.Invalid("Customs desk requires passport control first.", cells);
            return PlacementResult.Valid(DefaultValidMessage(definition), cells, false);
        }

        private bool HasAnyRunway()
        {
            return airport.HasBuildable(BuildableType.Runway) || airport.HasBuildable(BuildableType.MediumRunway) || airport.HasBuildable(BuildableType.LargeRunway) || airport.HasBuildable(BuildableType.InternationalRunway);
        }

        private bool HasAnySeatingWithin(Vector2Int origin, int range)
        {
            return airport.HasBuildableWithin(BuildableType.Seating, origin, range) || airport.HasBuildableWithin(BuildableType.Bench, origin, range) || airport.HasBuildableWithin(BuildableType.LuxurySeating, origin, range);
        }

        private bool CellHasAllowedFloor(GridCell cell, BuildableDefinition definition)
        {
            if (!cell.HasFloor || definition.allowedFloors == null || definition.allowedFloors.Length == 0) return false;
            for (int i = 0; i < definition.allowedFloors.Length; i++)
            {
                if (cell.Floor.Definition == definition.allowedFloors[i]) return true;
            }
            return false;
        }

        private bool HasClearTilesInFront(Vector2Int origin, int width, int distance)
        {
            for (int dy = 1; dy <= distance; dy++)
            {
                for (int dx = 0; dx < width; dx++)
                {
                    Vector2Int position = new Vector2Int(origin.x + dx, origin.y - dy);
                    if (!grid.InBounds(position)) return false;
                    GridCell cell = grid.GetCell(position);
                    if (cell.HasBuildable) return false;
                }
            }
            return true;
        }

        private bool IsOutdoorEdge(Vector2Int origin, Vector2Int size)
        {
            return origin.x == 0 || origin.y == 0 || origin.x + size.x >= grid.Width || origin.y + size.y >= grid.Height;
        }

        private bool HasAdjacent(BuildableType type, Vector2Int origin, Vector2Int size)
        {
            List<Vector2Int> footprint = grid.GetFootprint(origin, size);
            for (int i = 0; i < footprint.Count; i++)
            {
                foreach (Vector2Int neighbor in grid.GetCardinalNeighbors(footprint[i]))
                {
                    GridCell cell = grid.GetCell(neighbor);
                    if (cell.HasBuildable && cell.Buildable.Definition.type == type) return true;
                }
            }
            return false;
        }

        private string DefaultValidMessage(BuildableDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(definition.validPlacementMessage)) return definition.validPlacementMessage;
            return "Place " + definition.displayName + ".";
        }
    }
}

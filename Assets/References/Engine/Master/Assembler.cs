
using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Assembler : Ability
    {
        public override string Name { get { return "Assembler"; } }

        public List<string> BuildQueue { get; set; }

        public void Build (string blueprintName)
        {
            if (BuildQueue == null)
                BuildQueue = new List<string>();
            BuildQueue.Add(blueprintName);
        }


        public bool CanProduce()
        {
            if (Unit.Power == 0)
                return false;
            if (TileContainer != null && TileContainer.Minerals > 0)
                return true;
            if (Unit.Container != null && Unit.Container.TileContainer.Minerals> 0)
                return true;
            return false;
        }

        public Assembler(Unit owner, int level) : base(owner, TileObjectType.PartAssembler)
        {
            Level = level;
            TileContainer = new TileContainer();
            TileContainer.Capacity = 4;
            TileContainer.AcceptedTileObjectTypes = TileObjectType.Mineral;
        }

        
        public TileObject ConsumeMineralForUnit()
        {
            TileObject tileObject = null;
            if (Unit.Container != null)
            {
                tileObject = Unit.Container.TileContainer.RemoveTileObject(TileObjectType.Mineral);
            }
            if (tileObject == null)
            {
                if (TileContainer != null)
                {
                    tileObject = TileContainer.RemoveTileObject(TileObjectType.Mineral);
                }
            }
            return tileObject;
        }

        private Move CreateAssembleMove(Position2 pos, string productCode)
        {
            Move move;

            // possible production move
            move = new Move();
            move.MoveType = MoveType.Build;
            move.Positions = new List<Position2>();
            move.Positions.Add(Unit.Pos);
            move.Positions.Add(pos);
            move.PlayerId = Unit.Owner.PlayerModel.Id;
            move.OtherUnitId = Unit.UnitId;

            move.Stats = new MoveUpdateStats();
            move.Stats.BlueprintName = productCode;

            return move;
        }

        private Move CreateUpgradeMove(Position2 pos, Unit assemblerUnit, Unit upgradedUnit, BlueprintPart blueprintPart) 
        {
            int level = blueprintPart.Level;
            
            while (level > 1)
            {
                level--;
                if (upgradedUnit.IsInstalled(blueprintPart, level))
                {
                    level++;
                    break;
                }
            }

            Move move;

            // possible production move
            move = new Move();
            move.MoveType = MoveType.Upgrade;
            move.Positions = new List<Position2>();
            move.Positions.Add(Unit.Pos);
            move.Positions.Add(pos);
            move.PlayerId = Unit.Owner.PlayerModel.Id;
            move.UnitId = assemblerUnit.UnitId;
            move.OtherUnitId = upgradedUnit.UnitId;

            move.Stats = new MoveUpdateStats();

            MoveUpdateUnitPart part = new MoveUpdateUnitPart();
            part.PartType = blueprintPart.PartType;
            part.Level = level;
            part.Name = blueprintPart.Name;
            part.Capacity = blueprintPart.Capacity;
            part.TileObjects = new List<TileObject>();
            part.CompleteLevel = blueprintPart.Level;

            move.Stats.UnitParts = new List<MoveUpdateUnitPart>();
            move.Stats.UnitParts.Add(part);

            return move;
        }


        public static bool DoesMoveMinRequest(Move move, UnitType unitType, Unit unit)
        {
            // Minimal requests ok?
            if (move.UnitId == "Engine" && unitType.MinEngineLevel > 0)
            {
                if (unit == null || unit.Engine == null || unit.Engine.Level < unitType.MinEngineLevel)
                {
                    return true;
                }
            }
            if (move.UnitId == "Armor" && unitType.MinArmorLevel > 0)
            {
                if (unit == null || unit.Armor == null || unit.Armor.Level < unitType.MinArmorLevel)
                {
                    return true;
                }
            }
            if (move.UnitId == "Weapon" && unitType.MinWeaponLevel > 0)
            {
                if (unit == null || unit.Weapon == null || unit.Weapon.Level < unitType.MinWeaponLevel)
                {
                    return true;
                }
            }

            if (move.UnitId == "Assembler" && unitType.MinAssemblerLevel > 0)
            {
                if (unit == null || unit.Assembler == null || unit.Assembler.Level < unitType.MinAssemblerLevel)
                {
                    return true;
                }
            }

            if (move.UnitId == "Extractor" && unitType.MinExtractorLevel > 0)
            {
                if (unit == null || unit.Extractor == null || unit.Extractor.Level < unitType.MinExtractorLevel)
                {
                    return true;
                }
            }
            if (move.UnitId == "Container" && unitType.MinContainerLevel > 0)
            {
                if (unit == null || unit.Container == null || unit.Container.Level < unitType.MinContainerLevel)
                {
                    return true;
                }
            }
            if (move.UnitId == "Reactor" && unitType.MinReactorLevel > 0)
            {
                if (unit == null || unit.Reactor == null || unit.Reactor.Level < unitType.MinReactorLevel)
                {
                    return true;
                }
            }
            if (move.UnitId == "Radar" && unitType.MinRadarLevel > 0)
            {
                if (unit == null || unit.Radar == null || unit.Radar.Level < unitType.MinRadarLevel)
                {
                    return true;
                }
            }

            if (unit != null)
            {
                // Optional requests?
                if (move.UnitId == "Engine" && unit.Engine != null && unit.Engine.Level < unitType.MaxEngineLevel)
                {
                    return true;
                }
                if (move.UnitId == "Armor" && unit.Armor != null && unit.Armor.Level < unitType.MaxArmorLevel)
                {
                    return true;
                }
                if (move.UnitId == "Weapon" && unit.Weapon != null && unit.Weapon.Level < unitType.MaxWeaponLevel)
                {
                    return true;
                }
                if (move.UnitId == "Extractor" && unit.Extractor != null && unit.Extractor.Level < unitType.MaxExtractorLevel)
                {
                    return true;
                }
                if (move.UnitId == "Container" && unit.Container != null && unit.Container.Level < unitType.MaxContainerLevel)
                {
                    return true;
                }
                if (move.UnitId == "Reactor" && unit.Reactor != null && unit.Reactor.Level < unitType.MaxReactorLevel)
                {
                    return true;
                }
                if (move.UnitId == "Radar" && unit.Radar != null && unit.Radar.Level < unitType.MaxRadarLevel)
                {
                    return true;
                }
            }
            return false;
        }


        public Dictionary<Position2, TileWithDistance> CollectOutputPosition2s()
        {
            Dictionary<Position2, TileWithDistance> positions = Unit.Game.Map.EnumerateTiles(Unit.Pos, 1, false);

            return positions;
        }

        private bool CanBuildAt(TileWithDistance neighbor)
        {
            if (Unit.CurrentGameCommand != null)
            { 
                // Disabled. Otherwise cannt update a worker
                /*
                if (Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build &&
                        neighbor.Pos == Unit.CurrentGameCommand.TargetPosition2)
                {
                    return true;
                }
                
                return false;
                */
            }
            return true;
        }

        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position2> includedPosition2s, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Assemble) == 0 && (moveFilter & MoveFilter.Upgrade) == 0)
                return;

            if (!CanProduce())
                return;

            
            Dictionary<Position2, TileWithDistance> neighbors = CollectOutputPosition2s();
            /*
            foreach (TileWithDistance neighbor in neighbors.Values)
            {
                if (!CanBuildAt(neighbor))
                    continue;

                PlayerUnit playerUnitNeighbor = null;
                foreach (PlayerUnit playerUnit1 in Unit.Owner.UnitsInBuild.Values)
                {
                    if (playerUnit1.Unit != this.Unit && playerUnit1.Unit.Pos == neighbor.Tile.Pos)
                    {
                        playerUnitNeighbor = playerUnit1;
                        break;
                    }
                }

                if (playerUnitNeighbor != null)
                {
                    PlayerUnit playerUnit = Unit.Owner.UnitsInBuild[playerUnitNeighbor.Unit.UnitId];
                    if (playerUnit.Unit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                    {
                        if ((moveFilter & MoveFilter.Assemble) > 0)
                        {
                            // Already one Unit in progress, do not build another
                            return;
                        }
                        if ((moveFilter & MoveFilter.Upgrade) > 0)
                        {
                            foreach (BlueprintPart blueprintPart in playerUnit.Unit.Blueprint.Parts)
                            {
                                if (!playerUnit.Unit.IsInstalled(blueprintPart, blueprintPart.Level))
                                {
                                    possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, Unit, playerUnit.Unit, blueprintPart));
                                    return;
                                }
                            }
                        }
                    }
                }
            }*/
            foreach (TileWithDistance neighbor in neighbors.Values)
            {
                if (!CanBuildAt(neighbor))
                    continue;

                if (neighbor.Unit == null)
                {
                    if (!neighbor.Tile.CanBuild())
                        continue;
                }

                if (includedPosition2s != null)
                {
                    if (!includedPosition2s.Contains(neighbor.Pos))
                        continue;
                }

                if (neighbor.Unit == null)
                {
                    if ((moveFilter & MoveFilter.Assemble) > 0)
                    {
                        if (Level > 0)
                        {
                            if (Unit.CurrentGameCommand == null)
                            {
                                //Can build everything
                                foreach (Blueprint blueprint in Unit.Owner.Game.Blueprints.Items)
                                {
                                    possibleMoves.Add(CreateAssembleMove(neighbor.Pos, blueprint.Name));                                    
                                }
                            }
                            else
                            {
                                //foreach (BlueprintCommandItem blueprintCommandItem in Unit.CurrentGameCommand.BlueprintCommand.Units)
                                {
                                    // Can units in command
                                    //foreach (Blueprint blueprint in Unit.Owner.Game.Blueprints.Items)
                                    {
                                        //if (blueprint.Name == blueprintCommandItem.BlueprintName)
                                        {
                                            possibleMoves.Add(CreateAssembleMove(neighbor.Pos, Unit.CurrentGameCommand.BlueprintName));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (neighbor.Unit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                    {
                        if (Level > 0 && !neighbor.Unit.IsComplete() && !neighbor.Unit.ExtractMe)
                        {
                            if ((moveFilter & MoveFilter.Upgrade) > 0)
                            {
                                foreach (BlueprintPart blueprintPart in neighbor.Unit.Blueprint.Parts)
                                {
                                    if (!neighbor.Unit.IsInstalled(blueprintPart, blueprintPart.Level))
                                    {
                                        possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, Unit, neighbor.Unit, blueprintPart));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Enemy nearby? OMG
                    }
                }
            }
        }
    }
}

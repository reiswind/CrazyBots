using Engine.Control;
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
        public int Level { get; set; }

        public Container AttachedContainer { get; set; }

        public bool CanProduce()
        {
            if (Unit.Container != null && Unit.Container.Metal > 0)
                return true;

            if (AttachedContainer != null)
            {
                if (AttachedContainer != null && AttachedContainer.Metal > 0)
                    return true;
            }
            return Unit.Metal > 0;
        }

        public Assembler(Unit owner, int level) : base(owner)
        {
            Level = level;
        }

        public void ConsumeMetalForUnit(Unit unit)
        {
            if (Unit.Container != null && Unit.Container.Metal > 0)
                Unit.Container.Metal--;
            else if (Unit.Metal > 0)
                Unit.Metal--;
            else
            {
                if (AttachedContainer != null)
                {
                    if (AttachedContainer != null && AttachedContainer.Metal > 0)
                        AttachedContainer.Metal--;
                    else if (AttachedContainer.Metal > 0)
                        AttachedContainer.Metal--;
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        private Move CreateAssembleMove(Position pos, string productCode)
        {
            Move move;

            // possible production move
            move = new Move();
            move.MoveType = MoveType.Add;
            move.Positions = new List<Position>();
            move.Positions.Add(Unit.Pos);
            move.Positions.Add(pos);
            move.PlayerId = Unit.Owner.PlayerModel.Id;
            move.UnitId = productCode;
            move.OtherUnitId = Unit.UnitId;

            return move;
        }

        private Move CreateUpgradeMove(Position pos, string productCode)
        {
            Move move;

            // possible production move
            move = new Move();
            move.MoveType = MoveType.Upgrade;
            move.Positions = new List<Position>();
            move.Positions.Add(Unit.Pos);
            move.Positions.Add(pos);
            move.PlayerId = Unit.Owner.PlayerModel.Id;
            move.UnitId = productCode;
            move.OtherUnitId = Unit.UnitId;

            return move;
        }

        public bool HandleRequestUnit(PlayerUnit playerUnit, DispatcherRequestUnit dispatcherRequestUnit, Move move)
        {
            Tile tile = playerUnit.Unit.Owner.Game.Map.GetTile(move.Positions[1]);

            if (DoesMoveMinRequest(move, dispatcherRequestUnit.UnitType, tile.Unit))
            {
                playerUnit.PossibleMoves.Add(new PlayerMove(move));

                // Take first option or random but dicide here
                return true;
            }
            return false;

            /*

           Unit cntrlUnit = playerUnit.Unit;

            Dictionary<Position, TileWithDistance> outputPositions = cntrlUnit.Assembler.CollectOutputPositions();
            bool upgraded = false;

            // Check for incomplete units at output positions
            foreach (TileWithDistance outputPosition in outputPositions.Values)
            {
                if (outputPosition.Unit != null &&
                    !outputPosition.Unit.IsComplete() &&
                    outputPosition.Unit.Owner.PlayerModel.Id == playerUnit.Unit.Owner.PlayerModel.Id)
                {
                    if (dispatcherRequestUnit.FavoriteUnits != null)
                    {
                        bool outputPositionIsFine = false;
                        foreach (PlayerUnit favoriteUnit in dispatcherRequestUnit.FavoriteUnits)
                        {
                            if (favoriteUnit.Unit.UnitId == outputPosition.Unit.UnitId)
                            {
                                outputPositionIsFine = true;
                                break;
                            }
                        }
                        if (!outputPositionIsFine)
                            break;
                    }
                    
                    // Own incomplete unit at output area. Try to upgrade this
                    List<Move> possiblemoves = new List<Move>();
                    cntrlUnit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Upgrade);
                    if (possiblemoves.Count > 0)
                    {
                        foreach (Move move in possiblemoves)
                        {
                            Tile tile = playerUnit.Unit.Owner.Game.Map.GetTile(move.Positions[1]);

                            if (DoesMoveMinRequest(move, dispatcherRequestUnit.UnitType, tile.Unit))
                            {
                                if (dispatcherRequestUnit.FavoriteUnits == null)
                                    dispatcherRequestUnit.Command.AssignUnit(outputPosition.Unit.UnitId);

                                playerUnit.PossibleMoves.Add(move);
                                upgraded = true;
                                // Take first option or random but dicide here
                                break;
                            }
                        }
                        if (upgraded)
                            break;
                    }
                }
            }
            if (upgraded == false)
            {
                // Does it matter? Produce anything for now or something that is closer to unittype
                List<Move> possiblemoves = new List<Move>();
                cntrlUnit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Assemble);
                if (possiblemoves.Count > 0)
                {
                    foreach (Move move in possiblemoves)
                    {
                        if (DoesMoveMinRequest(move, dispatcherRequestUnit.UnitType, null))
                        {
                            playerUnit.PossibleMoves.Add(move);

                        }
                    }
                }
            }*/
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


        public Dictionary<Position, TileWithDistance> CollectOutputPositions()
        {
            Dictionary<Position, TileWithDistance> positions = Unit.Game.Map.EnumerateTiles(Unit.Pos, 1, true);

            return positions;
        }

        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Assemble) == 0 && (moveFilter & MoveFilter.Upgrade) == 0)
                return;

            if (!CanProduce())
                return;

            Dictionary<Position, TileWithDistance> neighbors = CollectOutputPositions();
            foreach (TileWithDistance neighbor in neighbors.Values)
            {
                if (!neighbor.Tile.CanMoveTo())
                    continue;

                if (includedPositions != null)
                {
                    if (!includedPositions.Contains(neighbor.Pos))
                        continue;
                }

                if (neighbor.Unit == null)
                {
                    if ((moveFilter & MoveFilter.Assemble) > 0)
                    {
                        if (Level > 0)
                        {
                            possibleMoves.Add(CreateAssembleMove(neighbor.Pos, "Engine"));
                            possibleMoves.Add(CreateAssembleMove(neighbor.Pos, "Armor"));
                            possibleMoves.Add(CreateAssembleMove(neighbor.Pos, "Weapon"));
                            possibleMoves.Add(CreateAssembleMove(neighbor.Pos, "Assembler"));
                            possibleMoves.Add(CreateAssembleMove(neighbor.Pos, "Extractor"));
                            possibleMoves.Add(CreateAssembleMove(neighbor.Pos, "Container"));
                            possibleMoves.Add(CreateAssembleMove(neighbor.Pos, "Reactor"));
                            possibleMoves.Add(CreateAssembleMove(neighbor.Pos, "Radar"));
                        }
                    }
                }
                else
                {
                    if (neighbor.Unit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                    {
                        if (Level > 0 && !neighbor.Unit.IsComplete())
                        {
                            if ((moveFilter & MoveFilter.Upgrade) > 0)
                            {
                                if (neighbor.Unit.Engine == null || neighbor.Unit.Engine.Level < 3)
                                {
                                    possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, "Engine"));
                                }
                                if (neighbor.Unit.Armor == null || neighbor.Unit.Armor.Level < 3)
                                {
                                    possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, "Armor"));
                                }
                                if (neighbor.Unit.Weapon == null || neighbor.Unit.Weapon.Level < 3)
                                {
                                    possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, "Weapon"));
                                }
                                if (neighbor.Unit.Extractor == null || neighbor.Unit.Extractor.Level < 3)
                                {
                                    possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, "Extractor"));
                                }
                                if (neighbor.Unit.Assembler == null || neighbor.Unit.Assembler.Level < 3)
                                {
                                    possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, "Assembler"));
                                }
                                if (neighbor.Unit.Container == null || neighbor.Unit.Container.Level < 3)
                                {
                                    possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, "Container"));
                                }
                                if (neighbor.Unit.Reactor == null || neighbor.Unit.Reactor.Level < 3)
                                {
                                    possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, "Reactor"));
                                }
                                if (neighbor.Unit.Radar == null || neighbor.Unit.Radar.Level < 3)
                                {
                                    possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, "Radar"));
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

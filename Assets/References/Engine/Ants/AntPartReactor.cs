﻿
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{

    internal class AntPartReactor : AntPart
    {
        public Reactor Reactor { get; private set; }

        public AntPartReactor(Ant ant, Reactor reactor) : base(ant)
        {
            Reactor = reactor;
        }

        public override string ToString()
        {
            return "AntPartReactor";
        }
        /*
        public bool CheckBuildReactorMove(Player player, Ant ant, List<Move> moves)
        {
            bool unitMoved = false;
            Unit cntrlUnit = ant.PlayerUnit.Unit;

            if (cntrlUnit.Engine == null && cntrlUnit.Reactor != null)
            {
                if (ant.PheromoneDepositEnergy != 0)
                {
                    Position2 pos = player.Game.Pheromones.Find(ant.PheromoneDepositEnergy, player, PheromoneType.Energy, 0.6f, 0.6f);
                    if (pos != Position2.Null)
                    {
                        Tile t = player.Game.GetTile(pos);
                        List<Position2> path = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, pos, ant.PlayerUnit.Unit);
                        if (path == null)
                        {

                        }
                        else
                        {
                            GameCommand gameCommand = new GameCommand();
                            gameCommand.GameCommandType = GameCommandType.Build;
                            gameCommand.TargetPosition = pos;
                            gameCommand.DeleteWhenFinished = true;
                            gameCommand.UnitId = "Outpost";

                            foreach (BlueprintCommand blueprintCommand in player.Game.Blueprints.Commands)
                            {
                                if (blueprintCommand.GameCommandType == GameCommandType.Build)
                                {
                                    gameCommand.BlueprintCommand = blueprintCommand;
                                }
                            }

                            player.GameCommands.Add(gameCommand);
                        }
                    }
                }
            }
            return unitMoved;
        }
        */
        public override bool Move(ControlAnt control, Player player, List<Move> moves)
        {
            if (Reactor.TileContainer.Count == 0 && Ant.Unit.CurrentGameCommand == null)
            {
                // Need something to burn!
                GameCommand gameCommand = new GameCommand();
                gameCommand.GameCommandType = GameCommandType.ItemRequest;
                gameCommand.Layout = "UIDelivery";
                gameCommand.TargetPosition = Ant.Unit.Pos;
                gameCommand.DeleteWhenFinished = true;
                gameCommand.PlayerId = player.PlayerModel.Id;

                BlueprintCommandItem blueprintCommandItem = new BlueprintCommandItem();
                blueprintCommandItem.BlueprintName = Ant.Unit.Blueprint.Name;
                blueprintCommandItem.Direction = Direction.C;

                GameCommandItem gameCommandItem = new GameCommandItem(gameCommand, blueprintCommandItem);
                gameCommandItem.TargetUnitId = Ant.Unit.UnitId;
                
                gameCommand.RequestedItems = new List<RecipeIngredient>();

                RecipeIngredient recipeIngredient = new RecipeIngredient(TileObjectType.Burn, Reactor.TileContainer.Capacity);
                gameCommand.RequestedItems.Add(recipeIngredient);

                Ant.Unit.SetGameCommand(gameCommandItem);

                gameCommand.GameCommandItems.Add(gameCommandItem);
                player.GameCommands.Add(gameCommand);
            }
            /*
            if (Reactor.TileContainer.Count < Reactor.TileContainer.Capacity)
            {
                AntNetworkNode.Demand(this, AntNetworkDemandType.Minerals, Reactor.TileContainer.Capacity / (Reactor.TileContainer.Capacity - Reactor.TileContainer.Count));
            }
            
            // Count connections to Reactors
            int countReactorsNearby = 0;
            foreach (AntNetworkConnect antNetworkConnect in AntNetworkNode.Connections)
            {
                if (antNetworkConnect.AntPartSource.Ant == Ant)
                    continue;

                if (antNetworkConnect.AntPartSource is AntPartReactor &&
                    antNetworkConnect.AntPartTarget is AntPartReactor)
                {
                    countReactorsNearby++;
                }
            }
            if (countReactorsNearby < 2) // && control.NumberOfReactors < 2)
            {
                // Every reactor needs a friend
                if (control.CanBuildReactor(player))
                {
                    CheckBuildReactorMove(player, Ant, moves);
                }
            }
            */
            return false;
        }
    }
}

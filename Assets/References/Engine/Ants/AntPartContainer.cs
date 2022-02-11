
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntPartContainer : AntPart
    {
        public Container Container { get; private set; }
        public AntPartContainer(Ant ant, Container container) : base(ant)
        {
            Container = container;
        }
        public override string ToString()
        {
            return "AntPartContainer";
        }
        public override bool Move(ControlAnt control, Player player, List<Move> moves)
        {
            bool needItems = false;
            GameCommand runningGameCommand = null;

            foreach (GameCommand existingGameCommand in player.GameCommands)
            {
                if (existingGameCommand.GameCommandType == GameCommandType.ItemRequest)
                {
                    foreach (GameCommandItem existingCommandItem in existingGameCommand.GameCommandItems)
                    {
                        if (existingCommandItem.TargetUnit.UnitId == Ant.Unit.UnitId)
                        {
                            runningGameCommand = existingGameCommand;
                            break;
                        }
                    }
                }
                if (runningGameCommand != null)
                    break;
            }

            if (Container.TileContainer.IsFreeSpace)
            {
                foreach (UnitItemOrder unitItemOrder in Ant.Unit.UnitOrders.unitItemOrders)
                {

                    if (unitItemOrder.TileObjectState == TileObjectState.Accept)
                    {
                        int maxTransferAmount = UnitOrders.GetAcceptedAmount(Ant.Unit, unitItemOrder.TileObjectType);
                        if (maxTransferAmount > 0)
                        {
                            needItems = true;
                            break;
                        }
                    }
                }
            }

            needItems = false;
            int disableDelivery = 0;

            if (needItems == false && runningGameCommand != null)
            {
                // Finish this command.
                runningGameCommand.CommandComplete = true;
            }
            if (needItems && runningGameCommand == null)
            {
                // Request delivery
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
                gameCommandItem.TargetUnit.SetUnitId(Ant.Unit.UnitId);
                gameCommandItem.TargetUnit.SetStatus("WaitingForDelivery");

                /*
                gameCommand.RequestedItems = new List<RecipeIngredient>();
                foreach (RecipeIngredient recipeIngredient in player.Game.RecipeForAnyUnit.Ingredients)
                {
                    gameCommand.RequestedItems.Add(recipeIngredient);
                }*/

                // Request would block factory commands.
                //Ant.Unit.SetGameCommand(gameCommandItem);

                gameCommand.GameCommandItems.Add(gameCommandItem);
                player.GameCommands.Add(gameCommand);
            }
            /*
            int items = Container.Unit.CountTileObjectsInContainer();
            int capacity = Container.Unit.CountCapacity();

            float urgency = 0;
            if (items > 0)
                urgency = (float)items / capacity;

            //if (items >= capacity)
            {
                AntNetworkNode.Demand(this, AntNetworkDemandType.Storage, urgency);
            }*/
            return false;
        }

    }
}

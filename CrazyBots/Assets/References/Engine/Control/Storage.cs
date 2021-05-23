using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Control
{
    public class Storage : Command
    {
        public Storage()
        {
            RequestBuilderUnit();
        }

        public override void DemandStartupUnits()
        {
            // Assembler
            UnitType unitType = new UnitType();
            unitType.MinAssemblerLevel = 1;
            unitType.MaxAssemblerLevel = 1;
            unitType.MinExtractorLevel = 3;
            unitType.MaxExtractorLevel = 3;
            DemandedUnitTypes.Add(unitType);

            // Container
            unitType = new UnitType();
            unitType.MinContainerLevel = 3;
            unitType.MaxContainerLevel = 3;
            unitType.MinExtractorLevel = 1;
            unitType.MaxExtractorLevel = 1;
            DemandedUnitTypes.Add(unitType);


            // Container x 2
            unitType = new UnitType();
            unitType.MinContainerLevel = 3;
            unitType.MaxContainerLevel = 3;
            unitType.MinExtractorLevel = 1;
            unitType.MaxExtractorLevel = 1;
            DemandedUnitTypes.Add(unitType);
        }

        public override void AttachUnits(Dispatcher dispatcher, Player player, List<PlayerUnit> moveableUnits)
        {
            base.AttachUnits(dispatcher, player, moveableUnits);
            if (WaitingForBuilder)
            {
                WaitForBuilder(dispatcher, moveableUnits);
            }
            else if (WaitingForDeconstrcut)
            {
                Deconstrcut(dispatcher, player, moveableUnits);
            }
            else
            {
                SanityCheck(dispatcher, player, moveableUnits);
                HandlyUnitsToExtract(dispatcher, player, moveableUnits);
            }
        }

        public override string ToString()
        {
            return "Storage";
        }
    }
}

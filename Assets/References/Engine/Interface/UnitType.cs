
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public class UnitType
    {
        public UnitType()
        {
            MaxEngineLevel = 0;
            MaxArmorLevel = 0;
            MaxWeaponLevel = 0;
            MaxAssemblerLevel = 0;
            MaxExtractorLevel = 0;
            MaxReactorLevel = 0;
            MaxRadarLevel = 0;
        }

        public int MinEngineLevel { get; set; }
        public int MaxEngineLevel { get; set; }

        public int MinArmorLevel { get; set; }
        public int MaxArmorLevel { get; set; }

        public int MinWeaponLevel { get; set; }
        public int MaxWeaponLevel { get; set; }

        public int MinAssemblerLevel { get; set; }
        public int MaxAssemblerLevel { get; set; }

        public int MinExtractorLevel { get; set; }
        public int MaxExtractorLevel { get; set; }


        public int MinContainerLevel { get; set; }
        public int MaxContainerLevel { get; set; }

        public int MinReactorLevel { get; set; }
        public int MaxReactorLevel { get; set; }

        public int MinRadarLevel { get; set; }
        public int MaxRadarLevel { get; set; }


        public bool Matches(PlayerUnit playerUnit)
        {
            bool matches = true;

            if (playerUnit.Unit.Engine == null)
            {
                if (MinEngineLevel > 0)
                    matches = false;
            }
            else
            {
                if (playerUnit.Unit.Engine.Level < MinEngineLevel)
                    matches = false;
                if (playerUnit.Unit.Engine.Level > MaxEngineLevel)
                    matches = false;
            }
            if (playerUnit.Unit.Armor == null)
            {
                if (MinArmorLevel > 0)
                    matches = false;
            }
            else
            {
                if (playerUnit.Unit.Armor.Level < MinArmorLevel)
                    matches = false;
                if (playerUnit.Unit.Armor.Level > MaxArmorLevel)
                    matches = false;
            }
            if (playerUnit.Unit.Weapon == null)
            {
                if (MinWeaponLevel > 0)
                    matches = false;
            }
            else
            {
                if (playerUnit.Unit.Weapon.Level < MinWeaponLevel)
                    matches = false;
                if (playerUnit.Unit.Weapon.Level > MaxWeaponLevel)
                    matches = false;
            }
            if (playerUnit.Unit.Assembler == null)
            {
                if (MinAssemblerLevel > 0)
                    matches = false;
            }
            else
            {
                if (playerUnit.Unit.Assembler.Level < MinAssemblerLevel)
                    matches = false;
                if (playerUnit.Unit.Assembler.Level > MaxAssemblerLevel)
                    matches = false;
            }
            if (playerUnit.Unit.Extractor == null)
            {
                if (MinExtractorLevel > 0)
                    matches = false;
            }
            else
            {
                if (playerUnit.Unit.Extractor.Level < MinExtractorLevel)
                    matches = false;
                if (playerUnit.Unit.Extractor.Level > MaxExtractorLevel)
                    matches = false;
            }
            if (playerUnit.Unit.Reactor == null)
            {
                if (MinReactorLevel > 0)
                    matches = false;
            }
            else
            {
                if (playerUnit.Unit.Reactor.Level < MinReactorLevel)
                    matches = false;
                if (playerUnit.Unit.Reactor.Level > MaxReactorLevel)
                    matches = false;
            }
            if (playerUnit.Unit.Radar == null)
            {
                if (MinRadarLevel > 0)
                    matches = false;
            }
            else
            {
                if (playerUnit.Unit.Radar.Level < MinRadarLevel)
                    matches = false;
                if (playerUnit.Unit.Radar.Level > MaxRadarLevel)
                    matches = false;
            }
            if (playerUnit.Unit.Container == null)
            {
                if (MinContainerLevel > 0)
                    matches = false;
            }
            else
            {
                if (playerUnit.Unit.Container.Level < MinContainerLevel)
                    matches = false;
                if (playerUnit.Unit.Container.Level > MaxContainerLevel)
                    matches = false;
            }
            return matches;
        }
        public bool Fits(PlayerUnit playerUnit)
        {
            bool fits = true;


            if (playerUnit.Unit.Engine != null &&
                playerUnit.Unit.Engine.Level > MaxEngineLevel)
                fits = false;
            //if (playerUnit.Unit.Engine == null && MinEngineLevel > 0)
            //    fits = false;

            if (playerUnit.Unit.Armor != null &&
                playerUnit.Unit.Armor.Level > MaxArmorLevel)
                fits = false;
            

            if (playerUnit.Unit.Weapon != null &&
                playerUnit.Unit.Weapon.Level > MaxWeaponLevel)
                fits = false;


            if (playerUnit.Unit.Assembler != null &&
                playerUnit.Unit.Assembler.Level > MaxAssemblerLevel)
                fits = false;


            if (playerUnit.Unit.Extractor != null &&
                playerUnit.Unit.Extractor.Level > MaxExtractorLevel)
                fits = false;


            if (playerUnit.Unit.Container != null &&
                playerUnit.Unit.Container.Level > MaxContainerLevel)
                fits = false;


            if (playerUnit.Unit.Reactor != null &&
                playerUnit.Unit.Reactor.Level > MaxReactorLevel)
                fits = false;

            if (playerUnit.Unit.Radar != null &&
                playerUnit.Unit.Radar.Level > MaxRadarLevel)
                fits = false;

            return fits;
        }

    }
}

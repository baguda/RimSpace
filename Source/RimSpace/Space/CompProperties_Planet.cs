using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RimWorld;
using Verse;

namespace RimSpace
{
    public class CompProperties_Planet : CompProperties
    {
        public CompProperties_Planet()
        {
            this.compClass = typeof(CompPlanet);
        }

        public string PlanetCategoryName = "Oceanic";

        public string PlanetName = "";
        public PlanetCategory getPlanetCategory
        {
            get
            {
                if (this.PlanetCategoryName.ToLower() == "oceanic")
                {
                    return PlanetCategory.OceanicWorld;
                }
                else if (this.PlanetCategoryName.ToLower() == "deadworld")
                {
                    return PlanetCategory.Deadworld;
                }
                else if (this.PlanetCategoryName.ToLower() == "junkworld")
                {
                    return PlanetCategory.JunkWorld;
                }
                else if (this.PlanetCategoryName.ToLower() == "toxicworld")
                {

                    return PlanetCategory.ToxicWorld;
                }
                else if (this.PlanetCategoryName.ToLower() == "iceworld")
                {
                    return PlanetCategory.Iceworld;
                }
                else if (this.PlanetCategoryName.ToLower() == "glassworld")
                {
                    return PlanetCategory.Glassworld;
                }

                else if (this.PlanetCategoryName.ToLower() == "industrialworld")
                {
                    return PlanetCategory.Glassworld;
                }
                else if (this.PlanetCategoryName.ToLower() == "midworld")
                {
                    return PlanetCategory.Midworld;
                }
                else if (this.PlanetCategoryName.ToLower() == "glitterworld")
                {
                    return PlanetCategory.Midworld;
                }
                else if (this.PlanetCategoryName.ToLower() == "farmingworld")
                {
                    return PlanetCategory.Midworld;
                }
                else if (this.PlanetCategoryName.ToLower() == "mineralworld")
                {
                    return PlanetCategory.Midworld;
                }

                else if (this.PlanetCategoryName.ToLower() == "glitterworld")
                {
                    return PlanetCategory.Midworld;
                }
                else if (this.PlanetCategoryName.ToLower() == "farmingworld")
                {
                    return PlanetCategory.Midworld;
                }
                else if (this.PlanetCategoryName.ToLower() == "mineralworld")
                {
                    return PlanetCategory.Midworld;
                }

                else if (this.PlanetCategoryName.ToLower() == "animalworld")
                {
                    return PlanetCategory.Midworld;
                }
                else if (this.PlanetCategoryName.ToLower() == "medievalworld")
                {
                    return PlanetCategory.Midworld;
                }
                else if (this.PlanetCategoryName.ToLower() == "rimworld")
                {
                    return PlanetCategory.Midworld;
                }
                else return PlanetCategory.OceanicWorld;

            }

        }
        public bool isHome = false;
        public string GroundMapWorldObjectDefName;
        public string GroundMapGeneratorDefName ="";

        public string BiomeDefName;
        public float Tempureture = 10f;
        public float Elevation = 1f;
        public float Rainfall = 0.01f;
        public float Swampiness = 1f;
        public Hilliness Hilliness = Hilliness.Flat;


        private Pawn GenerateTrader(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, TraderKindDef traderKind)
        {
            PawnKindDef kind = groupMaker.traders.RandomElementByWeight((PawnGenOption x) => x.selectionWeight).kind;
            Faction faction = parms.faction;
            PawnGenerationContext context = PawnGenerationContext.NonPlayer;
            Ideo ideo = parms.ideo;
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, context, parms.tile, false, false, false, true, false, 1f, false, true, false, true, true, parms.inhabitants, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, ideo, false, false, false, false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, false));
            pawn.mindState.wantsToTradeWithColony = true;
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
            pawn.trader.traderKind = traderKind;
            parms.points -= pawn.kindDef.combatPower;
            return pawn;
        }

    }
}

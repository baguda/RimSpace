using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimSpace
{
    public class CompProperties_Trader : CompProperties
    {

        public CompProperties_Trader()
        {
            this.compClass = typeof(CompTrader);
        }

        public string TraderKindDefName;
        public List<Thing> Goods;
        public string TraderName = "Trader";
        public float TradePriceImprovementOffsetForPlayer = -0.1f;
        public string FactionDefName;
        public bool UseFavorTradeCurrency = false;
    }
}

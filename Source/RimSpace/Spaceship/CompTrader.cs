using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace RimSpace
{
	public class CompTrader : ThingComp, ITrader, IThingHolder, IExposable
	{
		private ThingOwner<Thing> stock;
		private int lastStockGenerationTicks = -1;
		private bool everGeneratedStock;
		private const float DefaultTradePriceImprovement = 0.02f;
		private List<Pawn> tmpSavedPawns = new List<Pawn>();
		private TraderKindDef traderKind;
		public CompSpaceship compSpaceship => this.parent.GetComp<CompSpaceship>();
		public bool isSpaceship => compSpaceship != null;
		private Faction faction;

		public Pawn ProxPawnToTrade => this.parent.Map.GetComponent<MapComp_SpaceMap>().playerShips.Find(s => GenRadial.RadialCellsAround(this.parent.Position, 4f, false).ToList().Contains(s.Position));

		public CompProperties_Trader Props => this.props as CompProperties_Trader;
		public TraderKindDef TraderKind
		{
			get
			{
				if (traderKind == null)
				{
					if (Props.TraderKindDefName != null)
					{
						traderKind = DefDatabase<TraderKindDef>.GetNamed(Props.TraderKindDefName);
					}
					else
					{
						traderKind =  DefDatabase<TraderKindDef>.GetRandom();
					}
				}
				return traderKind;
				
			}
		}
		public IEnumerable<Thing> Goods
		{
			get
			{
				if (Props.Goods != null)
				{
					foreach (var thing in Props.Goods)
					{
						yield return thing;
					}
				}
				else
				{
					foreach (var thing in stock)
					{
						yield return thing;
					}
				}
				/*
				List<Thing> inv = AllInventoryItems(this.compSpaceship.CrewList);
				int num;
				for (int i = 0; i < inv.Count; i = num + 1)
				{
					yield return inv[i];
					num = i;
				}
				List<Pawn> pawns = this.compSpaceship.CrewList;
				for (int i = 0; i < pawns.Count; i = num + 1)
				{
					Pawn pawn = pawns[i];
					if ( (!pawn.RaceProps.packAnimal || pawn.inventory == null || pawn.inventory.innerContainer.Count <= 0))
					{
						yield return pawn;
					}
					num = i;
				}
				yield break;

				*/



			}
		}
		public static List<Thing> AllInventoryItems(List<Pawn> pawnsListForReading)
		{
			List<Thing> result = new List<Thing>();
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				Pawn pawn = pawnsListForReading[i];
				for (int j = 0; j < pawn.inventory.innerContainer.Count; j++)
				{
					Thing item = pawn.inventory.innerContainer[j];
					result.Add(item);
				}
			}
			return result;
		}
		public int RandomPriceFactorSeed => Rand.Int;
		public string TraderName => Props.TraderName;

		public float TradePriceImprovementOffsetForPlayer => Props.TradePriceImprovementOffsetForPlayer;
		public Faction Faction
		{
			get
			{
				if (faction == null)
                {
					if (Props.FactionDefName != null) faction = Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed(Props.FactionDefName));
					else faction = Find.FactionManager.RandomEnemyFaction();
				}

				return faction;
			}
		}
		public TradeCurrency TradeCurrency => Props.UseFavorTradeCurrency ? TradeCurrency.Favor : TradeCurrency.Silver;

		public ThingOwner GetDirectlyHeldThings() => this.stock;
		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}
		public virtual void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.tmpSavedPawns.Clear();
				if (this.stock != null)
				{
					for (int i = this.stock.Count - 1; i >= 0; i--)
					{
						Pawn pawn = this.stock[i] as Pawn;
						if (pawn != null)
						{
							this.stock.Remove(pawn);
							this.tmpSavedPawns.Add(pawn);
						}
					}
				}
			}
			Scribe_Collections.Look<Pawn>(ref this.tmpSavedPawns, "tmpSavedPawns", LookMode.Reference, Array.Empty<object>());
			Scribe_Deep.Look<ThingOwner<Thing>>(ref this.stock, "stock", Array.Empty<object>());
			Scribe_Values.Look<int>(ref this.lastStockGenerationTicks, "lastStockGenerationTicks", 0, false);
			Scribe_Values.Look<bool>(ref this.everGeneratedStock, "wasStockGeneratedYet", false, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.Saving)
			{
				for (int j = 0; j < this.tmpSavedPawns.Count; j++)
				{
					this.stock.TryAdd(this.tmpSavedPawns[j], false);
				}
				this.tmpSavedPawns.Clear();
			}
		}

		protected virtual int RegenerateStockEveryDays => 1;
		public List<Thing> StockListForReading
		{
			get
			{
				if (this.stock == null)
				{
					this.RegenerateStock();
				}
				return this.stock.InnerListForReading;
			}
		}
		public bool EverVisited => this.everGeneratedStock;
		public bool RestockedSinceLastVisit => this.everGeneratedStock && this.stock == null;
		public int NextRestockTick
		{
			get
			{
				if (this.stock == null || !this.everGeneratedStock)
				{
					return -1;
				}
				return ((this.lastStockGenerationTicks == -1) ? 0 : this.lastStockGenerationTicks) + this.RegenerateStockEveryDays * 60000;
			}
		}
		public virtual bool CanTradeNow => true;//this.TraderKind != null && (this.stock == null || this.stock.InnerListForReading.Any((Thing x) => this.TraderKind.WillTrade(x.def)));
		public virtual IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			/*
			if (ProxPawnToTrade != null)
            {
				foreach(var thing in ProxPawnToTrade.GetComp<CompSpaceship>().ContentsList)
                {
					yield return thing;
				}
            }
			*/
			Caravan playerCaravan = playerNegotiator.GetCaravan();
			foreach (Thing thing in playerNegotiator.GetComp<CompSpaceship>().ContentsList)
			{
				yield return thing;
			}
			
			foreach(Pawn pawn in playerNegotiator.GetComp<CompSpaceship>().CrewList)
            {
				yield return pawn;
			}
			Find.WorldGrid.tiles.Add(new Tile()); 
			yield break;
		}
		public virtual void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			if (this.stock == null)
			{
				this.RegenerateStock();
			}
			Caravan caravan = playerNegotiator.GetCaravan();

			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
			Pawn pawn = toGive as Pawn;
			if (pawn != null)
			{
				CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawn, caravan.PawnsListForReading, null);
				if (pawn.RaceProps.Humanlike)
				{
					return;
				}
				if (!this.stock.TryAdd(pawn, false))
				{
					pawn.Destroy(DestroyMode.Vanish);
					return;
				}
			}
			else if (!this.stock.TryAdd(thing, false))
			{
				thing.Destroy(DestroyMode.Vanish);
			}
		}
		public virtual void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			Caravan caravan = playerNegotiator.GetCaravan();
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				caravan.AddPawn(pawn, true);
				return;
			}
			Pawn pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(thing, caravan.PawnsListForReading, null, null);
			if (pawn2 == null)
			{
				Log.Error("Could not find any pawn to give sold thing to.");
				thing.Destroy(DestroyMode.Vanish);
				return;
			}
			if (!pawn2.inventory.innerContainer.TryAdd(thing, true))
			{
				Log.Error("Could not add sold thing to inventory.");
				thing.Destroy(DestroyMode.Vanish);
			}
		}
		public virtual void TraderTrackerTick()
		{
			if (this.stock != null)
			{
				if (Find.TickManager.TicksGame - this.lastStockGenerationTicks > this.RegenerateStockEveryDays * 60000)
				{
					this.TryDestroyStock();
					return;
				}
				for (int i = this.stock.Count - 1; i >= 0; i--)
				{
					Pawn pawn = this.stock[i] as Pawn;
					if (pawn != null && pawn.Destroyed)
					{
						this.stock.Remove(pawn);
					}
				}
				for (int j = this.stock.Count - 1; j >= 0; j--)
				{
					Pawn pawn2 = this.stock[j] as Pawn;
					if (pawn2 != null && !pawn2.IsWorldPawn())
					{
						Log.Error("Faction base has non-world-pawns in its stock. Removing...");
						this.stock.Remove(pawn2);
					}
				}
			}
		}
		public void TryDestroyStock()
		{
			if (this.stock != null)
			{
				for (int i = this.stock.Count - 1; i >= 0; i--)
				{
					Thing thing = this.stock[i];
					this.stock.Remove(thing);
					if (!(thing is Pawn) && !thing.Destroyed)
					{
						thing.Destroy(DestroyMode.Vanish);
					}
				}
				this.stock = null;
			}
		}
		public bool ContainsPawn(Pawn p)
		{
			return this.stock != null && this.stock.Contains(p);
		}
		protected virtual void RegenerateStock()
		{
			this.TryDestroyStock();
			this.stock = new ThingOwner<Thing>(this);
			this.everGeneratedStock = true;
			if (this.Faction == null || !this.Faction.IsPlayer)
			{
				ThingSetMakerParams parms = default(ThingSetMakerParams);
				parms.traderDef = this.TraderKind;
				parms.tile = 1;
				parms.makingFaction = this.Faction;
				this.stock.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms), true, false);
			}
			for (int i = 0; i < this.stock.Count; i++)
			{
				Pawn pawn = this.stock[i] as Pawn;
				if (pawn != null)
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
				}
			}
			this.lastStockGenerationTicks = Find.TickManager.TicksGame;
		}

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
			this.RegenerateStock();

			base.PostSpawnSetup(respawningAfterLoad);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
			{
				yield return gizmo;
			}
			if (ProxPawnToTrade != null)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "Trade";
				command_Action.defaultDesc = "Trade With Planet";
				command_Action.icon = CompLaunchShip.LaunchCommandTex;
				command_Action.alsoClickIfOtherInGroupClicked = false;
				command_Action.action = delegate ()
				{
					Find.WindowStack.Add(new Dialog_Trade(ProxPawnToTrade, this, false));
					//PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(this.Goods.OfType<Pawn>(), "LetterRelatedPawnsTradeShip".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, false, true);
				};
				yield return command_Action;
			}
			yield break;
        }
    }


	public class CompPlanetTrader : CompTrader
	{

	}

}
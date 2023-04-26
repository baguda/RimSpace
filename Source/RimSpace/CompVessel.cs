using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;


namespace RimSpace
{
	public class CompVessel : ThingComp, IThingHolder, IOpenable
	{
		public ThingOwner innerContainer;
		protected bool contentsKnown;
		public string openedSignal;
		public List<Manager> managers = new List<Manager>();
		public Manager GetManager(ManagerType type) => this.managers.Find(s => s.MgrType == type);

		public Pawn pawn => this.parent as Pawn;
		public Map map => this.pawn.Map;
		public virtual int OpenTicks => 60;
		public CompVessel()
		{
			this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
		}
		public bool HasAnyContents => this.innerContainer.Count > 0;
		public Thing ContainedThing => this.innerContainer.Count != 0 ? this.innerContainer[0] : null;
		public virtual bool CanOpen => this.HasAnyContents;
		public ThingOwner GetDirectlyHeldThings() => this.innerContainer;
		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}
		public override void CompTickRare()
		{
			base.CompTickRare();
			this.innerContainer.ThingOwnerTickRare(true);
		}
		public override void CompTick()
		{
			base.CompTick();
			this.innerContainer.ThingOwnerTick(true);
		}
		public virtual void Open()
		{
			if (!this.HasAnyContents)
			{
				return;
			}
			this.EjectContents();
			if (!this.openedSignal.NullOrEmpty())
			{
				Find.SignalManager.SendSignal(new Signal(this.openedSignal, this.Named("SUBJECT")));
			}
		}
		public override void PostExposeData()
		{
			base.PostExposeData(); Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
			 {
				this
			 });
			Scribe_Values.Look<bool>(ref this.contentsKnown, "contentsKnown", false, false);
			Scribe_Values.Look<string>(ref this.openedSignal, "openedSignal", null, false);

		}
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
			{
				yield return gizmo;
			}
			Gizmo gizmo2;
			if ((gizmo2 = Building.SelectContainedItemGizmo(parent, this.ContainedThing)) != null)
			{
				yield return gizmo2;
			}
			if (DebugSettings.ShowDevGizmos && this.CanOpen)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Open",
					action = delegate ()
					{
						this.Open();
					}
				};
			}
			yield break;
		}
		public virtual bool Accepts(Thing thing)
		{
			return this.innerContainer.CanAcceptAnyOf(thing, true);
		}
		public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (!this.Accepts(thing))
			{
				return false;
			}
			bool flag;
			if (thing.holdingOwner != null)
			{
				thing.holdingOwner.TryTransferToContainer(thing, this.innerContainer, thing.stackCount, true);
				flag = true;
			}
			else
			{
				flag = this.innerContainer.TryAdd(thing, true);
			}
			if (flag)
			{
				if (thing.Faction != null && thing.Faction.IsPlayer)
				{
					this.contentsKnown = true;
				}
				return true;
			}
			return false;
		}
		public virtual void EjectContents()
		{
			this.innerContainer.TryDropAll(this.parent.InteractionCell, map, ThingPlaceMode.Near, null, null, true);
			this.contentsKnown = true;
		}
		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			if (this.innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
			{
				if (mode != DestroyMode.Deconstruct)
				{
					List<Pawn> list = new List<Pawn>();
					foreach (Thing thing in ((IEnumerable<Thing>)this.innerContainer))
					{
						Pawn pawn = thing as Pawn;
						if (pawn != null)
						{
							list.Add(pawn);
						}
					}
					foreach (Pawn p in list)
					{
						HealthUtility.DamageUntilDowned(p, true);
					}
				}
				this.EjectContents();
			}
			this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
			base.PostDestroy(mode, previousMap);
		}
		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			string str;
			if (!this.contentsKnown)
			{
				str = "UnknownLower".Translate();
			}
			else
			{
				str = this.innerContainer.ContentsString;
			}
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			return text + ("CasketContains".Translate() + ": " + str.CapitalizeFirst());

		}
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (parent.Faction != null && parent.Faction.IsPlayer)
			{
				this.contentsKnown = true;
			}
			//this.pawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, this.pawn);
		}

		/*
		public override void ReceiveCompSignal(string signal)
		{
		}

		public override void PostExposeData()
		{
		}



		public override void PostDeSpawn(Map map)
		{
		}


		public override void PostPostMake()
		{
		}

		public override void CompTick()
		{
		}

		public override void CompTickRare()
		{
		}

		public override void CompTickLong()
		{
		}

		public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			absorbed = false;
		}

		public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
		}

		public override void PostDraw()
		{
		}

		public override void PostDrawExtraSelectionOverlays()
		{
		}

		public override void PostPrintOnto(SectionLayer layer)
		{
		}

		public override void CompPrintForPowerGrid(SectionLayer layer)
		{
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
		}

		public override void PostSplitOff(Thing piece)
		{
		}

		public override string TransformLabel(string label)
		{
			return base.TransformLabel(label);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			return base.CompGetGizmosExtra();
		}

		public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
			return base.CompGetWornGizmosExtra();
		}

		public override bool AllowStackWith(Thing other)
		{
			return true;
		}


		public override string GetDescriptionPart()
		{
			return null;
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			return Enumerable.Empty<FloatMenuOption>();
		}

		public override IEnumerable<FloatMenuOption> CompMultiSelectFloatMenuOptions(List<Pawn> selPawns)
		{
			return Enumerable.Empty<FloatMenuOption>();
		}

		public override void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
		}

		public override void PrePostIngested(Pawn ingester)
		{
		}

		public override void PostIngested(Pawn ingester)
		{
		}

		public override void PostPostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
		{
		}

		public override void Notify_SignalReceived(Signal signal)
		{
		}

		public override void Notify_LordDestroyed()
		{
		}

		public override void Notify_MapRemoved()
		{
		}

		public override void DrawGUIOverlay()
		{
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			return null;
		}

		public override void Notify_Equipped(Pawn pawn)
		{
		}

		public override void Notify_Unequipped(Pawn pawn)
		{
		}

		public override void Notify_UsedWeapon(Pawn pawn)
		{
		}

		public override void Notify_KilledPawn(Pawn pawn)
		{
		}

		public override void Notify_WearerDied()
		{
		}

		public override void Notify_AddBedThoughts(Pawn pawn)
		{
		}

		public override void Notify_AbandonedAtTile(int tile)
		{
		}

		public override void Notify_KilledLeavingsLeft(List<Thing> leavings)
		{
		}

		public override void CompDrawWornExtras()
		{
		}

		public override bool CompAllowVerbCast(Verb verb)
		{
			return true;
		}

		public override bool CompPreventClaimingBy(Faction faction)
		{
			return false;
		}

		public override float CompGetSpecialApparelScoreOffset()
		{
			return 0f;
		}
		*/

	}
}

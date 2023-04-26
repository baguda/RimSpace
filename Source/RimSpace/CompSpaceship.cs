using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse.AI;

namespace RimSpace
{
	public class CompSpaceship : CompVessel
	{
		public CompProperties_Spaceship Props => this.props as CompProperties_Spaceship;


		// Managers

		public Manager_Shields shields => this.managers.Find(s => s.MgrType == ManagerType.Shields) as Manager_Shields;
		public Manager_Energy energy => this.managers.Find(s => s.MgrType == ManagerType.Energy) as Manager_Energy;
		public Manager_LifeSupport lifeSupport => this.managers.Find(s => s.MgrType == ManagerType.LifeSupport) as Manager_LifeSupport;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			foreach (var mgr in managers)
			{
				mgr.Setup(respawningAfterLoad);
			}
			//this.pawn.mechanitor = new Pawn_SpaceshipTracker();
		}
		public override void CompTick()
		{
			if (this.Overseen) overseer.Position = this.parent.Position;
			foreach (var mgr in this.managers)
			{
				mgr.ManagerTick();
			}
			if (inSpace)
				base.CompTick();

            if (!pawn.Drafted) { pawn.drafter.Drafted = true;  }
		}
		public override void Initialize(CompProperties props)
		{
			this.managers.Add(new Manager_Shields(this.pawn));
			this.managers.Add(new Manager_Energy(this.pawn));
			this.managers.Add(new Manager_LifeSupport(this.pawn));
			base.Initialize(props);
		}
		public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			//if (Rand.Chance(Math.Max(0, Math.Min(0.0f, 0.9f - pawn.HealthScale)))) (this.managers.Find(s => s.MgrType == ManagerType.LifeSupport) as Manager_LifeSupport).consoleExplodes(totalDamageDealt, dinfo);
		}
		public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			Manager_Shields shields = managers.Find(m => m.MgrType == ManagerType.Shields) as Manager_Shields;
			float trample = shields.DamageShields(dinfo).Amount;
			if (trample > 0)
			{
				absorbed = false;
				return;
			}
			absorbed = true;


		}

		private static readonly Material BubbleMaterial = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);




		// container

		public int Count => this.innerContainer.Count;
		public bool hasContents => this.innerContainer.Any();
		public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (base.TryAcceptThing(thing, allowSpecialEffects))
			{
				if (allowSpecialEffects)
				{
					SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(parent.Position, map, false));
				}
				return true;
			}
			return false;
		}
		public static Building_CryptosleepCasket FindCryptosleepCasketFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
		{

			foreach (ThingDef singleDef in from def in DefDatabase<ThingDef>.AllDefs
										   where def.IsCryptosleepCasket
										   select def)
			{
				IntVec3 positionHeld = p.PositionHeld;
				Map mapHeld = p.MapHeld;
				ThingRequest thingReq = ThingRequest.ForDef(singleDef);
				PathEndMode peMode = PathEndMode.InteractionCell;
				TraverseParms traverseParams = TraverseParms.For(traveler, Danger.Deadly, TraverseMode.ByPawn, false, false, false);
				float maxDistance = 9999f;
				Predicate<Thing> validator = ((Thing x) => !((Building_CryptosleepCasket)x).HasAnyContents && traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations));

				Building_CryptosleepCasket building_CryptosleepCasket = (Building_CryptosleepCasket)GenClosest.ClosestThingReachable(positionHeld, mapHeld, thingReq, peMode, traverseParams, maxDistance, validator, null, 0, -1, false, RegionType.Set_Passable, false);
				if (building_CryptosleepCasket != null)
				{
					return building_CryptosleepCasket;
				}
			}
			return null;
		}
		public List<Pawn> CrewList 
		{
			get
			{
				List<Pawn> result = new List<Pawn>();
				foreach (Thing thing in this.innerContainer.ToList().FindAll(s => s is Pawn))
				{

					result.Add(thing as Pawn);
				}
				return result;

			}
		}
		public override int OpenTicks => Props.getReadyTime;
		public override void EjectContents()
		{
			ThingDef filth_Slime = ThingDefOf.Filth_Slime;
			foreach (Thing thing in ((IEnumerable<Thing>)this.innerContainer))
			{
				Pawn pawn = thing as Pawn;
				if (pawn != null)
				{
					PawnComponentsUtility.AddComponentsForSpawn(pawn);
					pawn.filth.GainFilth(filth_Slime);
					if (pawn.RaceProps.IsFlesh)
					{
						//pawn.health.AddHediff(HediffDefOf.CryptosleepSickness, null, null, null);
					}
				}
			}
			if (!parent.Destroyed)
			{
				SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(parent.Position, map, false), MaintenanceType.None));
			}
			base.EjectContents();
		}

		public Pawn Pilot => CrewList.Find(s => s.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant));
		public bool HasPilot => Pilot != null;


		public bool inSpace => this.map.Parent.def.defName.Equals("OrbitalLocation");

		



	
		public void DrawCommandRadius()
		{
			if (this.pawn.Spawned && this.pawn.Drafted)
			{
				GenDraw.DrawRadiusRing(this.pawn.Position, 24.9f, Color.white, (IntVec3 c) => this.CanCommandTo(c));

			}
		}
		public bool CanCommandTo(LocalTargetInfo target)
		{
			return target.Cell.InBounds(this.pawn.MapHeld) && (float)this.pawn.Position.DistanceToSquared(target.Cell) < 620.01f;
		}		
		public Pawn overseer => this.pawn.GetOverseer();
		public bool hasOverseer => overseer != null;
		public bool Overseen => hasOverseer ? this.CrewList.FindAll(s => s.ThingID == this.overseer.ThingID).Any() : false;



		




		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
		{
			if (myPawn.IsQuestLodger())
			{
				FloatMenuOption floatMenuOption = new FloatMenuOption("CannotUseReason".Translate("CryptosleepCasketGuestsNotAllowed".Translate()), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
				yield return floatMenuOption;
				yield break;
			}
			foreach (FloatMenuOption floatMenuOption2 in base.CompFloatMenuOptions(myPawn))
			{
				yield return floatMenuOption2;
			}
			if (this.innerContainer.Count < this.Props.getCrewSize && this.overseer != null)
			{
				if (!myPawn.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
				{
					FloatMenuOption floatMenuOption3 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
					yield return floatMenuOption3;
				}
				else
				{
					JobDef jobDef = DefDatabase<JobDef>.GetNamed("EnterCrew");    //JobDefOf.EnterCryptosleepCasket;
					string label = "Enter Vessel";
					Action action = delegate ()
					{
						if (!ModsConfig.BiotechActive)
						{
							myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, parent), new JobTag?(JobTag.Misc), false);
							return;
						}
						Hediff_PsychicBond hediff_PsychicBond = myPawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond, false) as Hediff_PsychicBond;
						if (hediff_PsychicBond == null || !ThoughtWorker_PsychicBondProximity.NearPsychicBondedPerson(myPawn, hediff_PsychicBond))
						{
							myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, parent), new JobTag?(JobTag.Misc), false);
							return;
						}
						WindowStack windowStack = Find.WindowStack;
						TaggedString text = "PsychicBondDistanceWillBeActive_Cryptosleep".Translate(myPawn.Named("PAWN"), ((Pawn)hediff_PsychicBond.target).Named("BOND"));
						Action confirmedAct = delegate ()
						{
							myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, parent), new JobTag?(JobTag.Misc), false);
						};

						windowStack.Add(Dialog_MessageBox.CreateConfirmation(text, confirmedAct, true, null, WindowLayer.Dialog));
					};
					yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), myPawn, parent, "ReservedBy", null);
				}
			}
			yield break;
		}
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetGizmosExtra())
			{
				yield return gizmo;
			}
			if (parent.Faction == Faction.OfPlayer && this.innerContainer.Count > 0 && true)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.action = new Action(this.EjectContents);
				command_Action.defaultLabel = Props.getEjectLabel;
				command_Action.defaultDesc = Props.getEjectDesc; ;
				if (this.innerContainer.Count == 0)
				{
					command_Action.Disable("CommandPodEjectFailEmpty".Translate());
				}
				if ((this.map.GetComponent<MapComp_SpaceMap>() as MapComp_SpaceMap).isSpace)
				{
					command_Action.Disable("Cannot Eject Crew Into Space...");
				}
				command_Action.hotKey = KeyBindingDefOf.Misc8;
				command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
				yield return command_Action;
			}
			//----
			
			if (this.pawn.drafter == null) this.pawn.drafter = new Pawn_DraftController(this.pawn);
			Command_Toggle command_Toggle = new Command_Toggle();
			command_Toggle.hotKey = KeyBindingDefOf.Command_ColonistDraft;
			command_Toggle.isActive = (() => this.pawn.drafter.Drafted);
			command_Toggle.toggleAction = delegate ()
			{
				this.pawn.drafter.Drafted = !this.pawn.drafter.Drafted;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Drafting, KnowledgeAmount.SpecificInteraction);
				if (this.pawn.drafter.Drafted)
				{
					LessonAutoActivator.TeachOpportunity(ConceptDefOf.QueueOrders, OpportunityType.GoodToKnow);
				}
			};
			command_Toggle.defaultDesc = "CommandToggleDraftDesc".Translate();
			command_Toggle.icon = TexCommand.Draft;
			command_Toggle.turnOnSound = SoundDefOf.DraftOn;
			command_Toggle.turnOffSound = SoundDefOf.DraftOff;
			command_Toggle.groupKeyIgnoreContent = 81729172;
			command_Toggle.defaultLabel = (this.pawn.drafter.Drafted ? "CommandUndraftLabel" : "CommandDraftLabel").Translate();
			if (this.pawn.Downed)
			{
				command_Toggle.Disable("IsIncapped".Translate(this.pawn.LabelShort, this.pawn));
			}
			if (this.pawn.Deathresting)
			{
				command_Toggle.Disable("IsDeathresting".Translate(this.pawn.Named("PAWN")));
			}
			if (!Overseen)
			{
				command_Toggle.Disable("Must have overseer inside");
			}
			if (!this.pawn.drafter.Drafted)
			{
				command_Toggle.tutorTag = "Draft";
			}
			else
			{
				command_Toggle.tutorTag = "Undraft";
			}
			yield return command_Toggle;
			
			//----
			yield break;
		}

		public override IEnumerable<FloatMenuOption> CompMultiSelectFloatMenuOptions(List<Pawn> selPawns)
		{
			return Enumerable.Empty<FloatMenuOption>();
		}
		public override void PostDraw()
		{

			float num = Mathf.Lerp(0.5f, 1.5f, 1f);// shields.shields.CurLevel);//this.Props.minDrawSize, this.Props.maxDrawSize, this.energy);
			Vector3 vector = this.pawn.Drawer.DrawPos;
			vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
			int num2 = 4; // Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
			if (num2 < 8)
			{
				float num3 = (float)(8 - num2) / 8f * 0.05f;
				//vector += this.impactAngleVect * num3;
				num -= num3;
			}
			float angle = (float)Rand.Range(0, 360);
			Vector3 s = new Vector3(num, 1f, num);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, CompSpaceship.BubbleMaterial, 0);

			DrawCommandRadius();




		}



	}


}
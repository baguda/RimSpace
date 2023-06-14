using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using Verse.AI;

namespace RimSpace
{
	public class JobDriver_EnterCrew : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed);
		}
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Log.Message("JobDriver_EnterCrew.MakeNewToils 1");
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil toil = Toils_General.Wait(500, TargetIndex.None);
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			toil.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return toil;
			Toil enter = ToilMaker.MakeToil("MakeNewToils");
			enter.initAction = delegate ()
			{
				Log.Message("JobDriver_EnterCrew.MakeNewToils 2");
				Pawn actor = enter.actor;
				ThingWithComps pod = (ThingWithComps)actor.CurJob.targetA.Thing;
				Log.Message("JobDriver_EnterCrew.MakeNewToils 3");
				Action action = delegate ()
				{
					bool flag = actor.DeSpawnOrDeselect(DestroyMode.Vanish);
					if (pod.GetComp<CompSpaceship>().TryAcceptThing(actor, true) && flag)
					{
						Find.Selector.Select(actor, false, false);
					}
				};
				//if (pod.def.building.isPlayerEjectable)
				{
					//action();
					//return;
				}
				if (this.Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate(actor.Named("PAWN")).AdjustedFor(actor, "PAWN", true), action, false, null, WindowLayer.Dialog));
					return;
				}
				action();
			};
			enter.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return enter;
			yield break;
		}
	}


}

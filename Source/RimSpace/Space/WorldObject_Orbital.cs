using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace RimSpace
{
	[StaticConstructorOnStartup]
    public class WorldObject_Orbital : MapParent
    {

        public Tile realTile;
		public string Name;
		public Vector3 DrawOrbitPos;
		public IntVec3 initPosition = new IntVec3(0,90,-200);
		public float randomCoef = 0.0f;
		public Vector3 maxOrbitAmplitudes;
		public Vector3 shiftOrbits;
		public float period;	
		private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
		public int timeOffset = 0;
		public bool disableOrbit = true;

		public override Vector3 DrawPos => disableOrbit ? DrawOrbitPos : this.calcParametricEllipse(this.maxOrbitAmplitudes, this.shiftOrbits, this.period, this.timeOffset);
		public virtual bool Visitable => base.Faction != Faction.OfPlayer && (base.Faction == null || !base.Faction.HostileTo(Faction.OfPlayer));


		public override void Draw()
		{
			float averageTileSize = Find.WorldGrid.averageTileSize;
			float transitionPct = ExpandableWorldObjectsUtility.TransitionPct;
			if (this.def.expandingIcon && transitionPct > 0f)
			{
				Color color = this.Material.color;
				float num = 1f - transitionPct;
				WorldObject_Orbital.propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * num));
				WorldRendererUtility.DrawQuadTangentialToPlanet(this.DrawPos, 0.7f * averageTileSize, 1100f, this.Material, false, false, null);
				return;
			}
			WorldRendererUtility.DrawQuadTangentialToPlanet(this.DrawPos, 0.7f * averageTileSize,1100f, this.Material, false, false, null);
		}

        public override void SpawnSetup()
        {
			this.DrawOrbitPos = Find.WorldGrid.GetTileCenter(this.Tile) + initPosition.ToVector3() + (Rand.UnitVector3 * randomCoef);
			base.SpawnSetup();
        }
		public Vector3 calcParametricEllipse(Vector3 max, Vector3 shift, float Period, int timeOffset)
		{
			Vector3 vec3 = default(Vector3);
			int time = Find.TickManager.TicksGame;
			vec3.x = max.x * (float)Math.Cos((double)(6.28f / Period * (float)(time + timeOffset))) + shift.x;
			vec3.z = max.z * (float)Math.Sin((double)(6.28f / Period * (float)(time + timeOffset))) + shift.z;
			vec3.y = max.y * (float)Math.Cos((double)(6.28f / Period * (float)(time + timeOffset))) + shift.y;
			return vec3;
		}
	}

	public class WorldObject_Wormhole : WorldObject_Orbital
	{

		private int TickOfCollapse = -1;
		public int Duration = -1;
		public int RemainingDuration = -1;

		private bool inSustain = false;

        public override void Tick()
        {
        

            base.Tick();
        }

		public override void SpawnSetup()
		{

			base.SpawnSetup();
		}

	}



	public class WorldObject_HiddenPlanet : MapParent
	{

		public Tile realTile;
		public string Name;
		public Vector3 DrawOrbitPos;
		public IntVec3 initPosition = new IntVec3(0, 90, -200);
		public float randomCoef = 0.0f;
		public Vector3 maxOrbitAmplitudes;
		public Vector3 shiftOrbits;
		public float period;
		private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
		public int timeOffset = 0;
		public bool disableOrbit = true;

		public override Vector3 DrawPos => disableOrbit ? DrawOrbitPos : this.calcParametricEllipse(this.maxOrbitAmplitudes, this.shiftOrbits, this.period, this.timeOffset);
		public virtual bool Visitable => base.Faction != Faction.OfPlayer && (base.Faction == null || !base.Faction.HostileTo(Faction.OfPlayer));


		public override void Draw()
		{
			float averageTileSize = Find.WorldGrid.averageTileSize;
			float transitionPct = ExpandableWorldObjectsUtility.TransitionPct;
			if (this.def.expandingIcon && transitionPct > 0f)
			{
				Color color = this.Material.color;
				float num = 1f - transitionPct;
				WorldObject_HiddenPlanet.propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * num));
				WorldRendererUtility.DrawQuadTangentialToPlanet(this.DrawPos, 0.7f * averageTileSize, 1100f, this.Material, false, false, null);
				return;
			}
			WorldRendererUtility.DrawQuadTangentialToPlanet(this.DrawPos, 0.7f * averageTileSize, 1100f, this.Material, false, false, null);
		}

		public override void SpawnSetup()
		{
			this.DrawOrbitPos = new Vector3();
			base.SpawnSetup();
		}
		public Vector3 calcParametricEllipse(Vector3 max, Vector3 shift, float Period, int timeOffset)
		{
			Vector3 vec3 = default(Vector3);
			int time = Find.TickManager.TicksGame;
			vec3.x = max.x * (float)Math.Cos((double)(6.28f / Period * (float)(time + timeOffset))) + shift.x;
			vec3.z = max.z * (float)Math.Sin((double)(6.28f / Period * (float)(time + timeOffset))) + shift.z;
			vec3.y = max.y * (float)Math.Cos((double)(6.28f / Period * (float)(time + timeOffset))) + shift.y;
			return vec3;
		}
	}



}

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorld.BaseGen;

namespace RimSpace
{
	public class GenStep_StarSystem : GenStep
	{
		public override int SeedPart => 826504671;
		public GameComp_StarSystem SpaceComp => Current.Game.GetComponent<GameComp_StarSystem>() as GameComp_StarSystem;
		public List<string> PlanetList => SpaceComp.def.getSpaceObjectDefNames;
		public List<string> usedPlanetList = new List<string>();

		public override void Generate(Map map, GenStepParams parms)
		{
			MapComp_SpaceMap mapComp = map.GetComponent<MapComp_SpaceMap>();



			MapToolBag.MapWorkerUtility.AdaptiveGen(map, map.Center, "RimSun");
			MapToolBag.MapWorkerUtility.AdaptiveGen(map, new IntVec3((int)Rand.Range(2f,298f), 0, (int)Rand.Range(2f, 298f)), "RimWormhole");

			int index = 1;
			foreach (IntVec3 point in MapToolBag.MapHandlerUtility.RandomPointsInQuads(new IntVec3(20, 0, 20), new IntVec3(240, 0,240)))
			{
				string name = index == 1 ? "RimPlanetHome" : PlanetList.FindAll(s => !usedPlanetList.Contains(s)).RandomElement<string>();
				usedPlanetList.Add(name);
				Building Planet = MapToolBag.MapWorkerUtility.AdaptiveGen(map, point, name) as Building;

				Planet.GetComp<CompPlanet>().GroundMap = null;
				//mapComp.addPlanet(data);
				//mapComp.addPlanet(Planet, index == 1 ? true : false ) ;
				index++;
			}
			map.MapUpdate();
		}


		public string OrbitalNameGenerator()
		{
			string result = "";
			List<string> s = new List<string>() { "do", "ray", "mi", "pha", "su", "la", "ti", "on" };
			result = s.RandomElement() + s.RandomElement() + s.RandomElement();
			return result;
		}
	}




	public class GenStep_PlanetSettlement : GenStep_Scatterer
	{
		private static readonly IntRange SettlementSizeRange = new IntRange(250, 260);
		private static List<IntVec3> tmpCandidates = new List<IntVec3>();


		public override int SeedPart => 148842069;
		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!base.CanScatterAt(c, map))return false;
			if (!c.Standable(map))return false;
			if (c.Roofed(map))return false;
			if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false, false, false)))return false;
			int min = GenStep_PlanetSettlement.SettlementSizeRange.min;
			CellRect cellRect = new CellRect(c.x - min / 2, c.z - min / 2, min, min);
			return cellRect.FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z));
		}
		protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
		{
			int randomInRange = GenStep_PlanetSettlement.SettlementSizeRange.RandomInRange;
			int randomInRange2 = GenStep_PlanetSettlement.SettlementSizeRange.RandomInRange;
			CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
			Faction faction;
			if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
			{
				faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
			}
			else
			{
				faction = map.ParentFaction;
			}
			rect.ClipInsideMap(map);
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = rect;
			resolveParams.faction = faction;
			BaseGen.globalSettings.map = map;
			BaseGen.globalSettings.minBuildings = 1;
			BaseGen.globalSettings.minBarracks = 1;
			BaseGen.symbolStack.Push("settlement", resolveParams, null);
			if (faction != null && faction == Faction.OfEmpire)
			{
				BaseGen.globalSettings.minThroneRooms = 1;
				BaseGen.globalSettings.minLandingPads = 1;
			}
			BaseGen.Generate();
			if (faction != null && faction == Faction.OfEmpire && BaseGen.globalSettings.landingPadsGenerated == 0)
			{
				CellRect cellRect;
				GenStep_Settlement.GenerateLandingPadNearby(resolveParams.rect, map, faction, out cellRect);
			}
			
		}
		public static void GenerateLandingPadNearby(CellRect rect, Map map, Faction faction, out CellRect usedRect)
		{
			ResolveParams resolveParams = default(ResolveParams);
			List<CellRect> usedRects;
			MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out usedRects);
			GenStep_PlanetSettlement.tmpCandidates.Clear();
			int size = 9;
			GenStep_PlanetSettlement.tmpCandidates.Add(new IntVec3(rect.maxX + 1, 0, rect.CenterCell.z));
			GenStep_PlanetSettlement.tmpCandidates.Add(new IntVec3(rect.minX - size, 0, rect.CenterCell.z));
			GenStep_PlanetSettlement.tmpCandidates.Add(new IntVec3(rect.CenterCell.x, 0, rect.maxZ + 1));
			GenStep_PlanetSettlement.tmpCandidates.Add(new IntVec3(rect.CenterCell.x, 0, rect.minZ - size));
			IntVec3 intVec;
			if (!GenStep_PlanetSettlement.tmpCandidates.Where(delegate (IntVec3 x)
			{
				CellRect r = new CellRect(x.x, x.z, size, size);
				return r.InBounds(map) && (usedRects == null || !usedRects.Any((CellRect y) => y.Overlaps(r)));
			}).TryRandomElement(out intVec))
			{
				usedRect = CellRect.Empty;
				return;
			}
			resolveParams.rect = new CellRect(intVec.x, intVec.z, size, size);
			resolveParams.faction = faction;
			BaseGen.globalSettings.map = map;
			BaseGen.symbolStack.Push("landingPad", resolveParams, null);
			BaseGen.Generate();
			usedRect = resolveParams.rect;
		}
	}
}

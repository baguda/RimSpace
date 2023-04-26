using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

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
			MapToolBag.MapWorkerUtility.AdaptiveGen(map, map.Center, "RimSun");
			/*
						int systemCount = 4;
						for (int num = 2; num <= systemCount + 2; num++)
						{
							string name = num == 2 ? "RimPlanetHome" : PlanetList.FindAll(s => !usedPlanetList.Contains(s)).RandomElement<string>();
							usedPlanetList.Add(name);
							IntVec3 point = MapToolBag.MapHandlerUtility.circArea(map.Center, num * 25, num * 25, true).RandomElement();
							MapToolBag.MapWorkerUtility.AdaptiveGen(map, point, name);
							this.SpaceComp.addPlanet(DefDatabase<ThingDef>.AllDefs.Named(name).label, point, num == 2 ? true : false);
						}

						int index = 1;
						foreach(IntVec3 point in MapToolBag.MapHandlerUtility.RandomPointsInQuads(new IntVec3(157, 0, 157), new IntVec3(140, 0, 140)))
						{
							string name = index == 1 ? "RimPlanetHome" : PlanetList.FindAll(s => !usedPlanetList.Contains(s)).RandomElement<string>();
							//usedPlanetList.Add(name);
							MapToolBag.MapWorkerUtility.AdaptiveGen(map, point, name);
							this.SpaceComp.addPlanet(DefDatabase<ThingDef>.AllDefs.Named(name).label, point, index == 1 ? true : false);
							index++;
						}
						foreach (IntVec3 point in MapToolBag.MapHandlerUtility.RandomPointsInQuads(new IntVec3(157, 0, 02), new IntVec3(140, 0, 140)))
						{
							string name = index == 1 ? "RimPlanetHome" : PlanetList.FindAll(s => !usedPlanetList.Contains(s)).RandomElement<string>();
							//usedPlanetList.Add(name);
							MapToolBag.MapWorkerUtility.AdaptiveGen(map, point, name);
							this.SpaceComp.addPlanet(DefDatabase<ThingDef>.AllDefs.Named(name).label, point, index == 1 ? true : false);
							index++;
						}
						foreach (IntVec3 point in MapToolBag.MapHandlerUtility.RandomPointsInQuads(new IntVec3(2, 0, 02), new IntVec3(140, 0, 140)))
						{
							string name = index == 1 ? "RimPlanetHome" : PlanetList.FindAll(s => !usedPlanetList.Contains(s)).RandomElement<string>();
							//usedPlanetList.Add(name);
							MapToolBag.MapWorkerUtility.AdaptiveGen(map, point, name);
							this.SpaceComp.addPlanet(DefDatabase<ThingDef>.AllDefs.Named(name).label, point, index == 1 ? true : false);
							index++;
						}
						foreach (IntVec3 point in MapToolBag.MapHandlerUtility.RandomPointsInQuads(new IntVec3(2, 0, 150), new IntVec3(140, 0, 140)))
						{
							string name = index == 1 ? "RimPlanetHome" : PlanetList.FindAll(s => !usedPlanetList.Contains(s)).RandomElement<string>();
							//usedPlanetList.Add(name);
							MapToolBag.MapWorkerUtility.AdaptiveGen(map, point, name);
							this.SpaceComp.addPlanet(DefDatabase<ThingDef>.AllDefs.Named(name).label, point, index == 1 ? true : false);
							index++;
						}
						*/
			int index = 1;
			foreach (IntVec3 point in MapToolBag.MapHandlerUtility.RandomPointsInQuads(new IntVec3(20, 0, 20), new IntVec3(240, 0,240)))
			{
				string name = index == 1 ? "RimPlanetHome" : PlanetList.FindAll(s => !usedPlanetList.Contains(s)).RandomElement<string>();
				usedPlanetList.Add(name);
				MapToolBag.MapWorkerUtility.AdaptiveGen(map, point, name);
				this.SpaceComp.addPlanet(DefDatabase<ThingDef>.AllDefs.Named(name).label, point, index == 1 ? true : false);
				index++;
			}
		}


		public string OrbitalNameGenerator()
		{
			string result = "";
			List<string> s = new List<string>() { "do", "ray", "mi", "pha", "su", "la", "ti", "on" };
			result = s.RandomElement() + s.RandomElement() + s.RandomElement();
			return result;
		}
	}

}

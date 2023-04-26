using System.Collections.Generic;
using RimWorld;
using System.Linq;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace MapToolBag
{
	[StaticConstructorOnStartup]
	public static class TileHandlerUtility
	{

		public static void ConditionTile(int tileInd, float elevation=-1, float rainfall = -1, float swampiness = -1, float temperature = -1, BiomeDef biome=null, Hilliness hilliness = Hilliness.Undefined)
        {

			if (elevation >= 0) SetTileElevation(tileInd, elevation);
			if (rainfall >= 0) SetTileRainfall(tileInd, rainfall);
			if (swampiness >= 0) SetTileSwampiness(tileInd, swampiness);
			if (temperature >= 0) SetTileTemperature(tileInd, temperature);
			if (biome != null) SetTileBiome(tileInd, biome);
			if (hilliness != Hilliness.Undefined) SetTileHilliness(tileInd, hilliness);

		}

		public static void SetTileElevation(int Tile,float newValue)
        {
			getTile(Tile).elevation = newValue;
        }
		public static void SetTileRainfall(int Tile, float newValue)
		{
			getTile(Tile).rainfall = newValue;
		}
		public static void SetTileSwampiness(int Tile, float newValue)
		{
			getTile(Tile).swampiness = newValue;
		}
		public static void SetTileTemperature(int Tile, float newValue)
		{
			getTile(Tile).temperature = newValue;
		}
		public static void SetTileBiome(int Tile, BiomeDef newValue)
		{
			getTile(Tile).biome = newValue;
		}
		public static void SetTileHilliness(int Tile, Hilliness newValue)
		{
			getTile(Tile).hilliness = newValue;
		}


		public static int getProxyTile(int TileNum = -1, string biome = null, int stepCount = 0, bool useBiome = false)
		{

			if (TileNum == -1) TileFinder.TryFindRandomPlayerTile(out TileNum, true, null);
			List<int> list = new List<int>();
			int tileBuf = TileNum;
			int count = stepCount;
			bool flag = true;
			while (flag)
			{
				if (count < 0)
				{
					list = scanForValidNeighbors(TileNum, tileBuf, biome, useBiome);
				}
				else list.Clear();
				if (list.Count != 0)
				{
					flag = false;
				}
				else
				{
					Find.World.grid.GetTileNeighbors(TileNum, list);
					TileNum = list.RandomElement<int>();
				}
				count--;
			}
			return list.RandomElement<int>();

		}

		public static int getProxyTile2(int TileNum = -1, string targetBiomeName = null, int Distance = 1)
		{
			if (TileNum == -1) TileFinder.TryFindRandomPlayerTile(out TileNum, true, null);
			float scan = 0.10f;
			while (scan <= 1.0f)
			{

				var list = getValidTileNumsAtDistance(TileNum, Distance, scan, 1, targetBiomeName);
				if (list.Count() == 0)
				{
					scan += 0.01f;
					//Prim.Log(scan.ToString());
				}
				else
				{
					return list.RandomElement<int>();

				}

			}

			return -1;


			//Find.World.grid.tiles.FindAll(s => Find.World.grid.tiles.IndexOf)
			//foreach (Tile tile in Find.World.grid.tiles)
			//{
			//	var d = Find.World.grid.tiles.IndexOf(tile);
			//}
		}

		public static List<int> getValidTileNumsAtDistanceX(int TileNum, int Distance, float tolerance = 0.1f, int magnitude = 1, string targetBiomeName = null)
		{
			List<int> valids = new List<int>();
			var r = Find.World.grid.LongLatOf(TileNum);
			for (int tile = 1; tile <= Find.World.grid.TilesCount; tile++)
			{
				var tt = Find.World.grid.LongLatOf(tile);
				//

				var dis = MapHandlerUtility.getDistance(new IntVec3((int)(r.x * magnitude), 0, (int)(r.x * magnitude)), new IntVec3((int)(tt.x * magnitude), 0, (int)(tt.y * magnitude)));
				if (dis < Distance * (1f + tolerance) && dis >= Distance * (1f - tolerance))
				{
					valids.Add(tile);
				}
			}
			if (valids.Count != 0)
			{
				foreach (int num in valids)
				{
					if (getTile(num).hilliness.Equals(Hilliness.Impassable))
					{

						valids.Remove(num);

					}
					else if (getTile(num).hilliness.Equals(Hilliness.Mountainous))
					{

						valids.Remove(num);

					}
					else if (getTile(num).elevation > 0)
					{
						valids.Remove(num);

					}
					else if (getTile(num).biome.defName.Equals("Ocean"))
					{
						valids.Remove(num);

					}
					else if (!targetBiomeName.Equals(null) && !getTile(num).biome.defName.Equals(targetBiomeName))
					{
						valids.Remove(num);
					}
				}

			}

			return valids;
		}
		public static List<int> getValidTileNumsAtDistance(int TileNum, int Distance, float tolerance = 0.1f, int magnitude = 1, string targetBiomeName = null)
		{
			List<int> valids = new List<int>();
			Vector3 r = Find.World.grid.GetTileCenter(TileNum);
			for (int tile = 1; tile <= Find.World.grid.TilesCount; tile++)
			{
				var tt = Find.World.grid.GetTileCenter(tile);
				var dis = MapHandlerUtility.getDistance(new IntVec3((int)(r.x * magnitude), (int)(r.y * magnitude), (int)(r.z * magnitude)), new IntVec3((int)(tt.x * magnitude), (int)(tt.y * magnitude), (int)(tt.z * magnitude)));
				if (dis < Distance * (1f + tolerance) && dis >= Distance * (1f - tolerance))
				{
					valids.Add(tile);
					Log.ResetMessageCount();
				}
			}
			if (valids.Count != 0)
			{
				foreach (int num in valids)
				{
					if (getTile(num).hilliness.Equals(Hilliness.Impassable))
					{

						valids.Remove(num);

					}
					else if (getTile(num).hilliness.Equals(Hilliness.Mountainous))
					{

						valids.Remove(num);

					}
					else if (getTile(num).elevation > 0)
					{
						valids.Remove(num);

					}
					else if (getTile(num).biome.defName.Equals("Ocean"))
					{
						valids.Remove(num);

					}
					else if (!targetBiomeName.Equals(null) && !getTile(num).biome.defName.Equals(targetBiomeName))
					{
						valids.Remove(num);
					}
				}

			}

			return valids;
		}
		public static List<int> scanForValidNeighbors(int tile, int tileBuf, string biome, bool useBiome)
		{
			//Log.Message("startScan", false);

			List<int> list = new List<int>();

			Find.World.grid.GetTileNeighbors(tile, list);

			list.Remove(tileBuf);
			List<int> source = list;

			//Prim.Log(biome);
			foreach (int num in source.ToList<int>())
			{
				//Log.Message("Checking Tile: " + num.ToString(), false);

				//Prim.Log(getTile(num).hilliness.ToString() + " : " + getTile(num).biome.defName + " ; "+getTile(num).elevation.ToString());
				if (getTile(num).hilliness.Equals(Hilliness.Impassable))
				{

					list.Remove(num);// Prim.Log("Impassable");

				}
				else if (getTile(num).hilliness.Equals(Hilliness.Mountainous))
				{

					list.Remove(num);// Prim.Log("Mountainous");

				}
				else if (getTile(num).elevation < 0)
				{
					list.Remove(num);// Prim.Log("elevation");

				}
				else if (useBiome && !getTile(num).biome.defName.Equals(biome))
				{
					list.Remove(num);// Prim.Log("biome");
				}
				Log.ResetMessageCount();
			}

			return list;
		}

		public static Tile getTile(int tileNum)
		{
			return Find.World.grid.tiles.ElementAt(tileNum);
		}


	}

}

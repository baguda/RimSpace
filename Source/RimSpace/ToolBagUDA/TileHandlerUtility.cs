using System.Collections.Generic;
using RimWorld;
using System.Linq;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using System;

namespace MapToolBag
{
	[StaticConstructorOnStartup]
	public static class TileHandlerUtility
	{

		public static void ConditionTile(int tileInd, string biomeDefName = null, float elevation=-1, float rainfall = -1, float swampiness = -1, float temperature = -1,  Hilliness hilliness = Hilliness.Undefined)
        {

			SetTileElevation(tileInd, elevation);
			SetTileRainfall(tileInd, rainfall);
			SetTileSwampiness(tileInd, swampiness);
			SetTileTemperature(tileInd, temperature);
			if (biomeDefName != null) SetTileBiome(tileInd, DefDatabase<BiomeDef>.GetNamed(biomeDefName));
			SetTileHilliness(tileInd, hilliness);

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
			List<int> list = new List<int>();

			Find.World.grid.GetTileNeighbors(tile, list);

			list.Remove(tileBuf);
			List<int> source = list;
			foreach (int num in source.ToList<int>())
			{
				if (getTile(num).hilliness.Equals(Hilliness.Impassable))
				{
					list.Remove(num);
				}
				else if (getTile(num).hilliness.Equals(Hilliness.Mountainous))
				{
					list.Remove(num);
				}
				else if (getTile(num).elevation < 0)
				{
					list.Remove(num);
				}
				else if (useBiome && !getTile(num).biome.defName.Equals(biome))
				{
					list.Remove(num);
				}
				Log.ResetMessageCount();
			}
			return list;

			HashSet<int> R0 = new HashSet<int>() { tile };
			HashSet<int> R1 = GetOuterNeighbors(R0);
			HashSet<int> R2 = GetOuterNeighbors(R1);
			HashSet<int> R3 = GetOuterNeighbors(R2);
			HashSet<int> R4 = GetOuterNeighbors(R3);

		}
		public static List<HashSet<int>> concentricNeighbors(int tile, int iterations)
        {
			List<HashSet<int>> result = new List<HashSet<int>>() { new HashSet<int>() { tile } };
			for(int ind = 1; ind <= iterations; ind++)
            {
				HashSet<int> set = new HashSet<int>();
				result[ind - 1].UnionWith(result[Math.Max(ind, 0)]);
				result.Add(GetOuterNeighbors(result[ind - 1]));
            }
			return null;
		}
		public static HashSet<int> GetOuterNeighbors(HashSet<int> S1 )
        {
			HashSet<int> S2 = new HashSet<int>();
			foreach (int tileN in S1)
			{
				List<int> A1 = new List<int>();
				Find.World.grid.GetTileNeighbors(tileN, A1);
				S2.AddRange(A1);
			}
			S2.ExceptWith(S1);
			return S2;
		}

		public static Tile getTile(int tileNum)
		{
			return Find.World.grid.tiles.ElementAt(tileNum);
		}

		public static List<int> applyNeighbors(int tileNum,  Action<Tile> applyToEachNeighborTile = null)
		{
				List<int> neighbors = new List<int>();
				Find.World.grid.GetTileNeighbors(tileNum, neighbors);
				foreach (int tile in neighbors)
				{
					if (applyToEachNeighborTile != null) applyToEachNeighborTile.Invoke(TileHandlerUtility.getTile(tile));
				}
			return neighbors;
			/*
			Action<Tile> aa = delegate (Tile T1)
			{
				T1.elevation = 0.0f;
			};
			*/

			

		}
	}

}

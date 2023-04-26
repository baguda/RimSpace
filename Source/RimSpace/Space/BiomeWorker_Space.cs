using RimWorld;
using RimWorld.Planet;

namespace RimSpace
{
	public class BiomeWorker_Space : BiomeWorker
	{
		public override float GetScore(Tile tile, int tileID)
		{
			return -999f;
		}
	}
}

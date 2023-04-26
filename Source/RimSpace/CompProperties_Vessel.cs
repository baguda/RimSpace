using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimSpace
{
    public class CompProperties_Vessel : CompProperties_Transporter
    {

		public CompProperties_Vessel()
		{
			this.compClass = typeof(CompVessel);
		}

		public int ReadyTicks = 300;
		public int getReadyTime => ReadyTicks;

		public string EjectLabel = "Elect";
		public string getEjectLabel => EjectLabel;

		public string EjectDesc = "Eject Crew";
		public string getEjectDesc => EjectDesc;
	}


}

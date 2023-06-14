using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimSpace
{
    public class CompProperties_Spaceship : CompProperties_Vessel
    {

        public CompProperties_Spaceship()
        {
            this.compClass = typeof(CompSpaceship);
        }


		public int CrewSize = 3;
		public int getCrewSize => CrewSize;


		public bool MedicalTendingEnabled = false;
		public bool getMedicalTendingEnabled => MedicalTendingEnabled;


	}
}

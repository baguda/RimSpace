using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Diagnostics;

namespace RimSpace
{
    public class RimSpace : Mod
    {
        public RimSpace(ModContentPack modContent) : base(modContent) { }

        public const UInt16 ModVersion = 1;
        public void DebugLog(Type T , string input)
        {
            if (Prefs.DevMode)
            {
               // Log.Message((T.Name new StackTrace().GetFrame(1).GetMethod().Name))
            }
        }
        public string getMethodName => (new StackTrace().GetFrame(1).GetMethod().Name);
        public string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }
    }
}


/*
 
 Rim Space Road Map (RSRM)

initial release v1.0:
    
    Overview: 
                Game will play as normal, player will be able to build spaceship building anywhere on map and load it up with medicine, food, and ChemFuel
                Ship will require 200 fuel and a colonist with a mechlink to oversee it in order to launch.
                when launched, the ship will dissapear from map and appear on world map where it will proceed to an orbital point 
                once it reaches the orbital point, the world object will despawn and a space map will be generated. 
                The 
                
 
 */
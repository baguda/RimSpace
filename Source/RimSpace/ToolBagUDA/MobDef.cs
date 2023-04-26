
using System.Collections.Generic;
using Verse;

namespace MobileObjects
{
    public class MobDef : ThingDef
    {
        List<Mob_Path> Pathing = new List<Mob_Path>();
        public Mob_AggroProps Aggro = new Mob_AggroProps();
        public float MoveSpeed = 0.5f;

        public new bool CanHaveFaction => true;
    }
}

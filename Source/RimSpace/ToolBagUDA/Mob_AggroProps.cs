
namespace MobileObjects
{
    public class Mob_AggroProps
    {
        public Mob_AggroProps() { }

        public bool AggroPlayerOnDamage = true;
        public bool AggroPlayerOnSight = true;
        public bool AggroNonPlayerOnDamage = false;
        public bool AggroNonPlayerOnSight = false;
        public bool UseLeash = true;


        public float LeashRadius = 30f;
        public float AggroRadius = 30f;
        public float AggroPerDamage = 1;
        public float AggroOnSight = 10;

        public int moveDuration = 120;
        public bool MoveToTarget = true;
        public NonAggroBehavior NoAggroBehavior = NonAggroBehavior.Post;

        public AggroBehavior AggroBehavior = AggroBehavior.Charge;
        public int wanderInterval = 60 * 5;


    }

    public enum NonAggroBehavior
    {
        Post,
        Idle,
        Wander,
        Patrol
    }
    public enum AggroBehavior
    {
        Charge,
        Range,
        Strafe,
        Hide
    }
}

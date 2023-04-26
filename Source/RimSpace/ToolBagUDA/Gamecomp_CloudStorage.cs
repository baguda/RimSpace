using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using UnityEngine;


namespace MapToolBag
{
    public class Gamecomp_CloudStorage : GameComponent
    {
        

        public Dictionary<string, bool> BoolCloud = new Dictionary<string, bool>();
        public Dictionary<string, int> IntCloud = new Dictionary<string, int>();
        public Dictionary<string, float> FloatCloud = new Dictionary<string, float>();
        public Dictionary<string, string> StringCloud = new Dictionary<string, string>();
        public Dictionary<string, IntVec3> IntVec3Cloud = new Dictionary<string, IntVec3>();
        public Dictionary<string, Vector3> Vector3Cloud = new Dictionary<string, Vector3>();
        public Dictionary<string, Color> ColorCloud = new Dictionary<string, Color>();

        public Dictionary<string, LocalTargetInfo> LocalTargetInfoCloud = new Dictionary<string, LocalTargetInfo>();
        public Dictionary<string, List<LocalTargetInfo>> LocalTargetInfoListCloud = new Dictionary<string, List<LocalTargetInfo>>();

        public Dictionary<string, TargetInfo> TargetInfoCloud = new Dictionary<string, TargetInfo>();
        public Dictionary<string, List<TargetInfo>> TargetInfoListCloud = new Dictionary<string, List<TargetInfo>>();

        public Dictionary<string, GlobalTargetInfo> GlobalTargetInfoInfoCloud = new Dictionary<string, GlobalTargetInfo>();
        public Dictionary<string, List<GlobalTargetInfo>> GlobalTargetInfoInfoListCloud = new Dictionary<string, List<GlobalTargetInfo>>();


        public Dictionary<string, List<bool>> BoolListCloud = new Dictionary<string, List<bool>>();
        public Dictionary<string, List<int>> IntListCloud = new Dictionary<string, List<int>>();
        public Dictionary<string, List<float>> FloatListCloud = new Dictionary<string, List<float>>();
        public Dictionary<string, List<string>> StringListCloud = new Dictionary<string, List<string>>();
        public Dictionary<string, List<IntVec3>> IntVec3ListCloud = new Dictionary<string, List<IntVec3>>();
        public Dictionary<string, List<Vector3>> Vector3ListCloud = new Dictionary<string, List<Vector3>>();
        public Dictionary<string, List<Color>> ColorListCloud = new Dictionary<string, List<Color>>();

        /*Unsaved "RAM"*/
        public Dictionary<string, Thing> ThingCloud = new Dictionary<string, Thing>();
        public Dictionary<string, ThingWithComps> ThingWithCompsCloud = new Dictionary<string, ThingWithComps>();
        public Dictionary<string, Pawn> PawnCloud = new Dictionary<string, Pawn>();
        public Dictionary<string, List<Thing>> ThingListCloud = new Dictionary<string, List<Thing>>();
        public Dictionary<string, List<ThingWithComps>> ThingWithCompsListCloud = new Dictionary<string, List<ThingWithComps>>();
        public Dictionary<string, List<Pawn>> PawnListCloud = new Dictionary<string, List<Pawn>>();

        private static Dictionary<string, object> data = new Dictionary<string, object>();


        public static T GetVar<T>(string name)
        {
            object obj;
            if (Gamecomp_CloudStorage.data.TryGetValue(name, out obj))
            {
                return (T)((object)obj);
            }
            return default(T);
        }

        public static bool TryGetVar<T>(string name, out T var)
        {
            object obj;
            if (Gamecomp_CloudStorage.data.TryGetValue(name, out obj))
            {
                var = (T)((object)obj);
                return true;
            }
            var = default(T);
            return false;
        }

        public static void SetVar<T>(string name, T var)
        {
            Gamecomp_CloudStorage.data[name] = var;
        }


        public override void GameComponentOnGUI()
        {
            /*
            if (Prefs.DevMode)
            {
                GizmoGridDrawer.DrawGizmoGrid(panel(), -1000, out Gizmo ds);

                Vector2 vector = new Vector2((float)UI.screenWidth * 0.5f - 24f, 3f);
                Find.WindowStack.ImmediateWindow(typeof(DebugWindowsOpener).GetHashCode(), new Rect(vector.x, vector.y, 24f, 24f).Rounded(), WindowLayer.GameUI, delegate
                {
                    if (new WidgetRow(24f, 0f, UIDirection.LeftThenDown, 99999f, 4f).ButtonIcon(ContentFinder<Texture2D>.Get("Adventure/searchIcon", true), "Restart Rimworld", null, true))
                    {
                        GenCommandLine.Restart();
                    }
                }, false, false, 0f);
            }*/
            base.GameComponentOnGUI();
        }

        public Gamecomp_CloudStorage(Game game) : base()
        {
        }
        public override void StartedNewGame()
        {
        }
        public override void GameComponentTick()
        {
            base.GameComponentTick();


        }
        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }
        public override void LoadedGame()
        {
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref BoolCloud, "BoolCloud", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref IntCloud, "IntCloud", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref FloatCloud, "FloatCloud", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref StringCloud, "StringCloud", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref IntVec3Cloud, "IntVec3Cloud", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref Vector3Cloud, "Vector3Cloud", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref ColorCloud, "ColorCloud", LookMode.Value, LookMode.Value);

            Scribe_Collections.Look(ref BoolListCloud, "BoolCloud", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref IntListCloud, "IntCloud", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref FloatListCloud, "FloatCloud", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref StringListCloud, "StringCloud", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref IntVec3ListCloud, "IntVec3Cloud", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref Vector3ListCloud, "Vector3Cloud", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref ColorListCloud, "ColorCloud", LookMode.Value, LookMode.Value);




        }


    }

   
}

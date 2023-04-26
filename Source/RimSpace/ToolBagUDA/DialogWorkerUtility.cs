using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

using RimWorld;
namespace MapToolBag
{

    [StaticConstructorOnStartup]
    public class DialogWorkerUtility
    {
        /*
        public DialogWorkerUtility() { }

        private Dictionary<string, Tuple<Rect, Vector2, string>> dataBank = new Dictionary<string, Tuple<Rect, Vector2, string>>();
        private Tuple<Rect, Vector2, string> tmpTpl;
        public static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);
        private Dictionary<string, string> stringBank = new Dictionary<string, string>();
        private string tmpstr;
        private Enum tmpObj= null;


        public List<string> ListInput(Listing_Standard listing_Standard, ref List<string> input, string Label, string toolTip = null)
        {
            if (!this.dataBank.ContainsKey(Label))
            {
                this.dataBank.Add(Label, new Tuple<Rect, Vector2, string>(new Rect(), new Vector2(), ""));
            }

            this.dataBank.TryGetValue(Label, out tmpTpl);
            Vector2 scrollPos = tmpTpl.Item2;
            Rect rect = tmpTpl.Item1;
            string buffer = tmpTpl.Item3;

            listing_Standard.Label(Label, -1f, toolTip);

            var listing_Standard2 = listing_Standard.BeginSection_NewTemp(96f);
            var r2 = listing_Standard2.GetRect(96f);
            rect.width = r2.width - 0f;
            listing_Standard2.BeginScrollView(r2, ref scrollPos, ref rect);

            input.RemoveAll((n) => n == null);
            for (int j = 0; j < input.Count(); j++)
            {
                string name = input[j];
                Rect rect2 = listing_Standard2.GetRect(24f);
                Widgets.Label(rect2, name);
                if (Widgets.ButtonImage(new Rect(rect2.xMax - 24f, rect2.y, 24f, 24f), DialogWorkerUtility.DeleteX, Color.white, GenUI.SubtleMouseoverColor, true))
                {
                    input.RemoveAt(j);
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                }
            }
            listing_Standard2.EndScrollView(ref rect);

            listing_Standard.EndSection(listing_Standard2);


            buffer = listing_Standard.TextEntry(buffer);
            if (!input.Contains(buffer) && listing_Standard.ButtonText("AddName".Translate() + "...", "Add name in text bow to list."))
            {
                input.Add(buffer);
            }

            this.dataBank.SetOrAdd(Label, new Tuple<Rect, Vector2, string>(rect, scrollPos, buffer));

            return input;
        }


        public string DropButtonDefs<T>(Listing_Standard listing_Standard, string Label, ref string curDefName) where T : Def
        {
            if (!stringBank.ContainsKey(Label))
            {
                stringBank.SetOrAdd(Label, curDefName);
            }

            if (listing_Standard.ButtonTextLabeled(Label, curDefName))
            { 
                
                List<FloatMenuOption> list3 = new List<FloatMenuOption>();
                foreach (var obj in DefDatabase<T>.AllDefs)
                {

                    list3.Add(new FloatMenuOption(obj.defName, delegate ()
                    {
                        stringBank.SetOrAdd(Label ,obj.defName); Log.Message("DialogWorkerUtility.DropButtonDefs:4 " + obj.defName);
                        
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list3));
            }
            if (curDefName != getValue(stringBank,Label))
            {
                curDefName = getValue(stringBank, Label);
            }
            return curDefName ;
        }

        public string getValue(Dictionary<string,string> data, string key)
        {
            string result;
            data.TryGetValue(key, out result);
            return result;
        }

        public T DropButtonEnum<T>(Listing_Standard listing_Standard,string label,ref T input) where T:Enum
        {
            
            if (listing_Standard.ButtonTextLabeled(label, input.ToString()))
            {
                List<FloatMenuOption> list3 = new List<FloatMenuOption>();
                foreach (var obj in Enum.GetValues(typeof(T)))
                {
                    T localMode2 = (T)obj;
                    T localMode = localMode2;

                        list3.Add(new FloatMenuOption(localMode.ToString(), delegate ()
                        {
                            tmpObj = localMode;
                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    
                }
                Find.WindowStack.Add(new FloatMenu(list3));
            }
            
            if (input!=null&& tmpObj != null && !input.Equals((T)tmpObj))
            {
                input = (T)tmpObj;
            }
            
            return input;
        }


        */
    }
}

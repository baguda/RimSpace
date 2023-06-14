using System.Collections.Generic;

using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace RimSpace
{
    public static class PanelWorkerUtility
    {
        public static void makeGraph(string Label, Rect space, float yMinPercent, float heightPercent, float fillPercent, Texture2D FullBarTex, Texture2D EmptyBarTex)
        {
            Rect graphWindow = space;
            graphWindow.yMin = space.y + space.height * yMinPercent;
            graphWindow.height = space.height * heightPercent;
            Widgets.FillableBar(graphWindow, fillPercent, FullBarTex, EmptyBarTex, false);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(graphWindow, (fillPercent * 100f).ToString("F0") + " / " + 100f.ToString("F0") + " " + Label);
            Text.Anchor = TextAnchor.UpperLeft;
        }
        public static void makeGraph(string Label, Rect space, float yMinPercent, float heightPercent, float CurAmount, float MaxAmount, Texture2D FullBarTex, Texture2D EmptyBarTex)
        {
            Rect graphWindow = space;
            graphWindow.yMin = space.y + space.height * yMinPercent;
            graphWindow.height = space.height * heightPercent;
            Widgets.FillableBar(graphWindow, CurAmount / MaxAmount, FullBarTex, EmptyBarTex, false);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(graphWindow, (CurAmount * 100f).ToString("F0") + " / " + (MaxAmount * 100f).ToString("F0") + " " + Label);
            Text.Anchor = TextAnchor.UpperLeft;
        }
        public static void makeNestedGraphs(string Label, Rect space, float yMinPercent, float heightPercent, float fillPercent1, float fillPercent2, Texture2D FullBarTex1, Texture2D FullBarTex2, Texture2D EmptyBarTex)
        {
            Rect graphWindow = space;
            graphWindow.yMin = space.y + space.height * yMinPercent;
            graphWindow.height = space.height * heightPercent;

            Rect subgraphWindow1 = graphWindow;
            subgraphWindow1.yMin = graphWindow.y + graphWindow.height * 0.25f;
            subgraphWindow1.height = graphWindow.height * 0.5f;

            Rect subgraphWindow2 = graphWindow;
            subgraphWindow2.yMin = graphWindow.y;
            subgraphWindow2.height = graphWindow.height * 1f;
            Widgets.FillableBar(subgraphWindow2, fillPercent2, FullBarTex2, EmptyBarTex, false);
            Widgets.FillableBar(subgraphWindow1, fillPercent1, FullBarTex1, EmptyBarTex, false);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(graphWindow, (fillPercent1 * 100f).ToString("F0") + "%|" + (fillPercent2 * 100f).ToString("F0") + "% " + Label);
            Text.Anchor = TextAnchor.UpperLeft;
        }


        public static GizmoResult GizmoOnGUI(Vector2 topLeft, GizmoRenderParms parms, int queueZ = 0, int queueX = 0, float GizmoWidth = 160f, float GizmoHeight = 75f)
        {
         

            topLeft = new Vector2(75f, 75f) + new Vector2(queueX * GizmoWidth, queueZ * GizmoHeight);

            //setup main window
            Rect mainWindow = new Rect(topLeft.x, topLeft.y, GizmoWidth, 75f);
            Widgets.DrawWindowBackground(mainWindow);
            Rect innerWindow = mainWindow.ContractedBy(6f);
           // makePanel(innerWindow);



            return new GizmoResult(GizmoState.Clear);

        }
        
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BlueprintsX
{
    public static class BPXManager
    {
        public static LEV_LevelEditorCentral central;

        //UI Colors
        public static Color blue = new Color(0, 0.547f, 0.82f, 1f);
        public static Color darkerBlue = new Color(0, 0.371f, 0.547f, 1f);

        //Paths
        public static string levelHomeDirectory;
        public static string blueprintHomeDirectory;

        //Blueprints
        public static Blueprint createdBlueprintFromEditor;
        public static BPXRenderCompleteArgs createdBlueprintRender;
        public static string targetedSavePath;

        public static Blueprint selectedBlueprintDuringLoading;
        public static BPXRenderCompleteArgs loadedBlueprintRender;

        public static Blueprint copyBuffer;

        public enum FocusState
        {
            Focused, FocusLost
        }

        public static FocusState currentFocusState = FocusState.Focused;

        public static void LevelEditorAwake(LEV_LevelEditorCentral instance)
        {
            central = instance;

            levelHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Zeepkist\\Levels";
            blueprintHomeDirectory = AppDomain.CurrentDomain.BaseDirectory + @"\BepInEx\plugins";

            BPXUI.Initialize();
            BPXConfig.ApplyGridLists();

            BPXRenderer.OnRenderComplete += BPXRenderer_OnRenderComplete;
        }

        private static void BPXRenderer_OnRenderComplete(object sender, BPXRenderCompleteArgs args)
        {
            switch (args.renderTag)
            {
                case "Load":
                    loadedBlueprintRender = args;                    
                    break;
                case "Save":
                    createdBlueprintRender = args;
                    break;
            }   

            BPXUI.ThumbnailGenerated();
        }

        public static void Run()
        {
            if(central == null) { return; }

            BPXInput.GetInputs();
            BPXGizmoManager.UpdateGizmo();
        }

        public static void Log(string msg)
        {
            Debug.LogWarning(msg);
        }

        public static void MainApplicationFocus()
        {
            if (currentFocusState != FocusState.Focused)
            {
                currentFocusState = FocusState.Focused;
            }
        }

        public static void MainApplicationFocusLost()
        {
            if (currentFocusState != FocusState.FocusLost)
            {
                currentFocusState = FocusState.FocusLost;
                BPXDrag.LostFocus();
            }
        }

        public static void MainGUICall()
        {
            if (central != null)
            {
                if (central.tool.currentTool == 0 && !central.gizmos.isDragging && !central.gizmos.isGrabbing)
                {
                    BPXDrag.DoGUI();
                }
            }
        }
    }    
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using UnityEngine;

namespace BlueprintsX
{
    public static class BPXConfig
    {
        public static ConfigFile config;

        //Preferences
        public static bool invertScrollWheel = false;
        public static bool includePlanesInCycle = false;
        public static bool doubleLoadButtons = false;
        public static bool clearSearchOnClose = false;

        //Controls
        public static bool scrollScaling = true;
        public static bool enableForScrollScale = false;
        public static bool keyScaling = true;
        public static bool enableForKeyScale = false;
        public static bool keyMove = true;
        public static bool enableForKeyMove = false;
        public static bool mmbDrag = true;
        public static bool enableForMMBDrag = false;
        public static bool keyDrag = true;
        public static bool enableForKeyDrag = false;
        public static bool keyMirror = true;
        public static bool enableForKeyMirror = false;
        public static bool clipboard = true;
        public static bool enableForClipboard = false;
        public static bool fastTravel = true;
        public static bool enableForFastTravel = false;

        //Keys
        public static KeyCode enableKey = KeyCode.LeftControl;
        public static KeyCode modifierKey = KeyCode.LeftShift;

        public static KeyCode positiveScaleKey = KeyCode.Equals;
        public static KeyCode negativeScaleKey = KeyCode.Minus;

        public static KeyCode moveLeftKey = KeyCode.LeftArrow;
        public static KeyCode moveRightKey = KeyCode.RightArrow;
        public static KeyCode moveUpKey = KeyCode.UpArrow;
        public static KeyCode moveDownKey = KeyCode.DownArrow;

        public static KeyCode dragKey = KeyCode.LeftAlt;        
        public static KeyCode mirrorKey = KeyCode.F;

        public static KeyCode cycleScaleKey = KeyCode.V;
        public static KeyCode cycleAxisKey = KeyCode.Space;
        
        public static KeyCode copyKey = KeyCode.K;
        public static KeyCode pasteKey = KeyCode.L;

        public static KeyCode fastTravelKey = KeyCode.U;

        //Values
        public static List<float> gridXZList = new List<float>();
        public static List<float> gridYList = new List<float>();
        public static List<float> gridRList = new List<float>();
        public static List<float> gridSList = new List<float>();

        //These lists are the default values of the grid selection lists.
        private static string defaultXZList = "0;0.2;0.8;1.6;4;8;16";
        private static string defaultYList = "0;0.2;0.8;1.6;4;8";
        private static string defaultRList = "0;1;5;10;30;45;90";
        private static string defaultSList = "0.05;0.5;1;2;10;20;50";

        //Default indices
        private static int gridXZListIndex = 6;
        private static int gridYListIndex = 5;
        private static int gridRListIndex = 5;
        private static int gridSListIndex = 5;

        //Titles
        public static string preferencesTitle = "1. Preferences";
        public static string controlSettingsTitle = "2. Control Settings";
        public static string keysTitle = "3. Keys";
        public static string valuesTitle = "4. Values";

        public static int configLabelLength = 38;

        //Setting names
        public static string preferences_invertScrollwheel = CreateConfigLabel(1, "Invert Scrollwheel");
        public static string preferences_includePlanesInCycle = CreateConfigLabel(2, "Include Planes");
        public static string preferences_doubleLoadButtons = CreateConfigLabel(3, "Double Load Button");
        public static string preferences_clearSearchOnClose = CreateConfigLabel(4, "Clear Search On Close");

        public static string control_scrollScaling = CreateConfigLabel(1, "Scroll Scaling");
        public static string control_enableForScrollScaling = CreateConfigLabel(2, "Enable For Scroll Scaling");
        public static string control_keyScaling = CreateConfigLabel(3, "Key Scaling");
        public static string control_enableForKeyScaling = CreateConfigLabel(4, "Enable For Key Scaling");
        public static string control_keyMove = CreateConfigLabel(5, "Key Move");
        public static string control_enableForKeyMove = CreateConfigLabel(6, "Enable For Key Move");
        public static string control_mmbDrag = CreateConfigLabel(7, "MMB Drag");
        public static string control_enableForMMBDrag = CreateConfigLabel(8, "Enable For MMB Drag");
        public static string control_keyDrag = CreateConfigLabel(9, "Key Drag");
        public static string control_enableForKeyDrag = CreateConfigLabel(10, "Enable For Key Drag");
        public static string control_keyMirror = CreateConfigLabel(11, "Key Mirror");
        public static string control_enableForKeyMirror = CreateConfigLabel(12, "Enable For Key Mirror");
        public static string control_clipboard = CreateConfigLabel(13, "Clipboard");
        public static string control_enableForClipboard = CreateConfigLabel(14, "Enable For Clipboard");
        public static string control_fastTravel = CreateConfigLabel(15, "Fast Travel");
        public static string control_enableForFastTravel = CreateConfigLabel(16, "Enable For Fast Travel");

        public static string keys_enable = CreateConfigLabel(1, "Enable");
        public static string keys_modifier = CreateConfigLabel(2, "Modifier");
        public static string keys_positiveScale = CreateConfigLabel(3, "Scale Up");
        public static string keys_negativeScale = CreateConfigLabel(4, "Scale Down");
        public static string keys_moveLeft = CreateConfigLabel(5, "Move Left");
        public static string keys_moveRight = CreateConfigLabel(6, "Move Right");
        public static string keys_moveUp = CreateConfigLabel(7, "Move Forward/Up");
        public static string keys_moveDown = CreateConfigLabel(8, "Move Back/Down");
        public static string keys_drag = CreateConfigLabel(9, "Drag");
        public static string keys_mirror = CreateConfigLabel(10, "Mirror");
        public static string keys_copy = CreateConfigLabel(11, "Copy");
        public static string keys_paste = CreateConfigLabel(12, "Paste");
        public static string keys_cycleAxis = CreateConfigLabel(13, "Cycle Axis");
        public static string keys_cycleScaleGizmo = CreateConfigLabel(14, "Cycle Scale Gizmo");
        public static string keys_fastTravel = CreateConfigLabel(15, "Fast Travel");

        public static string values_XZ = CreateConfigLabel(1, "XZ Values");
        public static string values_XZ_default_index = CreateConfigLabel(2, "Default XZ Index");
        public static string values_Y = CreateConfigLabel(3, "Y Values");
        public static string values_Y_default_index = CreateConfigLabel(4, "Default Y Index");
        public static string values_R = CreateConfigLabel(5, "R Values");
        public static string values_R_default_index = CreateConfigLabel(6, "Default R Index");
        public static string values_S = CreateConfigLabel(7, "S Values");
        public static string values_S_default_index = CreateConfigLabel(8, "Default S Index");

        

        public static void InitializeConfig(ConfigFile cfg)
        {
            config = cfg;

            //Preferences
            ConfigEntry<bool> cfg_invertScrollWheel = config.Bind(preferencesTitle, preferences_invertScrollwheel, invertScrollWheel, "");
            ConfigEntry<bool> cfg_includePlanesInCycle = config.Bind(preferencesTitle, preferences_includePlanesInCycle, includePlanesInCycle, "");
            ConfigEntry<bool> cfg_doubleLoadButtons = config.Bind(preferencesTitle, preferences_doubleLoadButtons, doubleLoadButtons, "");

            //Control Settings
            ConfigEntry<bool> cfg_scrollScaling = config.Bind(controlSettingsTitle, control_scrollScaling, scrollScaling, "");
            ConfigEntry<bool> cfg_enableForScrollScaling = config.Bind(controlSettingsTitle, control_enableForScrollScaling, enableForScrollScale, "");
            ConfigEntry<bool> cfg_keyScaling = config.Bind(controlSettingsTitle, control_keyScaling, keyScaling, "");
            ConfigEntry<bool> cfg_enableForKeyScaling = config.Bind(controlSettingsTitle, control_enableForKeyScaling, enableForKeyScale, "");
            ConfigEntry<bool> cfg_keyMove = config.Bind(controlSettingsTitle, control_keyMove, keyMove, "");
            ConfigEntry<bool> cfg_enableForKeyMove = config.Bind(controlSettingsTitle, control_enableForKeyMove, enableForKeyMove, "");
            ConfigEntry<bool> cfg_mmbDrag = config.Bind(controlSettingsTitle, control_mmbDrag, mmbDrag, "");
            ConfigEntry<bool> cfg_enableForMMBDrag = config.Bind(controlSettingsTitle, control_enableForMMBDrag, enableForMMBDrag, "");
            ConfigEntry<bool> cfg_keyDrag = config.Bind(controlSettingsTitle, control_keyDrag, keyDrag, "");
            ConfigEntry<bool> cfg_enableForKeyDrag = config.Bind(controlSettingsTitle, control_enableForKeyDrag, enableForKeyDrag, "");
            ConfigEntry<bool> cfg_keyMirror = config.Bind(controlSettingsTitle, control_keyMirror, keyMirror, "");
            ConfigEntry<bool> cfg_enableForKeyMirror = config.Bind(controlSettingsTitle, control_enableForKeyMirror, enableForKeyMirror, "");
            ConfigEntry<bool> cfg_clipboard = config.Bind(controlSettingsTitle, control_clipboard, clipboard, "");
            ConfigEntry<bool> cfg_enableForClipboard = config.Bind(controlSettingsTitle, control_enableForClipboard, enableForClipboard, "");
            ConfigEntry<bool> cfg_fastTravel = config.Bind(controlSettingsTitle, control_fastTravel, fastTravel, "");
            ConfigEntry<bool> cfg_enableForFastTravel = config.Bind(controlSettingsTitle, control_enableForFastTravel, enableForFastTravel, "");

            //Keys
            ConfigEntry<KeyCode> cfg_enable = config.Bind(keysTitle, keys_enable, enableKey, "");
            ConfigEntry<KeyCode> cfg_modifier = config.Bind(keysTitle, keys_modifier, modifierKey, "");
            ConfigEntry<KeyCode> cfg_positiveScale = config.Bind(keysTitle, keys_positiveScale, positiveScaleKey, "");
            ConfigEntry<KeyCode> cfg_negativeScale = config.Bind(keysTitle, keys_negativeScale, negativeScaleKey, "");
            ConfigEntry<KeyCode> cfg_moveLeft = config.Bind(keysTitle, keys_moveLeft, moveLeftKey, "");
            ConfigEntry<KeyCode> cfg_moveRight = config.Bind(keysTitle, keys_moveRight, moveRightKey, "");
            ConfigEntry<KeyCode> cfg_moveUp = config.Bind(keysTitle, keys_moveUp, moveUpKey, "");
            ConfigEntry<KeyCode> cfg_moveDown = config.Bind(keysTitle, keys_moveDown, moveDownKey, "");
            ConfigEntry<KeyCode> cfg_drag = config.Bind(keysTitle, keys_drag, dragKey, "");
            ConfigEntry<KeyCode> cfg_mirror = config.Bind(keysTitle, keys_mirror, mirrorKey, "");
            ConfigEntry<KeyCode> cfg_copy = config.Bind(keysTitle, keys_copy, copyKey, "");
            ConfigEntry<KeyCode> cfg_paste = config.Bind(keysTitle, keys_paste, pasteKey, "");
            ConfigEntry<KeyCode> cfg_cycleAxis = config.Bind(keysTitle, keys_cycleAxis, cycleAxisKey, "");
            ConfigEntry<KeyCode> cfg_cycleScaleGizmo = config.Bind(keysTitle, keys_cycleScaleGizmo, cycleScaleKey, "");
            ConfigEntry<KeyCode> cfg_fastTravelKey = config.Bind(keysTitle, keys_fastTravel, fastTravelKey, "");

            //Values
            ConfigEntry<string> cfg_valuesXZ = config.Bind(valuesTitle, values_XZ, defaultXZList, "");
            ConfigEntry<string> cfg_valuesY = config.Bind(valuesTitle, values_Y, defaultYList, "");
            ConfigEntry<string> cfg_valuesR = config.Bind(valuesTitle, values_R, defaultRList, "");
            ConfigEntry<string> cfg_valuesS = config.Bind(valuesTitle, values_S, defaultSList, "");

            //Default indices
            ConfigEntry<int> cfg_valuesXZIndex = config.Bind(valuesTitle, values_XZ_default_index, gridXZListIndex, "");
            ConfigEntry<int> cfg_valuesYIndex = config.Bind(valuesTitle, values_Y_default_index, gridYListIndex, "");
            ConfigEntry<int> cfg_valuesRIndex = config.Bind(valuesTitle, values_R_default_index, gridRListIndex, "");
            ConfigEntry<int> cfg_valuesSIndex = config.Bind(valuesTitle, values_S_default_index, gridSListIndex, "");
            
            cfg.SettingChanged += Cfg_SettingChanged;
            Cfg_SettingChanged(null, null);
        }

        private static void Cfg_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            try
            {
                //Preferences
                invertScrollWheel = (bool)config[preferencesTitle, preferences_invertScrollwheel].BoxedValue;
                includePlanesInCycle = (bool)config[preferencesTitle, preferences_includePlanesInCycle].BoxedValue;
                doubleLoadButtons = (bool)config[preferencesTitle, preferences_doubleLoadButtons].BoxedValue;

                //Controls
                scrollScaling = (bool)config[controlSettingsTitle, control_scrollScaling].BoxedValue;
                enableForScrollScale = (bool)config[controlSettingsTitle, control_enableForScrollScaling].BoxedValue;
                keyScaling = (bool)config[controlSettingsTitle, control_keyScaling].BoxedValue;
                enableForKeyScale = (bool)config[controlSettingsTitle, control_enableForKeyScaling].BoxedValue;
                keyMove = (bool)config[controlSettingsTitle, control_keyMove].BoxedValue;
                enableForKeyMove = (bool)config[controlSettingsTitle, control_enableForKeyMove].BoxedValue;
                mmbDrag = (bool)config[controlSettingsTitle, control_mmbDrag].BoxedValue;
                enableForMMBDrag = (bool)config[controlSettingsTitle, control_enableForMMBDrag].BoxedValue;
                keyDrag = (bool)config[controlSettingsTitle, control_keyDrag].BoxedValue;
                enableForKeyDrag = (bool)config[controlSettingsTitle, control_enableForKeyDrag].BoxedValue;
                keyMirror = (bool)config[controlSettingsTitle, control_keyMirror].BoxedValue;
                enableForKeyMirror = (bool)config[controlSettingsTitle, control_enableForKeyMirror].BoxedValue;
                clipboard = (bool)config[controlSettingsTitle, control_clipboard].BoxedValue;
                enableForClipboard = (bool)config[controlSettingsTitle, control_enableForClipboard].BoxedValue;
                fastTravel = (bool)config[controlSettingsTitle, control_fastTravel].BoxedValue;
                enableForFastTravel = (bool)config[controlSettingsTitle, control_enableForFastTravel].BoxedValue;

                //Keys
                enableKey = (KeyCode)config[keysTitle, keys_enable].BoxedValue;
                modifierKey = (KeyCode)config[keysTitle, keys_modifier].BoxedValue;
                positiveScaleKey = (KeyCode)config[keysTitle, keys_positiveScale].BoxedValue;
                negativeScaleKey = (KeyCode)config[keysTitle, keys_negativeScale].BoxedValue;
                moveLeftKey = (KeyCode)config[keysTitle, keys_moveLeft].BoxedValue;
                moveRightKey = (KeyCode)config[keysTitle, keys_moveRight].BoxedValue;
                moveUpKey = (KeyCode)config[keysTitle, keys_moveUp].BoxedValue;
                moveDownKey = (KeyCode)config[keysTitle, keys_moveDown].BoxedValue;
                dragKey = (KeyCode)config[keysTitle, keys_drag].BoxedValue;
                mirrorKey = (KeyCode)config[keysTitle, keys_mirror].BoxedValue;
                copyKey = (KeyCode)config[keysTitle, keys_copy].BoxedValue;
                pasteKey = (KeyCode)config[keysTitle, keys_paste].BoxedValue;
                cycleAxisKey = (KeyCode)config[keysTitle, keys_cycleAxis].BoxedValue;
                cycleScaleKey = (KeyCode)config[keysTitle, keys_cycleScaleGizmo].BoxedValue;
                fastTravelKey = (KeyCode)config[keysTitle, keys_fastTravel].BoxedValue;

                if(includePlanesInCycle)
                {
                    //BPXUI.chosenGizmoSelectionCycle = BPXUI.extendedGizmoSelectionCycle;
                    BPXGizmoManager.chosenGizmoSelectionCycle = BPXGizmoManager.extendedGizmoSelectionCycle;
                }
                else
                {
                    //BPXUI.chosenGizmoSelectionCycle = BPXUI.standardGizmoSelectionCycle;
                    BPXGizmoManager.chosenGizmoSelectionCycle = BPXGizmoManager.standardGizmoSelectionCycle;
                }

                gridXZList = ParseValueList(values_XZ, defaultXZList);
                gridYList = ParseValueList(values_Y, defaultYList);
                gridRList = ParseValueList(values_R, defaultRList);
                gridSList = ParseValueList(values_S, defaultSList);

                gridXZListIndex = ParseValueListIndex(gridXZList, values_XZ_default_index);
                gridYListIndex = ParseValueListIndex(gridXZList, values_Y_default_index);
                gridRListIndex = ParseValueListIndex(gridXZList, values_R_default_index);
                gridSListIndex = ParseValueListIndex(gridXZList, values_S_default_index);

                ApplyGridLists();
            }
            catch(Exception ex)
            {
                Debug.Log(ex);
            }
        }

        public static int ParseValueListIndex(List<float> list, string key)
        {
            try
            {
                int i = (int)config[valuesTitle, key].BoxedValue;
                if(i >= 0 && i < list.Count)
                {
                    return i;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public static List<float> ParseValueList(string key, string defaultValue)
        {
            try
            {
                string valueList = (string)config[valuesTitle, key].BoxedValue;
                List<float> parsedList = valueList.Split(';').Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToList();

                if (parsedList.Count == 0)
                {
                    parsedList = defaultValue.Split(';').Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToList();
                }

                return parsedList;
            }
            catch
            {
                return defaultValue.Split(';').Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToList();
            }
        }

        public static void ApplyGridLists()
        {
            if (BPXManager.central != null)
            {
                //Set the value of the XZ gizmo
                BPXManager.central.gizmos.list_gridXZ = gridXZList;
                BPXManager.central.gizmos.startiXZ = gridXZListIndex;
                //Set the value of the Y gizmo
                BPXManager.central.gizmos.list_gridY = gridYList;
                BPXManager.central.gizmos.startiY = gridYListIndex;
                //Set the value of the R gizmo
                BPXManager.central.gizmos.list_gridR = gridRList;
                BPXManager.central.gizmos.startiR = gridRListIndex;

                //Redraw
                BPXManager.central.gizmos.RedrawGridButtons();

                //Set the S gizmo
                BPXUI.gizmoScaleValues = gridSList.ToArray();
                BPXUI.currentGizmoScaleValueIndex = gridSListIndex;
                BPXUI.SetScaleGizmoButtonValue(gridSListIndex);                
            }
        }

        private static string CreateConfigLabel(int index, string label)
        {
            string formattedIndex = index < 10 ? $"0{index}" : index.ToString();
            return ($"{formattedIndex}. {label}").PadRight(configLabelLength) + "|";
        }
    }
}

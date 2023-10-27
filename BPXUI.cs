using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace BlueprintsX
{
    //This class will handle everything that is visible on screen like menu's and gizmos.
    public static class BPXUI
    {
        public static float[] gizmoScaleValues = new float[] { 0.05f, 0.5f, 1f, 2f, 10f, 20f, 50f };
        public static int currentGizmoScaleValueIndex = 2;

        private static LEV_CustomButton bpxLoadButton;
        private static LEV_CustomButton bpxSaveButton;
        private static TextMeshProUGUI scaleButtonValueLabel;
        private static Transform blueprintSavePanel;

        //UI elements that need to be accessed after initialization.
        private static LEV_CustomButton saveButton;
        private static LEV_CustomButton saveButtonSmallLeft;
        private static LEV_CustomButton saveButtonSmallRight;
        private static TextMeshProUGUI alreadyExistsText;
        private static RectTransform areYouSurePanel;
        private static RectTransform newFolderDialogPanel;
        private static TMP_InputField newFolderNameText;
        private static TMP_InputField fileName;
        private static TextMeshProUGUI URLText;
        private static RectTransform explorerPanel;
        private static TextMeshProUGUI zeepfileTypeText;
        private static LEV_CustomButton blueprintLevelSwitchButton;
        private static TMP_InputField searchBar;
        private static LEV_CustomButton previewContainer;

        //States
        public static bool savePanelIsOpen = false;
        public static bool savePanelInSaveMode = false;
        public static bool savePanelInBlueprintMode = true;
        
        //Save panel directories
        public static DirectoryInfo levelDirectory;
        public static DirectoryInfo blueprintDirectory;

        //List of all buttons for resetting the bools without creating a variable for each button.
        private static List<LEV_CustomButton> allButtons = new List<LEV_CustomButton>();
        private static List<LEV_FileContent> currentExplorerElements = new List<LEV_FileContent>();
        
        #region Initialization
        public static void Initialize()
        {
            if (BPXManager.central == null) { return; }

            #region Toolbar
            //Split the load and save buttons into two. One will have the default behaviour while the other will be used to open the blueprints save and load windows.
            bpxLoadButton = SplitLEVCustomButton(BPXManager.central.tool.button_load.transform, "Blueprint Load Button", () => { OnBlueprintLoadButton(); });
            bpxSaveButton = SplitLEVCustomButton(BPXManager.central.tool.button_save.transform, "Blueprint Save Button", () => { OnBlueprintSaveButton(); });
            #endregion

            #region Scaling Gizmo
            //Get the XZ and Y button and the Grid Size Label.
            RectTransform xzButtonRect = BPXManager.central.gizmos.value_XZ.transform.parent.GetComponent<RectTransform>();
            RectTransform yButtonRect = BPXManager.central.gizmos.value_Y.transform.parent.GetComponent<RectTransform>();
            RectTransform gridSizeTextRect = xzButtonRect.parent.GetChild(0).GetComponent<RectTransform>();

            //Calculate the distance between the top left corners of the two buttons to shift the elements up.
            Vector2 buttonShift = new Vector2(0, xzButtonRect.anchorMin.y - yButtonRect.anchorMin.y);

            //Create a copy of the XZ button and set the name.
            GameObject scaleButton = GameObject.Instantiate(xzButtonRect.gameObject, xzButtonRect.parent);
            RectTransform scaleButtonRect = scaleButton.GetComponent<RectTransform>();
            scaleButton.name = "S Grid Button";

            //Move the header up one button height.
            gridSizeTextRect.anchorMin += buttonShift;
            gridSizeTextRect.anchorMax += buttonShift;

            //Move the new scaling button up
            scaleButtonRect.anchorMin += buttonShift;
            scaleButtonRect.anchorMax += buttonShift;

            //Set a new color to the button.
            LEV_CustomButton scaleCustomButton = scaleButton.GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(scaleCustomButton, OnScaleGizmoButton);

            //Remove the hotkey button script
            scaleButton.GetComponent<LEV_HotkeyButton>().enabled = false;

            //Change the label text for the scaling button.
            scaleButtonRect.GetChild(2).GetComponent<TextMeshProUGUI>().text = "S";

            //Save a reference to the scale button value.
            scaleButtonValueLabel = scaleButtonRect.GetChild(1).GetComponent<TextMeshProUGUI>();
            SetScaleGizmoButtonValue(0);
            #endregion

            #region Save Panel
            //Create a copy of the save panel, which is the object that holds the LEV_SaveLoad
            blueprintSavePanel = GameObject.Instantiate(BPXManager.central.saveload.transform, BPXManager.central.saveload.transform.parent);
            blueprintSavePanel.gameObject.name = "Blueprint Save Panel";

            //Remove the LEV_SaveLoad component from the panel.
            GameObject.Destroy(blueprintSavePanel.GetComponent<LEV_SaveLoad>());

            //The actual save panel is stored as a child of this gameobject. Get the child for easy reference.
            RectTransform panel = blueprintSavePanel.GetChild(1).GetComponent<RectTransform>();
            panel.GetComponent<Image>().color = BPXManager.darkerBlue;
            panel.gameObject.SetActive(true);

            //Reset the list to prevent duplicates
            allButtons.Clear();

            //UI Buttons

            //The save button should be split into 3 different buttons
            // - A big button that can be used for saving, and build here.
            // - 2 smaller buttons than can be used for build here and build file.

            //First get the save button and reconfigure it.
            saveButton = panel.GetChild(0).GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(saveButton, OnPanelSaveButton);
            allButtons.Add(saveButton);

            //Create a copy of the save button.
            GameObject saveButtonSmallLeftObject = GameObject.Instantiate(saveButton.gameObject, saveButton.transform.parent);
            saveButtonSmallLeftObject.name = "Save Button Small Left";
            saveButtonSmallLeft = saveButtonSmallLeftObject.GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(saveButtonSmallLeft, OnPanelSaveButtonSmallLeft );
            saveButtonSmallLeft.transform.GetChild(0).GetComponent<Image>().sprite = BlueprintXPlugin.markerSprite;
            allButtons.Add(saveButtonSmallLeft);

            //Split it into  two to create the right button.
            saveButtonSmallRight = SplitLEVCustomButton(saveButtonSmallLeftObject.transform, "Save Button Small Right", () => { OnPanelSaveButtonSmallRight(); }, 0.05f );
            saveButtonSmallRight.transform.GetChild(0).GetComponent<Image>().sprite = BlueprintXPlugin.fileSprite;
            allButtons.Add(saveButtonSmallRight);

            //Take care of the rest of the buttons.
            LEV_CustomButton homeButton = panel.GetChild(2).GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(homeButton, OnPanelHomeButton);
            allButtons.Add(homeButton);

            LEV_CustomButton upOneFolderButton = panel.GetChild(1).GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(upOneFolderButton, OnPanelParentFolderButton);
            allButtons.Add(upOneFolderButton);            

            LEV_CustomButton newFolderButton = panel.GetChild(3).GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(newFolderButton, OnPanelNewFolderButton);
            allButtons.Add(newFolderButton);

            
            RectTransform homeButtonRectTransform = homeButton.GetComponent<RectTransform>();
            RectTransform upOneFolderButtonRectTransform = upOneFolderButton.GetComponent<RectTransform>();
            RectTransform newFolderButtonRectTransform = newFolderButton.GetComponent<RectTransform>();

            //Store the size of the original buttons.
            Vector2 sizeOfOriginalButton = homeButtonRectTransform.sizeDelta;
            //Calculate the padding between the 4 buttons, which is half of the distance currently between the two buttons.
            float newButtonPadding = (upOneFolderButtonRectTransform.anchorMin.x - homeButtonRectTransform.anchorMax.x) / 2f;
            //Calculate the total width of the two buttons
            float totalAvailableWidth = upOneFolderButtonRectTransform.anchorMax.x - homeButtonRectTransform.anchorMin.x;
            //We need to place 4 buttons and 3 paddings, so calculate the width of a single button.
            float singleButtonWidth = (totalAvailableWidth - 3 * newButtonPadding) / 4f;
            //Calculate the size change of the buttons.
            float sizeRatio = singleButtonWidth / (homeButtonRectTransform.anchorMax.x - homeButtonRectTransform.anchorMin.x);
            //Calculate the new size of the buttons
            Vector2 newButtonSize = new Vector2(singleButtonWidth, (homeButtonRectTransform.anchorMax.y - homeButtonRectTransform.anchorMin.y) * sizeRatio);

            //Set the new anchors of the home button.
            homeButtonRectTransform.anchorMax = new Vector2(homeButtonRectTransform.anchorMax.x + (sizeOfOriginalButton.x - newButtonSize.x), homeButtonRectTransform.anchorMax.y - newButtonSize.y);
            homeButtonRectTransform.anchorMin = new Vector2(homeButtonRectTransform.anchorMin.x, homeButtonRectTransform.anchorMax.y - newButtonSize.y);

            //Set the new anchors of the up folder button
            upOneFolderButtonRectTransform.anchorMax = new Vector2(homeButtonRectTransform.anchorMax.x + newButtonSize.x + newButtonPadding, homeButtonRectTransform.anchorMax.y);
            upOneFolderButtonRectTransform.anchorMin = new Vector2(homeButtonRectTransform.anchorMin.x + newButtonSize.x + newButtonPadding, homeButtonRectTransform.anchorMin.y);

            //Set the new anchors of the new folder button
            newFolderButtonRectTransform.anchorMax = new Vector2(upOneFolderButtonRectTransform.anchorMax.x + newButtonSize.x + newButtonPadding, upOneFolderButtonRectTransform.anchorMax.y);
            newFolderButtonRectTransform.anchorMin = new Vector2(upOneFolderButtonRectTransform.anchorMin.x + newButtonSize.x + newButtonPadding, upOneFolderButtonRectTransform.anchorMin.y);

            //Create a copy of the new folder button
            Transform blueprintLevelSwitchButtonObject = GameObject.Instantiate(newFolderButton.transform, newFolderButton.transform.parent);
            blueprintLevelSwitchButton = blueprintLevelSwitchButtonObject.GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(blueprintLevelSwitchButton, OnPanelBlueprintLevelSwitchButton);
            allButtons.Add(blueprintLevelSwitchButton);

            //Set the icon to the treegun logo.
            blueprintLevelSwitchButton.transform.GetChild(0).GetComponent<Image>().sprite = BlueprintXPlugin.fileSwitchSprite;

            //Set the new anchors for the blueprint level switch button.
            RectTransform blueprintLevelSwitchButtonRectTransform = blueprintLevelSwitchButton.GetComponent<RectTransform>();
            blueprintLevelSwitchButtonRectTransform.anchorMax = new Vector2(newFolderButtonRectTransform.anchorMax.x + newButtonSize.x + newButtonPadding, newFolderButtonRectTransform.anchorMax.y);
            blueprintLevelSwitchButtonRectTransform.anchorMin = new Vector2(newFolderButtonRectTransform.anchorMin.x + newButtonSize.x + newButtonPadding, newFolderButtonRectTransform.anchorMin.y);

            //Top left buttons of the panel.
            LEV_CustomButton openInExplorerButton = panel.GetChild(4).GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(openInExplorerButton, OnPanelOpenInExplorerButton);
            allButtons.Add(openInExplorerButton);

            LEV_CustomButton exitSavePanelButton = panel.GetChild(5).GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(exitSavePanelButton, CloseSavePanel);
            allButtons.Add(exitSavePanelButton);

            //Top path text
            URLText = panel.GetChild(6).GetComponent<TextMeshProUGUI>();

            //Scrollview related
            ScrollRect scrollRect = panel.GetChild(7).GetComponent<ScrollRect>();
            explorerPanel = scrollRect.content;

            ContentSizeFitter contentSizeFitter = explorerPanel.gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GridLayoutGroup groupLayoutGroup = explorerPanel.gameObject.AddComponent<GridLayoutGroup>();
            groupLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            groupLayoutGroup.constraintCount = 6;

            int horizontalGroupPadding = Mathf.RoundToInt(scrollRect.viewport.rect.width / 100f);
            float iconSize = (scrollRect.viewport.rect.width - horizontalGroupPadding) / 6f;
            float spacing = (scrollRect.viewport.rect.width - horizontalGroupPadding) / 6f * 0.05f;            

            groupLayoutGroup.cellSize = new Vector2(iconSize - spacing, iconSize - spacing);
            groupLayoutGroup.spacing = new Vector2(spacing, spacing);
            groupLayoutGroup.padding = new RectOffset(horizontalGroupPadding, horizontalGroupPadding, horizontalGroupPadding, horizontalGroupPadding);

            //Filename text box
            fileName = panel.GetChild(8).GetComponent<TMP_InputField>();
            fileName.GetComponent<Image>().color = Color.white;

            //Create a copy of the filename text box for the searchbar
            Transform searchBarObject = GameObject.Instantiate(fileName.transform, fileName.transform.parent);
            searchBar = searchBarObject.GetComponent<TMP_InputField>();
            searchBarObject.GetComponent<Image>().color = Color.white;

            GameObject.Destroy(searchBar.placeholder.GetComponent<I2.Loc.Localize>());
            searchBar.placeholder.GetComponent<TMP_Text>().text = "Search...";

            //Move the searchbar underneath the custom buttons.
            RectTransform searchBarRectTransform = searchBar.GetComponent<RectTransform>();
            searchBarRectTransform.anchorMax = new Vector2(blueprintLevelSwitchButtonRectTransform.anchorMax.x, blueprintLevelSwitchButtonRectTransform.anchorMax.y - newButtonSize.y - newButtonPadding);
            searchBarRectTransform.anchorMin = new Vector2(homeButtonRectTransform.anchorMin.x, homeButtonRectTransform.anchorMin.y - (newButtonSize.y * 0.75f) - newButtonPadding);

            searchBar.onValueChanged.AddListener(delegate { OnSearchBarValueChanged(); });

            //Create a button that will be used for a preview container
            Transform previewButtonObject = GameObject.Instantiate(homeButton.transform, homeButton.transform.parent);
            LEV_CustomButton previewButton = previewButtonObject.GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(previewButton, OnPreviewClicked);
            previewButton.clickColor = Color.black;
            previewButton.hoverColor = Color.black;
            previewButton.normalColor = Color.black;
            RectTransform previewButtonRectTransform = previewButtonObject.GetComponent<RectTransform>();
            previewButtonRectTransform.anchorMax = new Vector2(blueprintLevelSwitchButtonRectTransform.anchorMax.x,  searchBarRectTransform.anchorMin.y - newButtonPadding);
            previewButtonRectTransform.anchorMin = new Vector2(homeButtonRectTransform.anchorMin.x, scrollRect.transform.GetComponent<RectTransform>().anchorMin.y);

            RectTransform previewButtonImageRect = previewButton.transform.GetChild(0).GetComponent<RectTransform>();
            previewButtonImageRect.anchorMax = Vector2.one;
            previewButtonImageRect.anchorMin = Vector2.zero;

            previewButton.transform.GetChild(0).GetComponent<Image>().sprite = BlueprintXPlugin.blackPixelSprite;
            previewContainer = previewButton;

            //Are you sure panel
            areYouSurePanel = panel.GetChild(9).GetComponent<RectTransform>();
            alreadyExistsText = areYouSurePanel.GetChild(1).GetComponent<TextMeshProUGUI>();

            LEV_CustomButton areYouSureSaveButton = areYouSurePanel.GetChild(2).GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(areYouSureSaveButton, OnPanelAreYouSureSave);
            allButtons.Add(areYouSureSaveButton);

            LEV_CustomButton areYouSureCancelButton = areYouSurePanel.GetChild(3).GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(areYouSureCancelButton, OnPanelAreYouSureCancel);
            allButtons.Add(areYouSureCancelButton);

            //Text block inside panel
            zeepfileTypeText = panel.GetChild(10).GetComponent<TextMeshProUGUI>();
            //Move the text above the custom buttons.
            RectTransform zeepfileTypeTextRectTransform = zeepfileTypeText.GetComponent<RectTransform>();
            zeepfileTypeTextRectTransform.anchorMax = new Vector2(blueprintLevelSwitchButtonRectTransform.anchorMax.x, blueprintLevelSwitchButtonRectTransform.anchorMax.y + newButtonSize.y);
            zeepfileTypeTextRectTransform.anchorMin = new Vector2(homeButtonRectTransform.anchorMin.x, homeButtonRectTransform.anchorMin.y + newButtonSize.y);

            //Create new folder panel
            newFolderDialogPanel = panel.GetChild(11).GetComponent<RectTransform>();
            Transform newFolderChildPanel = newFolderDialogPanel.GetChild(0);

            LEV_CustomButton newFolderCancelButton = newFolderChildPanel.GetChild(0).GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(newFolderCancelButton, OnNewFolderExit);
            allButtons.Add(newFolderCancelButton);

            LEV_CustomButton newFolderCreateButton = newFolderChildPanel.GetChild(1).GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(newFolderCreateButton, OnNewFolderCreate);
            allButtons.Add(newFolderCreateButton);

            newFolderNameText = newFolderChildPanel.GetChild(2).GetComponent<TMP_InputField>();
            #endregion

            //Initialize the directories.
            levelDirectory = new DirectoryInfo(BPXManager.levelHomeDirectory);
            blueprintDirectory = new DirectoryInfo(BPXManager.blueprintHomeDirectory);

            //Change the order of the UI
            areYouSurePanel.SetAsLastSibling();
            newFolderDialogPanel.SetAsLastSibling();

            if (BPXConfig.doubleLoadButtons)
            {
                saveButton.gameObject.SetActive(false);
                saveButtonSmallLeft.gameObject.SetActive(true);
                saveButtonSmallRight.gameObject.SetActive(true);
            }
            else
            {
                saveButton.gameObject.SetActive(true);
                saveButtonSmallLeft.gameObject.SetActive(false);
                saveButtonSmallRight.gameObject.SetActive(false);
            }

            RefreshSavePanel();
        }

        private static void ReconfigureCustomButton(LEV_CustomButton button, UnityAction action)
        {
            button.normalColor = BPXManager.blue;
            button.overrideNormalColor = true;
            button.buttonImage.color = button.normalColor;
            button.onClick.RemoveAllListeners();

            for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                button.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
            }

            button.onClick.AddListener(action);
        }

        public static LEV_CustomButton SplitLEVCustomButton(Transform original, string objectName, UnityAction buttonAction, float padding = 0)
        {
            //Get the rect of the original button and calculate the size of the button based on the anchor points.
            RectTransform originalButtonRect = original.GetComponent<RectTransform>();
            Vector2 originalButtonSize = originalButtonRect.anchorMax - originalButtonRect.anchorMin;

            float paddingAmount = originalButtonSize.x * padding;

            //Duplicate the original and set the name.
            GameObject addedButton = GameObject.Instantiate(original.gameObject, original.parent);
            addedButton.gameObject.name = objectName;
            RectTransform addedButtonRect = addedButton.GetComponent<RectTransform>();

            //Resize the original button so it only takes up half the horizontal space.
            originalButtonRect.anchorMax = new Vector2(originalButtonRect.anchorMax.x - (originalButtonSize.x / 2) - paddingAmount, originalButtonRect.anchorMax.y);

            //Resize the added button to take up the rest of the space.            
            addedButtonRect.anchorMin = new Vector2(addedButtonRect.anchorMin.x + originalButtonSize.x / 2 + paddingAmount, addedButtonRect.anchorMin.y);

            //Get the LEV_CustomButton script of the new button and set the color and on click listener.
            LEV_CustomButton addedCustomButton = addedButton.GetComponent<LEV_CustomButton>();
            ReconfigureCustomButton(addedCustomButton, buttonAction);

            //Disable the hotkey script.
            LEV_HotkeyButton hotkeybutton = addedButton.GetComponent<LEV_HotkeyButton>();
            if(hotkeybutton != null)
            {
                hotkeybutton.enabled = false;
            }

            return addedCustomButton;
        }
        #endregion

        #region Toolbar
        public static void OnBlueprintLoadButton()
        {
            BPXManager.central.selection.DeselectAllBlocks(true, nameof(BPXManager.central.selection.ClickNothing));
            OpenSavePanel(true, false);
        }

        public static void OnBlueprintSaveButton()
        {
            if(BPXManager.central.selection.list.Count > 0)
            {
                BPXManager.createdBlueprintFromEditor = BPXIO.CreateBlueprintFromSelection(BPXManager.central.selection.list, PlayerManager.Instance.steamAchiever.GetPlayerName(true));
                //BPXManager.createdBlueprintFromEditor = BPXIO.CreateBlueprintFromSelection(BPXManager.central.selection.list, "Bouwerman");

                savePanelInBlueprintMode = true;

                RefreshSavePanel();
                OpenSavePanel(true, true);

                //Generate a new thumbnail for this blueprint.
                BPXRenderer.GenerateThumbnail(BPXManager.createdBlueprintFromEditor, new BPXRenderParameters(new Vector2Int(512, 512), renderTag: "Save", imageCount: 4));
            }
            else
            {
                PlayerManager.Instance.messenger.Log("No selection!", 3f);
            }
        }
        #endregion

        #region Gizmo        
        public static void OnScaleGizmoButton()
        {
            if(BPXManager.central.input.MultiSelect.buttonHeld)
            {
                ScaleGizmoChange(false);
            }
            else
            {
                ScaleGizmoChange(true);
            }
        }

        public static void ScaleGizmoChange(bool forward)
        {
            if(forward)
            {
                if(currentGizmoScaleValueIndex + 1 == gizmoScaleValues.Length)
                {
                    currentGizmoScaleValueIndex = 0;
                }
                else
                {
                    currentGizmoScaleValueIndex++;
                }
            }
            else
            {
                if(currentGizmoScaleValueIndex == 0)
                {
                    currentGizmoScaleValueIndex = gizmoScaleValues.Length - 1;
                }
                else
                {
                    currentGizmoScaleValueIndex--;
                }
            }

            SetScaleGizmoButtonValue(currentGizmoScaleValueIndex);
        }

        public static void SetScaleGizmoButtonValue(int gizmoScaleValueIndex)
        {
            float value = gizmoScaleValues[gizmoScaleValueIndex];
            //string buttonText = "";
            string buttonText = value.ToString(CultureInfo.InvariantCulture);

            /*
            if(value < 1f)
            {
                buttonText = value.ToString("0.0#");
                buttonText = buttonText.Substring(1);
            }
            else
            {
                buttonText = Mathf.RoundToInt(value).ToString();
            }*/

            scaleButtonValueLabel.text = buttonText;
        }
        
        public static GizmoValues GetGizmoValues()
        {
            return new GizmoValues()
            {
                XZ = BPXManager.central.gizmos.list_gridXZ[BPXManager.central.gizmos.index_gridXZ],
                Y = BPXManager.central.gizmos.list_gridY[BPXManager.central.gizmos.index_gridY],
                R = BPXManager.central.gizmos.list_gridR[BPXManager.central.gizmos.index_gridR],
                S = gizmoScaleValues[currentGizmoScaleValueIndex]
            };
        }
        #endregion
    
        #region SavePanel
        public static void OpenSavePanel(bool blueprintMode, bool openInSaveMode)
        {
            if (BPXManager.central == null) { return; }

            savePanelIsOpen = true;

            if (openInSaveMode)
            {
                savePanelInSaveMode = true;
                saveButton.transform.GetChild(0).GetComponent<Image>().sprite = BPXManager.central.saveload.saveImage;
                fileName.interactable = true;
                bpxSaveButton.isSelected = true;

                saveButtonSmallLeft.gameObject.SetActive(false);
                saveButtonSmallRight.gameObject.SetActive(false);
                saveButton.gameObject.SetActive(true);

                zeepfileTypeText.text = "Save Blueprint";

                searchBar.gameObject.SetActive(false);
            }
            else
            {
                savePanelInSaveMode = false;
                saveButton.transform.GetChild(0).GetComponent<Image>().sprite = BPXManager.central.saveload.loadImage;
                fileName.interactable = false;
                bpxLoadButton.isSelected = true;

                if(BPXConfig.doubleLoadButtons)
                {
                    saveButton.gameObject.SetActive(false);
                    saveButtonSmallLeft.gameObject.SetActive(true);
                    saveButtonSmallRight.gameObject.SetActive(true);
                }
                else
                {
                    saveButton.gameObject.SetActive(true);
                    saveButtonSmallLeft.gameObject.SetActive(false);
                    saveButtonSmallRight.gameObject.SetActive(false);
                }

                if(BPXManager.selectedBlueprintDuringLoading != null)
                {
                    ReloadBlueprintFromSelectedFile();
                }
                else
                {
                    previewContainer.transform.GetChild(0).GetComponent<Image>().sprite = BlueprintXPlugin.blackPixelSprite;
                    fileName.text = "";
                }

                zeepfileTypeText.text = "Import Blueprint";
                searchBar.gameObject.SetActive(true);
            }

            BPXManager.central.tool.DisableAllTools();
            BPXManager.central.tool.RecolorButtons();
            BPXManager.central.tool.currentTool = 3;
            BPXManager.central.tool.inspectorTitle.text = "";


            blueprintSavePanel.gameObject.SetActive(true);
            RefreshSavePanel();
            ResetAllButtons();
        }

        public static void CloseSavePanel()
        {
            if (BPXManager.central == null) { return; }

            BPXManager.central.tool.EnableEditTool();
            BPXManager.central.tool.RecolorButtons();
            BPXManager.central.cam.OverrideOutsideGameView(false);
            blueprintSavePanel.gameObject.SetActive(false);
            areYouSurePanel.gameObject.SetActive(false);
            savePanelIsOpen = false;
            ResetCustomToolbarButtons();
            ResetAllButtons();
        }

        public static void ResetAllButtons()
        {
            foreach (LEV_CustomButton b in allButtons)
            {
                b.ResetAllBools();
            }
        }

        public static void ResetCustomToolbarButtons()
        {
            bpxLoadButton.isSelected = false;
            bpxSaveButton.isSelected = false;
        }

        //Button Events
        public static void OnPanelSaveButton()
        {
            if(savePanelInSaveMode)
            {
                //Check the file name entry.
                string enteredFileName = fileName.text;

                //If no filename is given, show a message saying the user has to enter a name.
                if (string.IsNullOrEmpty(enteredFileName.Trim()))
                {
                    BPXManager.central.manager.messenger.LogError("Please enter a name!", 3f);
                    return;
                }

                //Remove the extension if the user has entered one.
                enteredFileName = enteredFileName.Replace(".zeeplevel", "");

                //Create the target path and check if there is already a file there.
                BPXManager.targetedSavePath = Path.Combine((savePanelInBlueprintMode ? blueprintDirectory.ToString() : levelDirectory.ToString()), enteredFileName + ".zeeplevel");

                //If the file aready exists, show the are you sure panel.
                if (File.Exists(BPXManager.targetedSavePath))
                {
                    //Show the are you sure panel with an overwrite message.
                    alreadyExistsText.text = "Overwrite?";
                    areYouSurePanel.gameObject.SetActive(true);
                }
                else
                {
                    //Save right now.
                    RefreshSavePanel();
                    CloseSavePanel();
                    BPXIO.SaveBlueprintAtTargetedPath();                                  
                }
            }
            else
            {
                //We are in loading mode. Check if there is a valid blueprint selected.
                if (BPXManager.selectedBlueprintDuringLoading != null)
                {
                    CloseSavePanel();
                    BPXIO.LoadBlueprintIntoEditor(BPXManager.selectedBlueprintDuringLoading, true);                    
                }
            }
        }

        public static void OnPanelSaveButtonSmallLeft()
        {
            if (BPXManager.selectedBlueprintDuringLoading != null)
            {
                CloseSavePanel();
                BPXIO.LoadBlueprintIntoEditor(BPXManager.selectedBlueprintDuringLoading, true);               
            }
        }

        public static void OnPanelSaveButtonSmallRight()
        {
            if (BPXManager.selectedBlueprintDuringLoading != null)
            {
                CloseSavePanel();
                BPXIO.LoadBlueprintIntoEditor(BPXManager.selectedBlueprintDuringLoading, false);
            }
        }

        public static void OnPanelParentFolderButton()
        {
            if(savePanelInBlueprintMode)
            {
                if(blueprintDirectory.Parent != null && blueprintDirectory.FullName != BPXManager.blueprintHomeDirectory)
                {
                    blueprintDirectory = blueprintDirectory.Parent;
                }
            }
            else
            {
                if (levelDirectory.Parent != null && levelDirectory.FullName != BPXManager.levelHomeDirectory)
                {
                    levelDirectory = levelDirectory.Parent;
                }
            }

            RefreshSavePanel();
        }

        public static void OnPanelHomeButton()
        {
            if(savePanelInBlueprintMode)
            {                
                blueprintDirectory = new DirectoryInfo(BPXManager.blueprintHomeDirectory);
            }
            else
            {
                levelDirectory = new DirectoryInfo(BPXManager.levelHomeDirectory);
            }

            RefreshSavePanel();
        }

        public static void OnPanelNewFolderButton()
        {
            //Open the new folder dialog
            newFolderDialogPanel.gameObject.SetActive(true);

            ResetAllButtons();
        }

        public static void OnPanelBlueprintLevelSwitchButton()
        {
            savePanelInBlueprintMode = !savePanelInBlueprintMode;            

            RefreshSavePanel();
        }

        public static void OnPanelOpenInExplorerButton()
        {
            string path = savePanelInBlueprintMode ? blueprintDirectory.FullName : levelDirectory.FullName;

            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                Process.Start("explorer.exe", path);
            }
            else
            {
                BPXManager.central.manager.messenger.LogError("Not supported on this platform", 3f);
            }
        }

        //This will only be shown when saving.
        public static void OnPanelAreYouSureSave()
        {
            BPXIO.SaveBlueprintAtTargetedPath();
            RefreshSavePanel();
            CloseSavePanel();
        }

        public static void OnPanelAreYouSureCancel()
        {
            areYouSurePanel.gameObject.SetActive(false);
        }
        
        public static void OnNewFolderCreate()
        {
            if(newFolderNameText.text.Trim() == "")
            {
                BPXManager.central.manager.messenger.LogError("Invalid folder name", 3f);
                return;
            }

            string newFolderPath = (savePanelInBlueprintMode ? blueprintDirectory.FullName : levelDirectory.FullName) + "/" + newFolderNameText.text;

            if(!Directory.Exists(newFolderPath))
            {
                Directory.CreateDirectory(newFolderPath);
            }
            else
            {
                BPXManager.central.manager.messenger.LogError(newFolderNameText.text + " already exists", 3f);
            }

            OnNewFolderExit();
        }

        public static void OnNewFolderExit()
        {
            //Reset the input field for next time.
            newFolderNameText.text = "";

            //Disable the dialog
            newFolderDialogPanel.gameObject.SetActive(false);

            ResetAllButtons();

            RefreshSavePanel();
        }

        //Explorer
        public static void ReloadBlueprintFromSelectedFile()
        {
            if (BPXManager.selectedBlueprintDuringLoading != null)
            {
                try
                {
                    string path = BPXManager.selectedBlueprintDuringLoading.path;
                    Blueprint blueprintFromSelectedFile = BPXIO.ReadBlueprintFromFile(path);
                    blueprintFromSelectedFile.path = path;

                    //Save a reference to the loaded blueprint.
                    BPXManager.selectedBlueprintDuringLoading = blueprintFromSelectedFile;

                    //Set the info text to the creator of the blueprint.
                    //infoText.text = "by " + blueprintFromSelectedFile.creator;

                    //Set the file name input field to the selected file
                    fileName.text = blueprintFromSelectedFile.title;

                    //Generate a new thumbnail for this blueprint.
                    BPXRenderer.GenerateThumbnail(blueprintFromSelectedFile, new BPXRenderParameters(new Vector2Int(512, 512), renderTag: "Load", imageCount: 4));
                }
                catch
                {
                    BPXManager.selectedBlueprintDuringLoading = null;
                    previewContainer.transform.GetChild(0).GetComponent<Image>().sprite = BlueprintXPlugin.blackPixelSprite;
                    fileName.text = "";
                    
                    if (savePanelInBlueprintMode)
                    {
                        blueprintDirectory = new DirectoryInfo(BPXManager.blueprintHomeDirectory);
                    }
                    else
                    {
                        levelDirectory = new DirectoryInfo(BPXManager.levelHomeDirectory);
                    }
                }
            }
        }

        public static void OnFileSelectedInExplorer(FileInfo fileInfo)
        {
            if (savePanelInSaveMode)
            {
                //When we are in save mode, we are only really interested in the selected name.
                //We dont need to load the selected blueprint.
                fileName.text = Path.GetFileNameWithoutExtension(fileInfo.FullName);
            }
            //Load mode
            else
            {
                //As only zeeplevels are allowed as files in the explorer, this is a zeeplevel file.
                //Read the file into a blueprint
                Blueprint blueprintFromSelectedFile = BPXIO.ReadBlueprintFromFile(fileInfo.FullName);

                if (blueprintFromSelectedFile == null)
                {
                    Debug.LogError("The blueprint returned null. [OnFileSelectedInExplorer]");
                    return;
                }

                blueprintFromSelectedFile.path = fileInfo.FullName;

                //Save a reference to the loaded blueprint.
                BPXManager.selectedBlueprintDuringLoading = blueprintFromSelectedFile;

                //Set the info text to the creator of the blueprint.
                //infoText.text = "by " + blueprintFromSelectedFile.creator;

                //Set the file name input field to the selected file
                fileName.text = blueprintFromSelectedFile.title;

                //Generate a new thumbnail for this blueprint.
                BPXRenderer.GenerateThumbnail(blueprintFromSelectedFile, new BPXRenderParameters(new Vector2Int(512, 512), renderTag: "Load", imageCount: 4));
            }
        }

        public static void OnDirectorySelectedInExplorer(DirectoryInfo directoryInfo)
        {
            //When a directory gets selected in the explorer, navigate to that directory.
            if(savePanelInBlueprintMode)
            {
                blueprintDirectory = directoryInfo;
            }
            else
            {
                levelDirectory = directoryInfo;
            }

            RefreshSavePanel();
        }

        public static void RefreshSavePanel()
        {
            try
            {
                //Get the files and directories.
                DirectoryInfo dir = savePanelInBlueprintMode ? blueprintDirectory : levelDirectory;

                FileInfo[] files;
                DirectoryInfo[] allDirectories;
                List<DirectoryInfo> directories = new List<DirectoryInfo>();
                Dictionary<string, string> directoriesToRename = new Dictionary<string, string>();

                if (searchBar.text == "" || savePanelInSaveMode)
                {
                    files = dir.GetFiles("*.zeeplevel");
                    allDirectories = dir.GetDirectories();

                    //Go over each directory and check if they are allowed. They are only allowed if:
                    // - They are empty
                    // - Only contains files with extension .jpg (level thumbs) or .zeeplevel
                    for (int i = 0; i < allDirectories.Length; i++)
                    {
                        bool addThisDirectory = true;

                        //Get all files in this directory.
                        FileInfo[] currentDirectoryFiles = allDirectories[i].GetFiles();

                        int zeeplevelCount = 0;
                        string firstZeeplevelName = "";

                        //Only compare extensions if there are files in the directory.
                        if (currentDirectoryFiles.Length > 0)
                        {
                            foreach (FileInfo fileInfo in currentDirectoryFiles)
                            {
                                string fileExtension = fileInfo.Extension.ToLower();
                                if(fileExtension == ".zeeplevel")
                                {
                                    if (zeeplevelCount == 0)
                                    {
                                        firstZeeplevelName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                                    }
                                    zeeplevelCount++;
                                }
                                else if(!BPXConfig.allowedExtensions.Contains(fileExtension))
                                {
                                    addThisDirectory = false;
                                }
                                
                                /*switch (fileExtension)
                                {
                                    case ".png":
                                    case ".obj":
                                    case ".jpg":
                                    case ".realm":
                                    case ".zeeplist":
                                    case ".zip":
                                        break;
                                    case ".zeeplevel":
                                        if (zeeplevelCount == 0)
                                        {
                                            firstZeeplevelName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                                        }
                                        zeeplevelCount++;
                                        break;
                                    default:
                                        addThisDirectory = false;
                                        break;
                                }*/
                            }
                        }

                        if (addThisDirectory)
                        {
                            //The directory is allowed, check if its a mod.io folder.
                            if (Regex.IsMatch(Path.GetFileName(allDirectories[i].FullName), @"^\d{7}_\d{7}$"))
                            {
                                //This is mod.io folder.
                                //Does it have zeeplevels in it?
                                if (zeeplevelCount > 0)
                                {
                                    directoriesToRename.Add(allDirectories[i].Name, "*" + firstZeeplevelName);
                                }
                                else
                                {
                                    //Does this only have 1 directory?
                                    DirectoryInfo[] nestedDirs = allDirectories[i].GetDirectories();
                                    if (nestedDirs.Length == 1)
                                    {
                                        //This seems to be a nested mod folder.
                                        allDirectories[i] = nestedDirs[0];
                                    }
                                }
                            }

                            directories.Add(allDirectories[i]);
                        }
                    }
                }
                else
                {
                    //Set 0 folders.
                    allDirectories = new DirectoryInfo[0];

                    //Scan for files in all subfolders.
                    files = dir.GetFiles("*.zeeplevel", SearchOption.AllDirectories);

                    files = files.Where(file => file.Name.ToUpper().Contains(searchBar.text.ToUpper())).ToArray();
                }

                //Set the path on top of the panel.
                URLText.text = dir.ToString();

                //Clear all elements currently in the explorer
                for (int i = 0; i < currentExplorerElements.Count; i++)
                {
                    if (currentExplorerElements[i] != null)
                    {
                        currentExplorerElements[i].button.onClick.RemoveAllListeners();
                        GameObject.Destroy(currentExplorerElements[i].gameObject);
                    }
                }
                currentExplorerElements.Clear();

                //The total count of elements in the explorer
                int amountOfElements = directories.Count + files.Length;
                //The amount of objects displayed on each row.
                int columnCount = 6;
                //The amount of rows needed for all elements.
                int rowCount = Mathf.CeilToInt((float)amountOfElements / (float)columnCount);
                //The horizontal padding for each element.
                float horizontalPadding = 0.1f / (columnCount + 1);
                //The vertical padding for each element.
                float verticalPadding = 0.12f / rowCount;
                //The width of each element.
                float elementWidth = 0.9f / columnCount;
                //The height of each element.
                float elementHeight = 1f - (rowCount + 1) * verticalPadding / rowCount;

                //Flag for completion.
                bool allElementsPlaced = false;

                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < columnCount; col++)
                    {
                        //In case of half filled rows, we can quit halfway through
                        if (row * columnCount + col >= amountOfElements)
                        {
                            allElementsPlaced = true;
                            break;
                        }

                        //Create the element
                        LEV_FileContent element = GameObject.Instantiate(BPXManager.central.saveload.filePrefab);
                        element.central = BPXManager.central;

                        int currentButtonIndex = row * columnCount + col;
                        bool isDirectory = currentButtonIndex < directories.Count;

                        //Initialize the directory or file.
                        if (isDirectory)
                        {
                            element.directory = directories[currentButtonIndex];
                            if(directoriesToRename.ContainsKey(element.directory.Name))
                            {
                                element.fileNameText.text = directoriesToRename[element.directory.Name];
                            }
                            else
                            {
                                element.fileNameText.text = element.directory.Name;
                            }
                            
                            element.thumbnail.sprite = BPXManager.central.saveload.folder;
                            element.fileType = 0;
                            element.button.onClick.AddListener(() => OnDirectorySelectedInExplorer(element.directory));
                        }
                        else
                        {
                            FileInfo fileInfo = files[currentButtonIndex - directories.Count];
                            element.directory = fileInfo.Directory;
                            element.fileNameText.text = fileInfo.Name.Replace(".zeeplevel", "");
                            element.thumbnail.sprite = BPXManager.central.saveload.file;
                            element.fileType = 2;
                            element.button.onClick.AddListener(() => OnFileSelectedInExplorer(fileInfo));
                        }

                        //Positioning
                        element.transform.SetParent(explorerPanel, false);
                        float xPosition = horizontalPadding + (horizontalPadding * col) + (elementWidth * col);
                        float yPosition = 1f - (verticalPadding + (verticalPadding * row) + (elementHeight * row));

                        RectTransform elementRectTransform = element.GetComponent<RectTransform>();
                        elementRectTransform.anchorMin = new Vector2(xPosition, yPosition - elementHeight);
                        elementRectTransform.anchorMax = new Vector2(xPosition + elementWidth, yPosition);

                        currentExplorerElements.Add(element);
                    }

                    if (allElementsPlaced)
                    {
                        break;
                    }
                }

                if (savePanelInSaveMode)
                {
                    //Disable the switching button.
                    blueprintLevelSwitchButton.gameObject.SetActive(false);

                    //Set the text to save blueprint
                    zeepfileTypeText.text = "Save Blueprint";
                }
                else
                {
                    //Enable the switching button.
                    blueprintLevelSwitchButton.gameObject.SetActive(true);

                    //Load Mode
                    if (savePanelInBlueprintMode)
                    {
                        zeepfileTypeText.text = "Import Blueprint";
                    }
                    else
                    {
                        zeepfileTypeText.text = "Import Level";
                    }
                }
            }
            catch
            {
                //Error with the directory.
                //Go to home directory.
                OnPanelHomeButton();
            }
        }

        public static void ThumbnailGenerated()
        {
            if(savePanelInSaveMode)
            {
                if(BPXManager.createdBlueprintRender != null)
                {
                    //Setting save button icon.
                    previewContainer.transform.GetChild(0).GetComponent<Image>().sprite = BPXManager.createdBlueprintRender.captures[0];
                }
            }
            else
            {
                if (BPXManager.loadedBlueprintRender != null)
                {
                    previewContainer.transform.GetChild(0).GetComponent<Image>().sprite = BPXManager.loadedBlueprintRender.captures[0];
                }
            }
        }

        public static void OnPreviewClicked()
        {
            if (savePanelInSaveMode)
            {
                if (BPXManager.createdBlueprintRender != null)
                {
                    if(BPXManager.createdBlueprintRender.currentCaptureIndex == BPXManager.createdBlueprintRender.captures.Count - 1)
                    {
                        BPXManager.createdBlueprintRender.currentCaptureIndex = 0;
                    }
                    else
                    {
                        BPXManager.createdBlueprintRender.currentCaptureIndex++;
                    }

                    //Setting save button icon.
                    previewContainer.transform.GetChild(0).GetComponent<Image>().sprite = BPXManager.createdBlueprintRender.captures[BPXManager.createdBlueprintRender.currentCaptureIndex];
                }
            }
            else
            {
                if (BPXManager.loadedBlueprintRender != null)
                {
                    if (BPXManager.loadedBlueprintRender.currentCaptureIndex == BPXManager.loadedBlueprintRender.captures.Count - 1)
                    {
                        BPXManager.loadedBlueprintRender.currentCaptureIndex = 0;
                    }
                    else
                    {
                        BPXManager.loadedBlueprintRender.currentCaptureIndex++;
                    }

                    previewContainer.transform.GetChild(0).GetComponent<Image>().sprite = BPXManager.loadedBlueprintRender.captures[BPXManager.loadedBlueprintRender.currentCaptureIndex];
                }
            }
        }

        public static void OnSearchBarValueChanged()
        {
            RefreshSavePanel();
        }
        #endregion
    }      
}

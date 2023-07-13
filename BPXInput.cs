using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlueprintsX
{
    public static class BPXInput
    {
        public static void GetInputs()
        {
            //If LEV_LevelEditorCentral is not available, don't continue;
            if (BPXManager.central == null) { return; }

            //If the blueprint save panel is open we don't want to process any inputs.
            if (BPXUI.savePanelIsOpen) { return; }

            //If the regular save panel is open we don't want to process any inputs either???
            if (BPXManager.central.saveload.gameObject.activeSelf) { return; }

            //Return if not in building mode.
            if (BPXManager.central.tool.currentTool != 0){ return; }

            //Return if we are currently dragging
            if (BPXManager.central.gizmos.isDragging) { return; }

            //Return if we are currently grabbing.
            if (BPXManager.central.gizmos.isGrabbing) { return; }
            
            //Get the state of the enable key.
            bool enableKey = Input.GetKey(BPXConfig.enableKey);

            #region SaveLoad
            //If save shortcut is enabled.
            if(BPXConfig.saveShortcut)
            {
                //If enable key is pressed (mandatory for saving).
                if(enableKey)
                {
                    //If the save button is pressed.
                    if(Input.GetKeyDown(BPXConfig.saveShortcutKey))
                    {
                        //Click the blueprint save button in the toolbar.
                        BPXUI.OnBlueprintSaveButton();
                    }
                }
            }
            //If load shortcut is enabled.
            if(BPXConfig.loadShortcut)
            {
                //If enable key is pressed (mandatory for loading).
                if (enableKey)
                {
                    //If the load button is pressed.
                    if (Input.GetKeyDown(BPXConfig.loadShortcutKey))
                    {
                        //Click the blueprint load button in the toolbar.
                        BPXUI.OnBlueprintLoadButton();
                    }
                }
            }
            #endregion

            #region Scaling
            //Scrollwheel scaling.
            if (BPXConfig.scrollScaling)
            {
                //If we use the enable key for scroll scaling, check if we can proceed.
                if (BPXConfig.enableForScrollScale ? (enableKey ? true : false) : true)
                {
                    //Get the state of scrolling up and down.
                    bool scrollUp = Input.mouseScrollDelta.y > 0;
                    bool scrollDown = Input.mouseScrollDelta.y < 0;

                    //If the scroll wheel should be inverted, switch the states
                    if (BPXConfig.invertScrollWheel)
                    {
                        bool temp = scrollUp;
                        scrollUp = scrollDown;
                        scrollDown = temp;
                    }

                    //If we are scrolling up
                    if (scrollUp)
                    {
                        //Check the modifier key to see if we should scale in place.
                        bool inPlace = Input.GetKey(BPXConfig.modifierKey);
                        //Get the amount from the gizmo settings.
                        float amount = BPXUI.GetGizmoValues().S / 100f + 1f;
                        //Scale the selection on the selected axis.
                        ScaleSelection(BPXGizmoManager.GetCurrentAxis(), amount, inPlace);
                    }
                    //If we are scrolling down
                    else if (scrollDown)
                    {
                        //Check the modifier key to see if we should scale in place.
                        bool inPlace = Input.GetKey(BPXConfig.modifierKey);
                        //Get the amount from the gizmo settings.
                        float amount = BPXUI.GetGizmoValues().S;
                        amount = 1f / (1f + amount / 100f);
                        //SCale the section on the selected axis.
                        ScaleSelection(BPXGizmoManager.GetCurrentAxis(), amount, inPlace);
                    }
                }
            }

            //Key scaling
            if (BPXConfig.keyScaling)
            {
                //If we use the enable key for key scaling, check if we can proceed.
                if (BPXConfig.enableForKeyScale ? (enableKey ? true : false) : true)
                {
                    //If we press the scale up / positive scaling key.
                    if (Input.GetKeyDown(BPXConfig.positiveScaleKey))
                    {
                        //Check the modifier key to see if we should scale in place.
                        bool inPlace = Input.GetKey(BPXConfig.modifierKey);
                        //Get the amount from the gizmo settings.
                        float amount = BPXUI.GetGizmoValues().S / 100f + 1f;
                        //Scale the selection on the selected axis.
                        ScaleSelection(BPXGizmoManager.GetCurrentAxis(), amount, inPlace);
                    }

                    //If we press the scale down / negative scaling key.
                    if (Input.GetKeyDown(BPXConfig.negativeScaleKey))
                    {
                        //Check the modifier key to see if we should scale in place.
                        bool inPlace = Input.GetKey(BPXConfig.modifierKey);
                        //Get the amount from the gizmo settings.
                        float amount = BPXUI.GetGizmoValues().S;
                        amount = 1f / (1f + amount / 100f);
                        //Scale the selection on the selected axis.
                        ScaleSelection(BPXGizmoManager.GetCurrentAxis(), amount, inPlace);
                    }
                }
            }
            #endregion

            #region Movement
            //Key Movement
            if (BPXConfig.keyMove)
            {
                //If we use the enable key for key movement, check if we can proceed.
                if (BPXConfig.enableForKeyMove ? (enableKey ? true : false) : true)
                {
                    //If we are holding the modifier key, we are moving in the Y axis
                    if (Input.GetKey(BPXConfig.modifierKey))
                    {
                        //If the move up key is pressed.
                        if (Input.GetKeyDown(BPXConfig.moveUpKey))
                        {
                            //Move the selection in the up direction, based on the value in the Y gizmo.
                            MoveSelection(Vector3.up * BPXUI.GetGizmoValues().Y);
                        }
                        //If the move down key is pressed.
                        else if (Input.GetKeyDown(BPXConfig.moveDownKey))
                        {
                            //Move the selection in the down direction, based on the value in the Y gizmo.
                            MoveSelection(-Vector3.up * BPXUI.GetGizmoValues().Y);
                        }
                    }
                    //If the modifier key is not held, we are moving in the XZ plane.
                    else
                    {
                        //If the move right key is pressed.
                        if (Input.GetKeyDown(BPXConfig.moveRightKey))
                        {
                            //Move the selection in the right direction, based on the value in the XZ gizmo.
                            MoveSelection(Vector3.right * BPXUI.GetGizmoValues().XZ);
                        }
                        //If the move left key is pressed.
                        else if (Input.GetKeyDown(BPXConfig.moveLeftKey))
                        {
                            //Move the selection in the left direction, based on the value in the XZ gizmo.
                            MoveSelection(-Vector3.right * BPXUI.GetGizmoValues().XZ);
                        }
                        //If the move up key is pressed.
                        else if (Input.GetKeyDown(BPXConfig.moveUpKey))
                        {
                            //Move the selection in the forward direction, based on the value in the XZ gizmo.
                            MoveSelection(Vector3.forward * BPXUI.GetGizmoValues().XZ);
                        }
                        //If the move down key is pressed.
                        else if (Input.GetKeyDown(BPXConfig.moveDownKey))
                        {
                            //Move the selection in the backwards direction, based on the value in the XZ gizmo.
                            MoveSelection(-Vector3.forward * BPXUI.GetGizmoValues().XZ);
                        }
                    }
                }
            }
            #endregion

            //Dragging will be handled by its own seperate class because of the complexity.
            BPXDrag.Run();

            #region Mirror
            //Key Mirror
            if (BPXConfig.keyMirror)
            {
                if (BPXConfig.enableForKeyMirror ? (enableKey ? true : false) : true)
                {
                    if (Input.GetKeyDown(BPXConfig.mirrorKey))
                    {
                        MirrorSelection(BPXGizmoManager.GetCurrentAxis());
                    }
                }
            }
            #endregion

            #region Clipboard
            //Clipboard
            if (BPXConfig.clipboard)
            {
                if (BPXConfig.enableForClipboard ? (enableKey ? true : false) : true)
                {
                    if (Input.GetKeyDown(BPXConfig.copyKey))
                    {
                        CopySelection();
                    }
                    else if (Input.GetKeyDown(BPXConfig.pasteKey))
                    {
                        PasteClipboard();
                    }
                }
            }
            #endregion

            #region Traveling
            //Fast Traveling
            if (BPXConfig.fastTravel)
            {
                if (BPXConfig.enableForFastTravel ? (enableKey ? true : false) : true)
                {
                    if (Input.GetKeyDown(BPXConfig.fastTravelKey))
                    {
                        FastTravel();
                    }
                }
            }
            #endregion

            #region Cycling
            //Axis Cycling
            if (Input.GetKeyDown(BPXConfig.cycleAxisKey))
            {
                if (Input.GetKey(BPXConfig.modifierKey))
                {
                    CycleAxis(false);
                }
                else
                {
                    CycleAxis(true);
                }
            }

            //Scale Cycling
            if (Input.GetKeyDown(BPXConfig.cycleScaleKey))
            {
                if (BPXManager.central.input.MultiSelect.buttonHeld)
                {
                    BPXUI.ScaleGizmoChange(false);
                }
                else
                {
                    BPXUI.ScaleGizmoChange(true);
                }
            }
            #endregion
        }

        #region Scaling
        public static void ScaleSelection(Vector3 axis, float amount, bool inPlace)
        {
            // Check if BPXManager.central is null
            if (BPXManager.central == null) { return; }

            // Check if there are selected blocks
            if (BPXManager.central.selection.list.Count > 0)
            {
                if (!inPlace)
                {
                    // Scale the selected blocks
                    Scale(BPXManager.central.selection.list, axis, amount);
                }
                else
                {
                    // Scale the selected blocks in place
                    ScaleInPlace(BPXManager.central.selection.list, axis, amount);
                }
            }
        }

        public static void Scale(List<BlockProperties> blockList, Vector3 axis, float amount)
        {
            // Check if BPXManager.central is null
            if (BPXManager.central == null) { return; }

            // Convert the blockList to JSON before scaling
            List<string> before = BPXManager.central.undoRedo.ConvertBlockListToJSONList(blockList);

            // Calculate the center position of the blockList
            Vector3 center = BPXUtilities.GetCenterPosition(blockList);

            if (axis.x > 0 && axis.y > 0 && axis.z > 0)
            {
                // Scale uniformly
                foreach (BlockProperties bp in blockList)
                {
                    // Calculate the new position and scale of the block
                    Vector3 pos = bp.transform.position;
                    pos -= center;
                    pos *= amount;
                    pos += center;
                    bp.transform.position = pos;
                    bp.transform.localScale *= amount;
                }
            }
            else
            {
                if (axis.x > 0)
                {
                    // Scale on the world X axis
                    foreach (BlockProperties bp in blockList)
                    {
                        // Calculate the new position of the block
                        Vector3 pos = bp.transform.position;
                        pos -= center;
                        pos.x *= amount;
                        pos += center;
                        bp.transform.position = pos;

                        // Convert local to world vectors for scaling
                        Vector3[] convertedVectors = BPXUtilities.ConvertLocalToWorldVectors(bp.transform);

                        // Determine the scaled axis for scaling the block
                        Vector3 scaledAxis = Vector3.zero;

                        if (convertedVectors[0] == Vector3.right)
                        {
                            scaledAxis = Vector3.right;
                        }
                        else if (convertedVectors[1] == Vector3.right)
                        {
                            scaledAxis = Vector3.up;
                        }
                        else if (convertedVectors[2] == Vector3.right)
                        {
                            scaledAxis = Vector3.forward;
                        }

                        // Calculate the scale addition for the block
                        Vector3 scaleAddition = Vector3.Scale((bp.transform.localScale * amount - bp.transform.localScale), scaledAxis);
                        bp.transform.localScale += scaleAddition;
                    }
                }

                if (axis.y > 0)
                {
                    // Scale on the world Y axis
                    foreach (BlockProperties bp in blockList)
                    {
                        // Calculate the new position of the block
                        Vector3 pos = bp.transform.position;
                        pos -= center;
                        pos.y *= amount;
                        pos += center;
                        bp.transform.position = pos;

                        // Convert local to world vectors for scaling
                        Vector3[] convertedVectors = BPXUtilities.ConvertLocalToWorldVectors(bp.transform);

                        // Determine the scaled axis for scaling the block
                        Vector3 scaledAxis = Vector3.zero;

                        if (convertedVectors[0] == Vector3.up)
                        {
                            scaledAxis = Vector3.right;
                        }
                        else if (convertedVectors[1] == Vector3.up)
                        {
                            scaledAxis = Vector3.up;
                        }
                        else if (convertedVectors[2] == Vector3.up)
                        {
                            scaledAxis = Vector3.forward;
                        }

                        // Calculate the scale addition for the block
                        Vector3 scaleAddition = Vector3.Scale((bp.transform.localScale * amount - bp.transform.localScale), scaledAxis);
                        bp.transform.localScale += scaleAddition;
                    }
                }

                if (axis.z > 0)
                {
                    // Scale on the world Z axis
                    foreach (BlockProperties bp in blockList)
                    {
                        // Calculate the new position of the block
                        Vector3 pos = bp.transform.position;
                        pos -= center;
                        pos.z *= amount;
                        pos += center;
                        bp.transform.position = pos;

                        // Convert local to world vectors for scaling
                        Vector3[] convertedVectors = BPXUtilities.ConvertLocalToWorldVectors(bp.transform);

                        // Determine the scaled axis for scaling the block
                        Vector3 scaledAxis = Vector3.zero;

                        if (convertedVectors[0] == Vector3.forward)
                        {
                            scaledAxis = Vector3.right;
                        }
                        else if (convertedVectors[1] == Vector3.forward)
                        {
                            scaledAxis = Vector3.up;
                        }
                        else if (convertedVectors[2] == Vector3.forward)
                        {
                            scaledAxis = Vector3.forward;
                        }

                        // Calculate the scale addition for the block
                        Vector3 scaleAddition = Vector3.Scale((bp.transform.localScale * amount - bp.transform.localScale), scaledAxis);
                        bp.transform.localScale += scaleAddition;
                    }
                }
            }

            // Convert the blockList to JSON after scaling
            List<string> after = BPXManager.central.undoRedo.ConvertBlockListToJSONList(blockList);
            List<string> selectionList = BPXManager.central.undoRedo.ConvertSelectionToStringList(blockList);

            // Break the lock and perform any necessary actions
            BPXManager.central.validation.BreakLock(BPXManager.central.undoRedo.ConvertBeforeAndAfterListToCollection(before, after, blockList, selectionList, selectionList), "Gizmo1");
        }

        public static void ScaleInPlace(List<BlockProperties> blockList, Vector3 axis, float amount)
        {
            // Check if BPXManager.central is null
            if (BPXManager.central == null) { return; }

            // Convert the blockList to JSON before scaling
            List<string> before = BPXManager.central.undoRedo.ConvertBlockListToJSONList(blockList);

            if (axis.x > 0 && axis.y > 0 && axis.z > 0)
            {
                // Scale uniformly
                foreach (BlockProperties bp in blockList)
                {
                    // Scale the block uniformly
                    bp.transform.localScale *= amount;
                }
            }
            else
            {
                if (axis.x > 0)
                {
                    // Scale on the world X axis
                    foreach (BlockProperties bp in blockList)
                    {
                        // Convert local to world vectors for scaling
                        Vector3[] convertedVectors = BPXUtilities.ConvertLocalToWorldVectors(bp.transform);

                        // Determine the scaled axis for scaling the block
                        Vector3 scaledAxis = Vector3.zero;

                        if (convertedVectors[0] == Vector3.right)
                        {
                            scaledAxis = Vector3.right;
                        }
                        else if (convertedVectors[1] == Vector3.right)
                        {
                            scaledAxis = Vector3.up;
                        }
                        else if (convertedVectors[2] == Vector3.right)
                        {
                            scaledAxis = Vector3.forward;
                        }

                        // Calculate the scale addition for the block
                        Vector3 scaleAddition = Vector3.Scale((bp.transform.localScale * amount - bp.transform.localScale), scaledAxis);
                        bp.transform.localScale += scaleAddition;
                    }
                }

                if (axis.y > 0)
                {
                    // Scale on the world Y axis
                    foreach (BlockProperties bp in blockList)
                    {
                        // Convert local to world vectors for scaling
                        Vector3[] convertedVectors = BPXUtilities.ConvertLocalToWorldVectors(bp.transform);

                        // Determine the scaled axis for scaling the block
                        Vector3 scaledAxis = Vector3.zero;

                        if (convertedVectors[0] == Vector3.up)
                        {
                            scaledAxis = Vector3.right;
                        }
                        else if (convertedVectors[1] == Vector3.up)
                        {
                            scaledAxis = Vector3.up;
                        }
                        else if (convertedVectors[2] == Vector3.up)
                        {
                            scaledAxis = Vector3.forward;
                        }

                        // Calculate the scale addition for the block
                        Vector3 scaleAddition = Vector3.Scale((bp.transform.localScale * amount - bp.transform.localScale), scaledAxis);
                        bp.transform.localScale += scaleAddition;
                    }
                }

                if (axis.z > 0)
                {
                    // Scale on the world Z axis
                    foreach (BlockProperties bp in blockList)
                    {
                        // Convert local to world vectors for scaling
                        Vector3[] convertedVectors = BPXUtilities.ConvertLocalToWorldVectors(bp.transform);

                        // Determine the scaled axis for scaling the block
                        Vector3 scaledAxis = Vector3.zero;

                        if (convertedVectors[0] == Vector3.forward)
                        {
                            scaledAxis = Vector3.right;
                        }
                        else if (convertedVectors[1] == Vector3.forward)
                        {
                            scaledAxis = Vector3.up;
                        }
                        else if (convertedVectors[2] == Vector3.forward)
                        {
                            scaledAxis = Vector3.forward;
                        }

                        // Calculate the scale addition for the block
                        Vector3 scaleAddition = Vector3.Scale((bp.transform.localScale * amount - bp.transform.localScale), scaledAxis);
                        bp.transform.localScale += scaleAddition;
                    }
                }
            }

            // Convert the blockList to JSON after scaling
            List<string> after = BPXManager.central.undoRedo.ConvertBlockListToJSONList(blockList);
            List<string> selectionList = BPXManager.central.undoRedo.ConvertSelectionToStringList(blockList);

            // Break the lock and perform any necessary actions
            BPXManager.central.validation.BreakLock(BPXManager.central.undoRedo.ConvertBeforeAndAfterListToCollection(before, after, blockList, selectionList, selectionList), "Gizmo1");
        }
        #endregion

        #region Moving
        public static void MoveSelection(Vector3 move)
        {
            // Check if BPXManager.central is null
            if (BPXManager.central == null) { return; }

            // Check if there are selected blocks
            if (BPXManager.central.selection.list.Count > 0)
            {
                // Get the camera's forward direction
                Vector3 camDirection = BPXManager.central.cam.cameraTransform.forward;

                // Calculate the move direction in world space
                Vector3 moveDirection = BPXUtilities.WorldSpaceRelativeMovement(camDirection, move);

                // Move the selected blocks
                Move(BPXManager.central.selection.list, moveDirection);
            }
        }

        public static void Move(List<BlockProperties> blockList, Vector3 move)
        {
            // Check if BPXManager.central is null
            if (BPXManager.central == null) { return; }

            // Convert the blockList to JSON before moving
            List<string> before = BPXManager.central.undoRedo.ConvertBlockListToJSONList(blockList);

            // Move each block by the specified amount
            foreach (BlockProperties bp in blockList)
            {
                bp.transform.position += move;
            }

            // Move the mother gizmo by the specified amount
            BPXManager.central.gizmos.motherGizmo.position += move;

            // Convert the blockList to JSON after moving
            List<string> after = BPXManager.central.undoRedo.ConvertBlockListToJSONList(blockList);
            List<string> selectionList = BPXManager.central.undoRedo.ConvertSelectionToStringList(blockList);

            // Break the lock and perform any necessary actions
            BPXManager.central.validation.BreakLock(BPXManager.central.undoRedo.ConvertBeforeAndAfterListToCollection(before, after, blockList, selectionList, selectionList), "Gizmo1");
        }
        #endregion

        #region Mirroring
        public static void MirrorSelection(Vector3 axis)
        {
            // Check if BPXManager.central is null
            if (BPXManager.central == null) { return; }

            // Check if there are selected blocks
            if (BPXManager.central.selection.list.Count > 0)
            {
                // Mirror the selected blocks
                Mirror(BPXManager.central.selection.list, axis);
            }
        }

        public static void Mirror(List<BlockProperties> blockList, Vector3 axis)
        {
            // Check if BPXManager.central is null
            if (BPXManager.central == null) { return; }

            // Convert the blockList to JSON before mirroring
            List<string> before = BPXManager.central.undoRedo.ConvertBlockListToJSONList(blockList);

            // Calculate the center position of the blockList
            Vector3 center = BPXUtilities.GetCenterPosition(blockList);

            // Create a temporary parent object for mirroring
            Transform tempParent = new GameObject("Temp Mirror Parent").transform;
            tempParent.position = center;

            foreach (BlockProperties bp in blockList)
            {
                // Set the temporary parent as the parent of each block
                bp.transform.parent = tempParent;
            }

            // Apply mirroring based on the specified axis
            if (axis.x > 0)
            {
                tempParent.transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (axis.y > 0)
            {
                tempParent.transform.localScale = new Vector3(1, -1, 1);
            }
            else if (axis.z > 0)
            {
                tempParent.transform.localScale = new Vector3(1, 1, -1);
            }

            foreach (BlockProperties bp in blockList)
            {
                // Remove the temporary parent by setting each block's parent to null
                bp.transform.parent = null;
            }

            // Destroy the temporary parent object
            GameObject.Destroy(tempParent.gameObject);

            // Convert the blockList to JSON after mirroring
            List<string> after = BPXManager.central.undoRedo.ConvertBlockListToJSONList(blockList);
            List<string> selectionList = BPXManager.central.undoRedo.ConvertSelectionToStringList(blockList);

            // Break the lock and perform any necessary actions
            BPXManager.central.validation.BreakLock(BPXManager.central.undoRedo.ConvertBeforeAndAfterListToCollection(before, after, blockList, selectionList, selectionList), "Gizmo1");
        }
        #endregion

        #region Clipboard
        public static void CopySelection()
        {
            // Check if BPXManager.central is null
            if (BPXManager.central == null) { return; }

            // Check if there are selected blocks
            if (BPXManager.central.selection.list.Count > 0)
            {
                // Create a blueprint from the selected blocks and store it in the copy buffer
                BPXManager.copyBuffer = BPXIO.CreateBlueprintFromSelection(BPXManager.central.selection.list);

                // Display a log message indicating successful copying
                PlayerManager.Instance.messenger.Log("Copied!", 3f);
            }
            else
            {
                // Display a log message indicating no selection to copy
                PlayerManager.Instance.messenger.Log("No Selection!", 3f);
            }
        }

        public static void PasteClipboard()
        {
            // Check if BPXManager.central is null
            if (BPXManager.central == null) { return; }

            // Check if the copy buffer is empty
            if (BPXManager.copyBuffer == null)
            {
                // Display a log message indicating an empty clipboard
                PlayerManager.Instance.messenger.Log("Clipboard Empty!", 3f);
            }
            else
            {
                Blueprint regen = BPXIO.RegenerateBlueprint(BPXManager.copyBuffer);

                // Load the blueprint from the copy buffer into the editor
                //BPXIO.LoadBlueprintIntoEditor(BPXManager.copyBuffer, true);
                BPXIO.LoadBlueprintIntoEditor(regen, true);
            }
        }        
        #endregion

        #region Travel
        public static void FastTravel()
        {
            // Check if BPXManager.central is null
            if (BPXManager.central == null) { return; }

            // Check if there are selected blocks
            if (BPXManager.central.selection.list.Count > 0)
            {
                // Move the camera's position to the mother gizmo's position
                BPXManager.central.cam.transform.position = BPXManager.central.gizmos.motherGizmo.transform.position;
            }
            else
            {
                // Display a log message indicating no selection to travel to
                PlayerManager.Instance.messenger.Log("No Selection To Travel To!", 3f);
            }
        }
        #endregion

        #region Cycling
        //Cycle the gizmo axis
        public static void CycleAxis(bool forward)
        {
            int currentIndex = BPXGizmoManager.chosenGizmoSelectionCycle.ToList().IndexOf(BPXGizmoManager.currentGizmoSelection);

            if (forward)
            {
                if (currentIndex == BPXGizmoManager.chosenGizmoSelectionCycle.Length - 1)
                {
                    BPXGizmoManager.SetGizmoSelection(BPXGizmoManager.chosenGizmoSelectionCycle[0]);
                }
                else
                {
                    BPXGizmoManager.SetGizmoSelection(BPXGizmoManager.chosenGizmoSelectionCycle[currentIndex + 1]);
                }
            }
            else
            {
                if (currentIndex == 0)
                {
                    BPXGizmoManager.SetGizmoSelection(BPXGizmoManager.chosenGizmoSelectionCycle[BPXGizmoManager.chosenGizmoSelectionCycle.Length - 1]);
                }
                else
                {
                    BPXGizmoManager.SetGizmoSelection(BPXGizmoManager.chosenGizmoSelectionCycle[currentIndex - 1]);
                }
            }
        }
        #endregion
    }
}

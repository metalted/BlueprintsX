using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BlueprintsX
{
    public class Blueprint
    {
        public string title = "New Blueprint";
        public string creator = "Bouwerman";
        public string path;
        public List<BlockPropertyJSON> blocks;

        public Blueprint()
        {
            blocks = new List<BlockPropertyJSON>();
        }
    }

    //This class will be responsible for reading and writing files and blueprints, spawning objects etc.
    public static class BPXIO
    {
        public static Blueprint ReadBlueprintFromFile(string path)
        {
            //Return null if the file doesnt exists.
            if (!File.Exists(path))
            {
                return null;
            }

            string[] file = new string[0];

            //Try to read the lines of the file. If an error occurs return null.
            try
            {
                file = File.ReadAllLines(path);
            }
            catch
            {
                return null;
            }

            //Get the info from the first line
            Blueprint blueprint = new Blueprint();
            blueprint.title = Path.GetFileNameWithoutExtension(path);
            blueprint.creator = file[0].Split(",")[1];
            blueprint.path = path;
            blueprint.blocks = new List<BlockPropertyJSON>();

            for (int i = 3; i < file.Length; i++)
            {
                //Skip empty lines.
                if (file[i].Trim() == "")
                {
                    continue;
                }

                //Split the data and skip if not the right length.
                string[] lineData = file[i].Split(",");

                if (lineData.Length != 38)
                {
                    continue;
                }

                try
                {
                    //Initialize
                    BlockPropertyJSON blockJSON = new BlockPropertyJSON();
                    blockJSON.properties = new List<float>();
                    //Block ID
                    blockJSON.blockID = int.Parse(lineData[0], CultureInfo.InvariantCulture);
                    //Position
                    blockJSON.position = new Vector3(
                        float.Parse(lineData[1], CultureInfo.InvariantCulture),
                        float.Parse(lineData[2], CultureInfo.InvariantCulture),
                        float.Parse(lineData[3], CultureInfo.InvariantCulture)
                    );
                    //Euler
                    blockJSON.eulerAngles = new Vector3(
                        float.Parse(lineData[4], CultureInfo.InvariantCulture),
                        float.Parse(lineData[5], CultureInfo.InvariantCulture),
                        float.Parse(lineData[6], CultureInfo.InvariantCulture)
                    );
                    //Scale
                    blockJSON.localScale = new Vector3(
                        float.Parse(lineData[7], CultureInfo.InvariantCulture),
                        float.Parse(lineData[8], CultureInfo.InvariantCulture),
                        float.Parse(lineData[9], CultureInfo.InvariantCulture)
                    );
                    //Properties. All expect block ID.
                    for (int j = 1; j < lineData.Length; j++)
                    {
                        blockJSON.properties.Add(float.Parse(lineData[j], CultureInfo.InvariantCulture));
                    }

                    //Add the block to the blueprint
                    blueprint.blocks.Add(blockJSON);
                }
                catch
                {
                    //There was a problem with this block, skipping.
                    Debug.LogError("Error occured while parsing line " + i + ".");
                }
            }

            Debug.Log("Read blueprint from path: " + path);
            return blueprint;
        }

        public static Blueprint CreateBlueprintFromSelection(List<BlockProperties> blockProperties, string creator = "", string title = "")
        {
            if (blockProperties.Count == 0)
            {
                Debug.Log("No blocks in selection");
                return null;
            }

            Blueprint blueprint = new Blueprint();
            if (creator != "")
            {
                blueprint.creator = creator;
            }

            if (title != "")
            {
                blueprint.title = title;
            }

            for (int i = 0; i < blockProperties.Count; i++)
            {
                try
                {
                    BlockPropertyJSON block = blockProperties[i].ConvertBlockToJSON_v15();
                    blueprint.blocks.Add(block);
                }
                catch
                {
                    Debug.Log("Error in block");
                }
            }

            return blueprint;
        }

        public static void SaveBlueprintAtTargetedPath()
        {
            if (BPXManager.createdBlueprintFromEditor.blocks.Count == 0)
            {
                Debug.Log("Can't save empty blueprint!");
                return;
            }

            //Ready to go! Create a 12 digit random number for the UID.
            string randomNumber = "";
            for (int i = 0; i < 12; i++)
            {
                randomNumber += UnityEngine.Random.Range(0, 10).ToString();
            }

            //Create the complete UID.
            DateTime now = DateTime.Now;
            string UID = now.Day.ToString("00") + now.Month.ToString("00") + now.Year.ToString() + "-" + now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00") + now.Millisecond.ToString("000") + "-" + BPXManager.createdBlueprintFromEditor.creator + "-" + randomNumber + "-" + BPXManager.createdBlueprintFromEditor.blocks.Count;

            //Create the list to hold the file.
            List<string> fileLines = new List<string>();

            //Create the header.
            fileLines.Add($"LevelEditor2,{BPXManager.createdBlueprintFromEditor.creator},{UID}");
            fileLines.Add("0,0,0,0,0,0,0,0");
            fileLines.Add("invalid track,0,0,0,0,90");

            //Add the blocks to the file
            for (int i = 0; i < BPXManager.createdBlueprintFromEditor.blocks.Count; i++)
            {
                try
                {
                    fileLines.Add($"{BPXManager.createdBlueprintFromEditor.blocks[i].blockID.ToString()},{string.Join(",", BPXManager.createdBlueprintFromEditor.blocks[i].properties.Select(p => p.ToString(CultureInfo.InvariantCulture)))}");
                }
                catch
                {
                    Debug.LogWarning($"BPSystem.IO.Write(): An error occured while converting a block to a comma seperated string. Skipping... (block #{i})");
                }
            }

            //All lines are created, write the file.
            try
            {
                File.WriteAllLines(BPXManager.targetedSavePath, fileLines);
            }
            catch
            {
                Debug.LogError("BPSystem.IO.Write(): An error occured while writing to file.");
            }
        }

        public static void LoadBlueprintIntoEditor(Blueprint blueprint, bool buildHere)
        {
            List<BlockProperties> blockList = InstantiateBlueprintIntoEditor(blueprint, true);

            if(buildHere)
            {
                BlueprintBounds bounds = new BlueprintBounds(blockList);
                Vector3 cameraGridPosition = BPXUtilities.ClosestGridPosition(BPXManager.central.cam.transform.position);
                Vector3 blueprintGridPosition = BPXUtilities.ClosestGridPosition(bounds.bounds.center);
                Vector3 move = cameraGridPosition - blueprintGridPosition;
                BPXInput.Move(blockList, move);
            }
        }

        public static List<BlockProperties> InstantiateBlueprintIntoEditor(Blueprint blueprint, bool notifyUndo)
        {
            //Deselect all blocks first.
            if (notifyUndo)
            {
                BPXManager.central.selection.DeselectAllBlocks(true, nameof(BPXManager.central.selection.ClickNothing));
            }

            //Create a list of blocks before the creation. //Same count, but null.
            List<string> before = Enumerable.Repeat((string)null, blueprint.blocks.Count).ToList();
            //Create a list of selections before the creation, which is empty.
            List<string> beforeSelection = new List<string>();

            //Stores the JSON strings of the blocks after creation.
            List<string> after = new List<string>();
            //Stores the selection after creation.
            List<string> afterSelection = new List<string>();

            //Stores the BlockProperties objects of the created blocks.
            List<BlockProperties> blockList = new List<BlockProperties>();            

            for (int i = 0; i < blueprint.blocks.Count; i++)
            {
                BlockProperties blockProperties;
                int id = blueprint.blocks[i].blockID;

                //Check if valid block.
                if (id >= 0 && id < PlayerManager.Instance.loader.globalBlockList.blocks.Count)
                {
                    blockProperties = UnityEngine.Object.Instantiate<BlockProperties>(PlayerManager.Instance.loader.globalBlockList.blocks[id]);
                    blockProperties.gameObject.name = PlayerManager.Instance.loader.globalBlockList.blocks[id].gameObject.name;
                }
                else
                {
                    blockProperties = UnityEngine.Object.Instantiate<BlockProperties>(PlayerManager.Instance.loader.globalBlockList.errorBlock);
                    blockProperties.gameObject.name = "Error!" + id.ToString();
                }

                if (notifyUndo)
                {
                    blueprint.blocks[i].UID = PlayerManager.Instance.GenerateUniqueIDforBlocks(id.ToString());
                }

                blockProperties.CreateBlock();
                blockProperties.properties.Clear();
                blockProperties.isEditor = true;
                blockProperties.LoadProperties_v15(blueprint.blocks[i], true);

                //Add the new block to the list of blocks.
                blockList.Add(blockProperties);

                //Add a json representation to the after blocks list.
                after.Add(blockProperties.ConvertBlockToJSON_v15_string());
            }

            if (notifyUndo)
            {
                //Create a new selection list using the UIDs of the blocks.
                afterSelection = BPXManager.central.undoRedo.ConvertSelectionToStringList(blockList);

                //Convert all the before and after data into a Change_Collection.
                Change_Collection collection = BPXManager.central.undoRedo.ConvertBeforeAndAfterListToCollection(
                    before, after,
                    blockList,
                    beforeSelection, afterSelection);

                //Register the creation
                BPXManager.central.validation.BreakLock(collection, "Gizmo6");

                //Select all the created objects.
                BPXManager.central.selection.UndoRedoReselection(blockList);
            }

            return blockList;
        }
    }
}

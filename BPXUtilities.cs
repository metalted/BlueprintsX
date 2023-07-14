using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BlueprintsX
{
    public static class BPXUtilities
    {
        public static Sprite Base64ToSprite(string base64)
        {
            // Convert the base64 string to a byte array
            byte[] imageBytes = Convert.FromBase64String(base64);

            // Create a new texture and load the image bytes
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(imageBytes);

            // Create a new sprite using the texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            return sprite;
        }

        public static Sprite Texture2DToSprite(Texture2D tex)
        {
                Rect rect = new Rect(0, 0, tex.width, tex.height);
                Vector2 pivot = new Vector2(0.5f, 0.5f);
                Sprite sprite = Sprite.Create(tex, rect, pivot);
                return sprite;
        }

        public static Vector3 ClosestGridPosition(Vector3 position)
        {
            return new Vector3(Mathf.FloorToInt(position.x / 16f), Mathf.FloorToInt(position.y / 16f), Mathf.FloorToInt(position.z / 16f)) * 16f;
        }

        public static Vector3 WorldSpaceRelativeMovement(Vector3 lookDirection, Vector3 move)
        {
            Vector3 relativeMove = Vector3.zero;

            if (Mathf.Abs(lookDirection.x) <= Mathf.Abs(lookDirection.z))
            {
                if (Mathf.Sign(lookDirection.z) < 0)
                {
                    relativeMove.x = move.x * -1;
                    relativeMove.y = move.y;
                    relativeMove.z = move.z * -1;
                }
                else
                {
                    relativeMove = move;
                }
            }
            else
            {
                if (Mathf.Sign(lookDirection.x) < 0)
                {
                    relativeMove.x = move.z * -1;
                    relativeMove.y = move.y;
                    relativeMove.z = move.x;
                }
                else
                {
                    relativeMove.x = move.z;
                    relativeMove.y = move.y;
                    relativeMove.z = move.x * -1;
                }
            }
            return relativeMove;
        }

        public static Vector3 GetAbsoluteVector(Vector3 inputVector)
        {
            return new Vector3(Mathf.Abs(inputVector.x), Mathf.Abs(inputVector.y), Mathf.Abs(inputVector.z));
        }

        public static Vector3[] ConvertLocalToWorldVectors(Transform local)
        {
            Vector3[] xDirections = GetSortedDirections(local.right);
            Vector3[] yDirections = GetSortedDirections(local.up);
            Vector3[] zDirections = GetSortedDirections(local.forward);

            List<string> taken = new List<string>();

            Vector3 chosenXDirection = xDirections[0];
            Vector3 chosenYDirection = Vector3.zero;
            Vector3 chosenZDirection = Vector3.zero;

            taken.Add(GetAxisFromDirection(chosenXDirection));

            foreach(Vector3 vy in yDirections)
            {
                string axis = GetAxisFromDirection(vy);

                if(!taken.Contains(axis))
                {
                    chosenYDirection = vy;
                    taken.Add(axis);
                    break;
                }
            }

            foreach (Vector3 vz in zDirections)
            {
                string axis = GetAxisFromDirection(vz);

                if(!taken.Contains(axis))
                {
                    chosenZDirection = vz;
                    taken.Add(axis);
                    break;
                }
            }

            return new Vector3[] { GetAbsoluteVector(chosenXDirection), GetAbsoluteVector(chosenYDirection), GetAbsoluteVector(chosenZDirection) };
        }

        public static BlockPropertyJSON BPXConvertBlockToJSON_v15(BlockProperties bp)
        {
            BlockPropertyJSON blockPropertyJSON = new BlockPropertyJSON();
            blockPropertyJSON.position = bp.transform.position;
            blockPropertyJSON.eulerAngles = bp.transform.eulerAngles;
            blockPropertyJSON.localScale = bp.transform.localScale;
            blockPropertyJSON.properties = new List<float>(bp.properties);
            blockPropertyJSON.blockID = bp.blockID;
            return blockPropertyJSON;
        }

        public static BlockPropertyJSON DeepCopyBlockPropertyJSON(BlockPropertyJSON bpj)
        {
            BlockPropertyJSON blockPropertyJSON = new BlockPropertyJSON();
            blockPropertyJSON.position = bpj.position;
            blockPropertyJSON.eulerAngles = bpj.eulerAngles;
            blockPropertyJSON.localScale = bpj.localScale;
            blockPropertyJSON.properties = new List<float>(bpj.properties);
            blockPropertyJSON.blockID = bpj.blockID;
            return blockPropertyJSON;
        }

        public static string GetAxisFromDirection(Vector3 direction)
        {
            // Get the absolute values of the direction components
            float absX = Mathf.Abs(direction.x);
            float absY = Mathf.Abs(direction.y);
            float absZ = Mathf.Abs(direction.z);

            // Check which axis has the largest absolute value
            if (absX > absY && absX > absZ)
            {
                return "X";
            }
            else if (absY > absX && absY > absZ)
            {
                return "Y";
            }
            else
            {
                return "Z";
            }
        }

        public static Vector3[] GetSortedDirections(Vector3 inputVector)
        {
            // Convert input vector to world space
            Vector3 inputVectorWorld = inputVector.normalized;

            // Define the six directions
            Vector3[] directions = {
                Vector3.up,
                Vector3.down,
                Vector3.left,
                Vector3.right,
                Vector3.forward,
                Vector3.back
            };

            // Calculate the angles between input vector and the six directions
            float[] angles = new float[directions.Length];
            for (int i = 0; i < directions.Length; i++)
            {
                angles[i] = Mathf.RoundToInt(Vector3.Angle(inputVectorWorld, directions[i]));
            }

            // Sort the directions based on the angles
            System.Array.Sort(angles, directions);

            return directions;
        }

        public static Vector3 GetCenterPosition(List<BlockProperties> list)
        {
            Vector3 total = Vector3.zero;

            foreach (BlockProperties bp in list)
            {
                total += bp.gameObject.transform.position;
            }

            return new Vector3(total.x / list.Count, total.y / list.Count, total.z / list.Count);
        }
    }

    public class BlueprintBounds
    {
        public Bounds bounds;

        public BlueprintBounds(List<BlockProperties> blockList)
        {
            Vector3 minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (BlockProperties bp in blockList)
            {
                MeshRenderer[] renderers = bp.gameObject.GetComponentsInChildren<MeshRenderer>();

                foreach (MeshRenderer r in renderers)
                {
                    if (r != null)
                    {
                        Bounds b = r.bounds;
                        minBounds = Vector3.Min(minBounds, b.min);
                        maxBounds = Vector3.Max(maxBounds, b.max);
                    }
                }
            }

            bounds = new Bounds((minBounds + maxBounds) * 0.5f, maxBounds - minBounds);
        }
    }

    public class BlueprintDimensionData
    {
        public Bounds bounds;

        public BlueprintDimensionData(List<GameObject> objs)
        {
            Vector3 minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (GameObject o in objs)
            {
                MeshRenderer[] renderers = o.GetComponentsInChildren<MeshRenderer>();

                foreach (MeshRenderer r in renderers)
                {
                    if (r != null)
                    {
                        Bounds b = r.bounds;
                        minBounds = Vector3.Min(minBounds, b.min);
                        maxBounds = Vector3.Max(maxBounds, b.max);
                    }
                }
            }

            bounds = new Bounds((minBounds + maxBounds) * 0.5f, maxBounds - minBounds);
        }
    }

    public class GizmoValues
    {
        public float XZ;
        public float Y;
        public float R;
        public float S;
    }
}

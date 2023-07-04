using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BlueprintsX
{
	public static class BPXDrag
	{
		public static Dictionary<Vector3, BlockProperties> currentObjects = new Dictionary<Vector3, BlockProperties>();
		public static Vector2 dragStartPosition;
		public static bool isDragging = false;
		public static Rect area;
		public static List<string> beforeSelection;

		public static void LostFocus()
		{
			if (isDragging)
			{
				BPXManager.central.selection.DeselectAllBlocks(true, nameof(BPXManager.central.selection.ClickNothing));
				currentObjects.Clear();
				dragStartPosition = Vector3.zero;
				isDragging = false;
				area = new Rect();
			}
		}

		public static void DoGUI()
		{
			if (area != null)
			{
				BPXDragUtils.DrawScreenRect(area, new Color(1.0f, 0.568f, 0f, 0.2f));
				BPXDragUtils.DrawScreenRectBorder(area, 1, new Color(1.0f, 0.568f, 0f));
			}
		}

		public static void StartDrag()
        {
			currentObjects = GetAllBlocks();
			dragStartPosition = Input.mousePosition;
			isDragging = true;
			BPXManager.central.selection.DeselectAllBlocks(true, nameof(BPXManager.central.selection.ClickNothing));
			beforeSelection = BPXManager.central.undoRedo.ConvertSelectionToStringList(BPXManager.central.selection.list);
		}

		public static void StopDrag()
        {
			isDragging = false;
			area = new Rect();
			currentObjects.Clear();
			List<string> afterSelection = BPXManager.central.undoRedo.ConvertSelectionToStringList(BPXManager.central.selection.list);
			BPXManager.central.selection.RegisterManualSelectionBreakLock(beforeSelection, afterSelection);
		}

		public static void Run()
		{
			//Get the state of the enable key.
			bool enableKey = Input.GetKey(BPXConfig.enableKey);

			//If the middle mouse button is enabled for drag selection.
			if (BPXConfig.mmbDrag)
			{
				//If we use the enable key for mmb drag, check if we can proceed.
				if (BPXConfig.enableForMMBDrag ? (enableKey ? true : false) : true)
				{
					//If we press the middle mouse button
					if (Input.GetMouseButtonDown(2))
					{
						StartDrag();
					}
				}
			}

			//If the middle mouse button is enabled for drag selection.
			if (BPXConfig.keyDrag)
			{
				//If we use the enable key for mmb drag, check if we can proceed.
				if (BPXConfig.enableForKeyDrag ? (enableKey ? true : false) : true)
				{
					//If we press the drag key
					if (Input.GetKeyDown(BPXConfig.dragKey))
					{
						StartDrag();
					}
				}
			}

			//If we are currently in the state of dragging:
			if (isDragging)
			{
				area = BPXDragUtils.GetScreenRect(dragStartPosition, Input.mousePosition);

				foreach (KeyValuePair<Vector3, BlockProperties> bp in currentObjects)
				{
					if (area.Contains((Vector2)bp.Key))
					{
						if (!BPXManager.central.selection.list.Contains(bp.Value))
						{
							BPXManager.central.selection.AddThisBlock(bp.Value);
						}
					}
					else
					{
						if (BPXManager.central.selection.list.Contains(bp.Value))
						{
							int index = BPXManager.central.selection.list.IndexOf(bp.Value);
							BPXManager.central.selection.RemoveBlockAt(index, false, false);
						}
					}
				}
			}

			//If the middle mouse button is enabled for drag selection, its enabled for stopping it.
			if (BPXConfig.mmbDrag)
			{
				//If we release the middle mouse button
				if (Input.GetMouseButtonUp(2))
				{
					StopDrag();
				}
			}

			//If the drag key is enabled for drag selection, its enabled for stopping it.
			if(BPXConfig.keyDrag)
            {
				//If we release the drag key.
				if(Input.GetKeyUp(BPXConfig.dragKey))
                {
					StopDrag();
                }
            }
		}

		public static void Reset()
		{
			currentObjects.Clear();
			dragStartPosition = Vector3.zero;
			isDragging = false;
			area = new Rect();
		}

		private static Vector3 temp;
		private static int c;
		public static Dictionary<Vector3, BlockProperties> GetAllBlocks()
		{
			Dictionary<Vector3, BlockProperties> found = new Dictionary<Vector3, BlockProperties>();
			GameObject[] allObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
			foreach (GameObject g in allObjects)
			{
				BlockProperties bp = g.GetComponent<BlockProperties>();
				if (bp != null)
				{
					c++;
					temp = Camera.main.WorldToScreenPoint(g.transform.position);

					//Dont want objects behind the camera.
					if (temp.z < 0) { continue; }

					temp.y = Screen.height - temp.y;
					temp.z = c;
					found.Add(temp, bp);
				}
			}
			c = 0;
			return found;
		}
	}

	public static class BPXDragUtils
	{
		static Texture2D _whiteTexture;
		public static Texture2D WhiteTexture
		{
			get
			{
				if (_whiteTexture == null)
				{
					_whiteTexture = new Texture2D(1, 1);
					_whiteTexture.SetPixel(0, 0, Color.white);
					_whiteTexture.Apply();
				}

				return _whiteTexture;
			}
		}

		public static void DrawScreenRect(Rect rect, Color color)
		{
			GUI.color = color;
			GUI.DrawTexture(rect, WhiteTexture);
			GUI.color = Color.white;
		}

		public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
		{
			// Top
			DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
			// Left
			DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
			// Right
			DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
			// Bottom
			DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
		}

		public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
		{
			// Move origin from bottom left to top left
			screenPosition1.y = Screen.height - screenPosition1.y;
			screenPosition2.y = Screen.height - screenPosition2.y;
			// Calculate corners
			var topLeft = Vector3.Min(screenPosition1, screenPosition2);
			var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
			// Create Rect
			return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
		}

		public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
		{
			var v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
			var v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
			var min = Vector3.Min(v1, v2);
			var max = Vector3.Max(v1, v2);
			min.z = camera.nearClipPlane;
			max.z = camera.farClipPlane;

			var bounds = new Bounds();
			bounds.SetMinMax(min, max);
			return bounds;
		}
	}
}

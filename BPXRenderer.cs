using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BlueprintsX
{
    public class BPXRenderCompleteArgs : EventArgs
    {
        public string renderTag;
        public List<Sprite> captures;
        public int currentCaptureIndex = 0;

        public BPXRenderCompleteArgs()
        {
            captures = new List<Sprite>();
        }
    }

    public class BPXRenderParameters
    {
        public Vector2Int size;
        public int imageCount = 1;

        public enum RenderStyle { Orthographic };
        public RenderStyle style = RenderStyle.Orthographic;
        public float horizontalAngle = 135f;
        public float horizontalAngleStep = 90f;
        public float verticalAngle = 45f;
        public string renderTag = "";

        public BPXRenderParameters(Vector2Int size, int imageCount = 1, RenderStyle style = RenderStyle.Orthographic, float horizontalAngle = 135f, float horizontalAngleStep = 90f, float verticalAngle = 45f, string renderTag = "")
        {
            this.size = size;
            this.style = style;
            this.imageCount = imageCount;
            this.horizontalAngle = horizontalAngle;
            this.horizontalAngleStep = horizontalAngleStep;
            this.verticalAngle = verticalAngle;
            this.renderTag = renderTag;
        }
    }


    //This class will be responsible for delivering renders of the blueprints.
    public static class BPXRenderer
    {
        public delegate void RenderCompleteHandler(object sender, BPXRenderCompleteArgs args);
        public static event RenderCompleteHandler OnRenderComplete;

        public static void GenerateThumbnail(Blueprint blueprint, BPXRenderParameters renderParameters)
        {
            BPXRenderBooth booth = InstantiateBlueprintRenderer(renderParameters.size);
            booth.transform.position = new Vector3(0, 20000, 0);

            //Capture the blueprint
            booth.StartCoroutine(booth.Render(blueprint, renderParameters,
                (args) => {

                    OnRenderComplete?.Invoke(null, args);

                    //Destroy the booth
                    GameObject.Destroy(booth.gameObject);
                }
            ));
        }

        public static BPXRenderBooth InstantiateBlueprintRenderer(Vector2Int renderSize)
        {
            BPXRenderBooth renderBooth = new GameObject("BlueprintRenderer").AddComponent<BPXRenderBooth>();
            renderBooth.t_cam = new GameObject("Camera Container").transform;
            renderBooth.t_cam.parent = renderBooth.transform;
            renderBooth.cam = renderBooth.t_cam.gameObject.AddComponent<Camera>();
            renderBooth.rt = new RenderTexture(renderSize.x, renderSize.y, 16, RenderTextureFormat.ARGB32);
            renderBooth.rt.Create();
            renderBooth.cam.targetTexture = renderBooth.rt;

            renderBooth.bg = GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
            renderBooth.bg.parent = renderBooth.t_cam;
            renderBooth.bg.localPosition = new Vector3(0, 0, 128);
            renderBooth.bg.transform.Rotate(-90, 0, 0);
            renderBooth.bgMat = new Material(Shader.Find("Unlit/Color"));
            renderBooth.bgMat.color = Color.black;
            renderBooth.bg.gameObject.GetComponent<UnityEngine.Renderer>().material = renderBooth.bgMat;
            renderBooth.bg.transform.localScale = new Vector3(50f,50f,50f);

            renderBooth.bpPivot = new GameObject("Blueprint Pivot").transform;
            renderBooth.bpPivot.parent = renderBooth.transform;
            renderBooth.bpPivot.localPosition = Vector3.zero;

            renderBooth.bpHolder = new GameObject("Blueprint Holder").transform;
            renderBooth.bpHolder.parent = renderBooth.bpPivot;
            renderBooth.bpHolder.localPosition = Vector3.zero;

            return renderBooth;
        }
    }

    public class BPXRenderBooth : MonoBehaviour
    {
        public Transform t_cam;
        public Camera cam;
        public RenderTexture rt;

        public Transform bgPivot;
        public Transform bg;
        public Material bgMat;

        public Transform bpPivot;
        public Transform bpHolder;

        public IEnumerator Render(Blueprint blueprint, BPXRenderParameters renderParameters, Action<BPXRenderCompleteArgs> callback)
        {
            //Instantiate the blueprint
            List<BlockProperties> blockList = BPXIO.InstantiateBlueprintIntoEditor(blueprint, false);

            List<GameObject> objsList = new List<GameObject>();
            foreach(BlockProperties bp in blockList)
            {
                GameObject o = bp.gameObject;
                GameObject.Destroy(o.GetComponent<BlockProperties>());
                objsList.Add(o);
                o.transform.parent = bpHolder;
            }

            //Get the bounds data for the blueprint
            BlueprintDimensionData dim = new BlueprintDimensionData(objsList);

            //Scale the blueprint down so it fits inside a 16x16x16 cube.
            float scaleFactor = 64f / dim.bounds.size.magnitude;
            bpHolder.transform.localScale = Vector3.one * scaleFactor;
            dim = new BlueprintDimensionData(objsList);

            //Calculate the translation needed to align to the center of the pivot.
            Vector3 move = bpPivot.position - dim.bounds.center;

            //Move the blueprint to the center of the pivot.
            bpHolder.position += move;

            yield return new WaitForEndOfFrame();

            BPXRenderCompleteArgs args = new BPXRenderCompleteArgs();
            args.renderTag = renderParameters.renderTag;

            if (renderParameters.style == BPXRenderParameters.RenderStyle.Orthographic)
            {
                cam.orthographic = true;
                cam.orthographicSize = dim.bounds.size.magnitude * 0.5f;

                float angleRads = Mathf.PI * renderParameters.verticalAngle / 180f;
                t_cam.position = new Vector3(bpPivot.transform.position.x, bpPivot.position.y + dim.bounds.size.magnitude * Mathf.Cos(angleRads), bpPivot.position.z + dim.bounds.size.magnitude * Mathf.Sin(angleRads));
                t_cam.LookAt(bpPivot);                
            }

            RenderTexture.active = rt;

            for (int i = 0; i < renderParameters.imageCount; i++)
            {
                bpPivot.rotation = Quaternion.Euler(0f, i * renderParameters.horizontalAngleStep + renderParameters.horizontalAngle, 0f);
                cam.Render();

                Texture2D capture = new Texture2D(rt.width, rt.height);
                capture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                capture.Apply();

                args.captures.Add(BPXUtilities.Texture2DToSprite(capture));
            }

            RenderTexture.active = null;
            callback(args);
        }
    }
}

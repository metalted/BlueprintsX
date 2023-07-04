using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BlueprintsX
{
    public enum GizmoSelection { All, X, Y, Z, XY, YZ, XZ };

    public class BPXGizmoManager
    {
        public static GizmoSelection[] standardGizmoSelectionCycle = new GizmoSelection[] { GizmoSelection.All, GizmoSelection.X, GizmoSelection.Y, GizmoSelection.Z };
        public static GizmoSelection[] extendedGizmoSelectionCycle = new GizmoSelection[] { GizmoSelection.All, GizmoSelection.X, GizmoSelection.Y, GizmoSelection.Z, GizmoSelection.XY, GizmoSelection.YZ, GizmoSelection.XZ };
        public static GizmoSelection[] chosenGizmoSelectionCycle = standardGizmoSelectionCycle;
        private static GizmoSelection lastGizmoSelection;
        public static GizmoSelection currentGizmoSelection;
        private static int lastQuadrant = 0;

        //Arrows can have 2 positions
        //Positive positions
        private static Vector3 arrow_x_positive_position = new Vector3(8, 0, 0);
        private static Vector3 arrow_x_positive_rotation = Vector3.zero;
        private static Vector3 arrow_y_positive_position = new Vector3(0, 8, 0);
        private static Vector3 arrow_y_positive_rotation = Vector3.zero;
        private static Vector3 arrow_z_positive_position = new Vector3(0, 0, 8);
        private static Vector3 arrow_z_positive_rotation = Vector3.zero;

        //Negative positions
        private static Vector3 arrow_x_negative_position = new Vector3(-8, 0, 0);
        private static Vector3 arrow_x_negative_rotation = new Vector3(0, 180, 0);
        private static Vector3 arrow_y_negative_position = new Vector3(0, -8, 0);
        private static Vector3 arrow_y_negative_rotation = new Vector3(180, 0, 0);
        private static Vector3 arrow_z_negative_position = new Vector3(0, 0, -8);
        private static Vector3 arrow_z_negative_rotation = new Vector3(0, 180, 0);

        //Scales
        private static Vector3 arrow_x_on_scale = Vector3.one * 10f;
        private static Vector3 arrow_x_off_scale = Vector3.one * 5f;
        private static Vector3 arrow_y_on_scale = Vector3.one * 10f;
        private static Vector3 arrow_y_off_scale = Vector3.one * 5f;
        private static Vector3 arrow_z_on_scale = Vector3.one * 10f;
        private static Vector3 arrow_z_off_scale = Vector3.one * 5f;

        //Planes can have 8 positions
        private static Vector3 plane_x_positive_y_positive_position_on = new Vector3(8, 8, 0);
        private static Vector3 plane_x_negative_y_positive_position_on = new Vector3(-8, 8, 0);
        private static Vector3 plane_x_positive_y_negative_position_on = new Vector3(8, -8, 0);
        private static Vector3 plane_x_negative_y_negative_position_on = new Vector3(-8, -8, 0);
        private static Vector3 plane_x_positive_y_positive_position_off = new Vector3(4, 4, 0);
        private static Vector3 plane_x_negative_y_positive_position_off = new Vector3(-4, 4, 0);
        private static Vector3 plane_x_positive_y_negative_position_off = new Vector3(4, -4, 0);
        private static Vector3 plane_x_negative_y_negative_position_off = new Vector3(-4, -4, 0);

        private static Vector3 plane_y_positive_z_positive_position_on = new Vector3(0, 8, 8);
        private static Vector3 plane_y_negative_z_positive_position_on = new Vector3(0, -8, 8);
        private static Vector3 plane_y_positive_z_negative_position_on = new Vector3(0, 8, -8);
        private static Vector3 plane_y_negative_z_negative_position_on = new Vector3(0, -8, -8);
        private static Vector3 plane_y_positive_z_positive_position_off = new Vector3(0, 4, 4);
        private static Vector3 plane_y_negative_z_positive_position_off = new Vector3(0, -4, 4);
        private static Vector3 plane_y_positive_z_negative_position_off = new Vector3(0, 4, -4);
        private static Vector3 plane_y_negative_z_negative_position_off = new Vector3(0, -4, -4);

        private static Vector3 plane_x_positive_z_positive_position_on = new Vector3(8, 0, 8);
        private static Vector3 plane_x_negative_z_positive_position_on = new Vector3(-8, 0, 8);
        private static Vector3 plane_x_positive_z_negative_position_on = new Vector3(8, 0, -8);
        private static Vector3 plane_x_negative_z_negative_position_on = new Vector3(-8, 0, -8);
        private static Vector3 plane_x_positive_z_positive_position_off = new Vector3(4, 0, 4);
        private static Vector3 plane_x_negative_z_positive_position_off = new Vector3(-4, 0, 4);
        private static Vector3 plane_x_positive_z_negative_position_off = new Vector3(4, 0, -4);
        private static Vector3 plane_x_negative_z_negative_position_off = new Vector3(-4, 0, -4);

        //Plane scales
        private static Vector3 plane_xy_on_scale = new Vector3(16, 16, 2);
        private static Vector3 plane_xy_off_scale = new Vector3(8, 8, 1);
        private static Vector3 plane_yz_on_scale = new Vector3(2, 16, 16);
        private static Vector3 plane_yz_off_scale = new Vector3(1, 8, 8);
        private static Vector3 plane_xz_on_scale = new Vector3(16, 2, 16);
        private static Vector3 plane_xz_off_scale = new Vector3(8, 1, 8);

        public static int GetCameraQuadrant()
        {
            int xVal = BPXManager.central.cam.cameraTransform.position.x >= BPXManager.central.gizmos.motherGizmo.position.x ? 1 : 0;
            int yVal = BPXManager.central.cam.cameraTransform.position.y >= BPXManager.central.gizmos.motherGizmo.position.y ? 2 : 0;
            int zVal = BPXManager.central.cam.cameraTransform.position.z >= BPXManager.central.gizmos.motherGizmo.position.z ? 4 : 0;

            return xVal + yVal + zVal;
        }

        public static void SetXArrow(int quadrant, bool active)
        {
            if(active)
            {
                BPXManager.central.gizmos.Xgizmo.transform.localScale = arrow_x_on_scale;
            }
            else
            {
                BPXManager.central.gizmos.Xgizmo.transform.localScale = arrow_x_off_scale;
            }

            switch(quadrant)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                    BPXManager.central.gizmos.Xgizmo.transform.localPosition = arrow_x_positive_position;
                    BPXManager.central.gizmos.Xgizmo.transform.localEulerAngles = arrow_x_positive_rotation;
                    break;
                case 0:
                case 2:
                case 4:
                case 6:
                    BPXManager.central.gizmos.Xgizmo.transform.localPosition = arrow_x_negative_position;
                    BPXManager.central.gizmos.Xgizmo.transform.localEulerAngles = arrow_x_negative_rotation;
                    break;
            }
        }

        public static void SetYArrow(int quadrant, bool active)
        {
            if (active)
            {
                BPXManager.central.gizmos.Ygizmo.transform.localScale = arrow_y_on_scale;
            }
            else
            {
                BPXManager.central.gizmos.Ygizmo.transform.localScale = arrow_y_off_scale;
            }

            switch (quadrant)
            {
                case 2:
                case 3:
                case 6:
                case 7:
                    BPXManager.central.gizmos.Ygizmo.transform.localPosition = arrow_y_positive_position;
                    BPXManager.central.gizmos.Ygizmo.transform.localEulerAngles = arrow_y_positive_rotation;
                    break;
                case 0:
                case 1:
                case 4:
                case 5:
                    BPXManager.central.gizmos.Ygizmo.transform.localPosition = arrow_y_negative_position;
                    BPXManager.central.gizmos.Ygizmo.transform.localEulerAngles = arrow_y_negative_rotation;
                    break;
            }
        }

        public static void SetZArrow(int quadrant, bool active)
        {
            if (active)
            {
                BPXManager.central.gizmos.Zgizmo.transform.localScale = arrow_z_on_scale;
            }
            else
            {
                BPXManager.central.gizmos.Zgizmo.transform.localScale = arrow_z_off_scale;
            }

            switch (quadrant)
            {
                case 4:
                case 5:
                case 6:
                case 7:
                    BPXManager.central.gizmos.Zgizmo.transform.localPosition = arrow_z_positive_position;
                    BPXManager.central.gizmos.Zgizmo.transform.localEulerAngles = arrow_z_positive_rotation;
                    break;
                case 0:
                case 1:
                case 2:
                case 3:
                    BPXManager.central.gizmos.Zgizmo.transform.localPosition = arrow_z_negative_position;
                    BPXManager.central.gizmos.Zgizmo.transform.localEulerAngles = arrow_z_negative_rotation;
                    break;
            }
        }

        public static void SetXYPlane(int quadrant, bool active)
        {
            if (active)
            {
                BPXManager.central.gizmos.XYgizmo.transform.localScale = plane_xy_on_scale;
                
            }
            else
            {
                BPXManager.central.gizmos.XYgizmo.transform.localScale = plane_xy_off_scale;
            }

            switch (quadrant)
            {
                case 3:
                case 7:
                    if(active)
                    {
                        BPXManager.central.gizmos.XYgizmo.transform.localPosition = plane_x_positive_y_positive_position_on;
                    }
                    else
                    {
                        BPXManager.central.gizmos.XYgizmo.transform.localPosition = plane_x_positive_y_positive_position_off;
                    }                    
                    break;
                case 2:
                case 6:
                    if (active)
                    {
                        BPXManager.central.gizmos.XYgizmo.transform.localPosition = plane_x_negative_y_positive_position_on;
                    }
                    else
                    {
                        BPXManager.central.gizmos.XYgizmo.transform.localPosition = plane_x_negative_y_positive_position_off;
                    }
                    break;
                case 1:
                case 5:
                    if (active)
                    {
                        BPXManager.central.gizmos.XYgizmo.transform.localPosition = plane_x_positive_y_negative_position_on;
                    }
                    else
                    {
                        BPXManager.central.gizmos.XYgizmo.transform.localPosition = plane_x_positive_y_negative_position_off;
                    }
                    break;
                case 0:
                case 4:
                    if (active)
                    {
                        BPXManager.central.gizmos.XYgizmo.transform.localPosition = plane_x_negative_y_negative_position_on;
                    }
                    else
                    {
                        BPXManager.central.gizmos.XYgizmo.transform.localPosition = plane_x_negative_y_negative_position_off;
                    }
                    break;
            }
        }

        public static void SetYZPlane(int quadrant, bool active)
        {
            if (active)
            {
                BPXManager.central.gizmos.YZgizmo.transform.localScale = plane_yz_on_scale;

            }
            else
            {
                BPXManager.central.gizmos.YZgizmo.transform.localScale = plane_yz_off_scale;
            }

            switch (quadrant)
            {
                case 6:
                case 7:
                    if (active)
                    {
                        BPXManager.central.gizmos.YZgizmo.transform.localPosition = plane_y_positive_z_positive_position_on;
                    }
                    else
                    {
                        BPXManager.central.gizmos.YZgizmo.transform.localPosition = plane_y_positive_z_positive_position_off;
                    }
                    break;
                case 4:
                case 5:
                    if (active)
                    {
                        BPXManager.central.gizmos.YZgizmo.transform.localPosition = plane_y_negative_z_positive_position_on;
                    }
                    else
                    {
                        BPXManager.central.gizmos.YZgizmo.transform.localPosition = plane_y_negative_z_positive_position_off;
                    }
                    break;
                case 2:
                case 3:
                    if (active)
                    {
                        BPXManager.central.gizmos.YZgizmo.transform.localPosition = plane_y_positive_z_negative_position_on;
                    }
                    else
                    {
                        BPXManager.central.gizmos.YZgizmo.transform.localPosition = plane_y_positive_z_negative_position_off;
                    }
                    break;
                case 0:
                case 1:
                    if (active)
                    {
                        BPXManager.central.gizmos.YZgizmo.transform.localPosition = plane_y_negative_z_negative_position_on;
                    }
                    else
                    {
                        BPXManager.central.gizmos.YZgizmo.transform.localPosition = plane_y_negative_z_negative_position_off;
                    }
                    break;
            }
        }

        public static void SetXZPlane(int quadrant, bool active)
        {
            if (active)
            {
                BPXManager.central.gizmos.XZgizmo.transform.localScale = plane_xz_on_scale;

            }
            else
            {
                BPXManager.central.gizmos.XZgizmo.transform.localScale = plane_xz_off_scale;
            }

            switch (quadrant)
            {
                case 5:
                case 7:
                    if (active)
                    {
                        BPXManager.central.gizmos.XZgizmo.transform.localPosition = plane_x_positive_z_positive_position_on;
                    }
                    else
                    {
                        BPXManager.central.gizmos.XZgizmo.transform.localPosition = plane_x_positive_z_positive_position_off;
                    }
                    break;
                case 4:
                case 6:
                    if (active)
                    {
                        BPXManager.central.gizmos.XZgizmo.transform.localPosition = plane_x_negative_z_positive_position_on;
                    }
                    else
                    {
                        BPXManager.central.gizmos.XZgizmo.transform.localPosition = plane_x_negative_z_positive_position_off;
                    }
                    break;
                case 1:
                case 3:
                    if (active)
                    {
                        BPXManager.central.gizmos.XZgizmo.transform.localPosition = plane_x_positive_z_negative_position_on;
                    }
                    else
                    {
                        BPXManager.central.gizmos.XZgizmo.transform.localPosition = plane_x_positive_z_negative_position_off;
                    }
                    break;
                case 0:
                case 2:
                    if (active)
                    {
                        BPXManager.central.gizmos.XZgizmo.transform.localPosition = plane_x_negative_z_negative_position_on;
                    }
                    else
                    {
                        BPXManager.central.gizmos.XZgizmo.transform.localPosition = plane_x_negative_z_negative_position_off;
                    }
                    break;
            }
        }

        public static void UpdateGizmo()
        {
            if(BPXManager.central == null){ return; }

            int quadrant = GetCameraQuadrant();

            if(currentGizmoSelection == lastGizmoSelection && quadrant == lastQuadrant)
            {
                return;
            }

            lastQuadrant = quadrant;
            lastGizmoSelection = currentGizmoSelection;

            switch (currentGizmoSelection)
            {
                case GizmoSelection.All:
                    SetXArrow(quadrant, false);
                    SetYArrow(quadrant, false);
                    SetZArrow(quadrant, false);
                    SetXYPlane(quadrant, false);
                    SetYZPlane(quadrant, false);
                    SetXZPlane(quadrant, false);
                    break;
                case GizmoSelection.X:
                    SetXArrow(quadrant, true);
                    SetYArrow(quadrant, false);
                    SetZArrow(quadrant, false);
                    SetXYPlane(quadrant, false);
                    SetYZPlane(quadrant, false);
                    SetXZPlane(quadrant, false);
                    break;
                case GizmoSelection.Y:
                    SetXArrow(quadrant, false);
                    SetYArrow(quadrant, true);
                    SetZArrow(quadrant, false);
                    SetXYPlane(quadrant, false);
                    SetYZPlane(quadrant, false);
                    SetXZPlane(quadrant, false);
                    break;
                case GizmoSelection.Z:
                    SetXArrow(quadrant, false);
                    SetYArrow(quadrant, false);
                    SetZArrow(quadrant, true);
                    SetXYPlane(quadrant, false);
                    SetYZPlane(quadrant, false);
                    SetXZPlane(quadrant, false);
                    break;
                case GizmoSelection.XY:
                    SetXArrow(quadrant, true);
                    SetYArrow(quadrant, true);
                    SetZArrow(quadrant, false);
                    SetXYPlane(quadrant, true);
                    SetYZPlane(quadrant, false);
                    SetXZPlane(quadrant, false);
                    break;
                case GizmoSelection.YZ:
                    SetXArrow(quadrant, false);
                    SetYArrow(quadrant, true);
                    SetZArrow(quadrant, true);
                    SetXYPlane(quadrant, false);
                    SetYZPlane(quadrant, true);
                    SetXZPlane(quadrant, false);
                    break;
                case GizmoSelection.XZ:
                    SetXArrow(quadrant, true);
                    SetYArrow(quadrant, false);
                    SetZArrow(quadrant, true);
                    SetXYPlane(quadrant, false);
                    SetYZPlane(quadrant, false);
                    SetXZPlane(quadrant, true);
                    break;
            }
        }

        public static void SetGizmoSelection(GizmoSelection selection)
        {
            currentGizmoSelection = selection;
        }

        public static void ResetGizmoSelection()
        {
            currentGizmoSelection = GizmoSelection.All;
        }

        public static Vector3 GetCurrentAxis()
        {
            switch (currentGizmoSelection)
            {
                default:
                case GizmoSelection.All:
                    return Vector3.one;
                case GizmoSelection.X:
                    return Vector3.right;
                case GizmoSelection.Y:
                    return Vector3.up;
                case GizmoSelection.Z:
                    return Vector3.forward;
                case GizmoSelection.XY:
                    return Vector3.right + Vector3.up;
                case GizmoSelection.YZ:
                    return Vector3.up + Vector3.forward;
                case GizmoSelection.XZ:
                    return Vector3.right + Vector3.forward;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(GuardsFOV))]
public class GuardsFOVEditor : Editor {

    private void OnSceneGUI()
    {
        GuardsFOV fov = (GuardsFOV)target;

        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.forward, Vector3.up, 360, fov.viewRadius);

        Vector3 viewAngleA = fov.DirFromAngle(-fov.viewAngle / 2, false);
        Vector3 viewAngleB = fov.DirFromAngle(fov.viewAngle / 2, false);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);

        Handles.color = Color.red;
        foreach(Transform visibleTarget in fov.visibleKnockedOutGuards)
        {
            Handles.DrawLine(fov.transform.parent.position, visibleTarget.position);
        }
    }
}

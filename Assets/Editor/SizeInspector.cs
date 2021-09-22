using System.Collections;
using UnityEngine;
using UnityEditor;

public class SizeInspector : EditorWindow {
    public double meshSizeX = 0.0;
    public double meshSizeY = 0.0;
    public double meshSizeZ = 0.0;
    public double effectiveSizeX = 0.0;
    public double effectiveSizeY = 0.0;
    public double effectiveSizeZ = 0.0;

    [MenuItem ("Window/Size Inspector")]

    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(SizeInspector));
    }

    void OnGUI() {
        GUIStyle fixedWidgetLabelStyle = new GUIStyle(GUI.skin.label) {
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,
            fixedWidth = 90,
            fontStyle = FontStyle.Bold
        };

        GUIStyle shortFixedWidgetLabelStyle = new GUIStyle(GUI.skin.label) {
            alignment = TextAnchor.MiddleRight,
            clipping = TextClipping.Clip,
            fixedWidth = 15
        };

        GUIStyle fixedWidthTextFieldStyle = new GUIStyle(GUI.skin.textField) {
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,
            fixedWidth = 70
        };

        GUILayout.BeginHorizontal();
        GUILayout.Label("Mesh Size", fixedWidgetLabelStyle);
        GUILayout.Label("X:", shortFixedWidgetLabelStyle);
        meshSizeX = EditorGUILayout.DoubleField(meshSizeX, fixedWidthTextFieldStyle);
        GUILayout.Label("Y:", shortFixedWidgetLabelStyle);
        meshSizeY = EditorGUILayout.DoubleField(meshSizeY, fixedWidthTextFieldStyle);
        GUILayout.Label("Z:", shortFixedWidgetLabelStyle);
        meshSizeZ = EditorGUILayout.DoubleField(meshSizeZ, fixedWidthTextFieldStyle);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Effective Size", fixedWidgetLabelStyle);
        GUILayout.Label("X:", shortFixedWidgetLabelStyle);
        effectiveSizeX = EditorGUILayout.DoubleField(effectiveSizeX, fixedWidthTextFieldStyle);
        GUILayout.Label("Y:", shortFixedWidgetLabelStyle);
        effectiveSizeY = EditorGUILayout.DoubleField(effectiveSizeY, fixedWidthTextFieldStyle);
        GUILayout.Label("Z:", shortFixedWidgetLabelStyle);
        effectiveSizeZ = EditorGUILayout.DoubleField(effectiveSizeZ, fixedWidthTextFieldStyle);
        GUILayout.EndHorizontal();

        if(Selection.activeTransform != null) {
            GameObject selectedObject = Selection.activeTransform.gameObject;

            if (selectedObject.TryGetComponent(typeof(MeshFilter), out Component meshComponent))
            {
                MeshFilter meshFilter = meshComponent as MeshFilter;
                Mesh mesh = meshFilter.sharedMesh;
                Bounds bounds = mesh.bounds;

                meshSizeX = bounds.size.x;
                meshSizeY = bounds.size.y;
                meshSizeZ = bounds.size.z;
            }

            if (selectedObject.TryGetComponent(typeof(Transform), out Component transformComponent))
            {
                Transform transform = transformComponent as Transform;
                effectiveSizeX = meshSizeX * transform.lossyScale.x;
                effectiveSizeY = meshSizeY * transform.lossyScale.y;
                effectiveSizeZ = meshSizeZ * transform.lossyScale.z;
            }
        }
        Repaint();
    }
}

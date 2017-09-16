using UnityEngine;
using UnityEditor;

namespace uHomography
{

[CustomEditor(typeof(Homography))]
public class HomographyEditor : Editor
{
    Homography homography
    {
        get { return target as Homography; }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Remove", GUILayout.Width(120)))
            {
                DestroyImmediate(homography.camera.gameObject);
                DestroyImmediate(homography);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }
}

}
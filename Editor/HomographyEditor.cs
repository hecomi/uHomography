using UnityEngine;
using UnityEditor;

namespace uHomography
{

[CustomEditor(typeof(Homography))]
public class HomographyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}

}
using UnityEngine;
using UnityEditor;

public class SceneViewParent
{
    /// <summary>
    /// Parents selected objects to the active object when SceneView is the active view.
    /// </summary>
    [MenuItem("Tools/Parent %f")]
    static void SetSceneViewParent()
    {
        Transform[] _transforms = Selection.GetTransforms(SelectionMode.TopLevel);
        Transform _parentTransform = Selection.activeTransform;

        if (SceneView.sceneViews.Contains(EditorWindow.focusedWindow))
        {
            if (Selection.objects.Length < 2)
            {
                EditorUtility.DisplayDialog("Alert", "Select at least two objects in SceneView!", "OK");
                //Debug.Log("Select at least two");
            }
            else
            {
                foreach (Transform t in _transforms)
                {
                    Undo.SetTransformParent(t, _parentTransform, "Parented object");
                    t.SetParent(_parentTransform);
                }
            }
        }

    }

}

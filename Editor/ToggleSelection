using UnityEditor;
using UnityEngine;

public class ToggleSelection
{
    /// <summary>
    /// Toggles selection between all and none.
    /// </summary>
    [MenuItem("Edit/Toggle Selection %#a")]
    static void SelectionToggle()
    {
        if(Selection.objects.Length > 0)
        {
            Selection.activeObject = null;
        }
        else
        {
            EditorApplication.ExecuteMenuItem("Edit/Select All");
        }
    }
}

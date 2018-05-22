using UnityEngine;
using UnityEditor;

public class SnapshotMesh : EditorWindow
{
    [MenuItem("Tools/Snapshot Mesh")]
    public static void ShowWindow()
    {
        GetWindow<SnapshotMesh>("Snapshot Mesh");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Utility for taking snapshots of skinned meshes.");
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Take Snapshot", GUILayout.Width(Screen.width / 2), GUILayout.Height(40)))
        {

            if(Selection.transforms.Length != 0)
            {
                for(int i = 0; i < Selection.transforms.Length; i++)
                {
                    if (Selection.gameObjects[i].GetComponent<SkinnedMeshRenderer>())
                    {
                        SkinnedMeshRenderer _sk;
                        GameObject _clone;
                        Mesh _snapshotMesh = new Mesh();
                        _sk = Selection.gameObjects[i].GetComponent<SkinnedMeshRenderer>();
                        _sk.BakeMesh(_snapshotMesh);

                        _clone = Instantiate(Selection.gameObjects[i], null);
                        _clone.transform.position = _sk.GetComponent<Transform>().position;
                        _clone.transform.rotation = _sk.GetComponent<Transform>().rotation;
                        _clone.transform.localScale = _sk.GetComponent<Transform>().localScale;

                        SkinnedMeshRenderer _destroyThis = _clone.GetComponent<SkinnedMeshRenderer>();
                        DestroyImmediate(_destroyThis);
                        _clone.AddComponent<MeshFilter>();
                        _clone.AddComponent<MeshRenderer>();
                        _clone.GetComponent<MeshFilter>().sharedMesh = _snapshotMesh;
                        _clone.GetComponent<MeshRenderer>().sharedMaterial = _sk.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
                        _clone.name = _clone.name.ToString().Replace("(Clone)", "_SnapShot");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Selection", "Selection needs to have at least one GameObject with Skinned Mesh Renderer component!", "OK");
                    }
                }
            }
 
            else
            {
                EditorUtility.DisplayDialog("Invalid Selection", "Nothing selected!\n\nSelect GameObject(s) with a Skinned Mesh Renderer component.", "OK");
            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}
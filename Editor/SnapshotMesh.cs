using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

public class SnapshotMesh : EditorWindow
{

    private bool _keepRig;
    private bool _preferenceFoldout;
    // private bool _keepMesh;
    // private bool _keepAnimator;

    private void Awake()
    {
        _keepRig = false;
    }

    [MenuItem("Tools/Snapshot Mesh")]
    public static void ShowWindow()
    {
        GetWindow<SnapshotMesh>("Snapshot Mesh");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Utility for taking snapshots of skinned meshes.");
        EditorGUILayout.Space();

        EditorGUIUtility.labelWidth = Screen.width / 3.5f;
        _keepRig = EditorGUILayout.Toggle("Keep rig:", _keepRig);
        EditorGUIUtility.labelWidth = 0;

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Take Snapshot", GUILayout.Width(Screen.width / 2), GUILayout.Height(40)))
        {

            if(Selection.transforms.Length != 0)
            {
                for(int i = 0; i < Selection.transforms.Length; i++)
                {
                    if (Selection.activeGameObject.GetComponentsInChildren<SkinnedMeshRenderer>() != null)
                    {
                        if (!_keepRig)
                        {
                            CaptureMesh(i);
                        }
                        else
                        {
                            CloneWithRig(i);
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Selection", "Selection needs to have at least one GameObject with SkinnedMeshRenderer component!", "OK");
                    }
                }
            }
 
            else
            {
                EditorUtility.DisplayDialog("Invalid Selection", "Nothing selected!\n\nSelect GameObject(s) with a SkinnedMeshRenderer component.", "OK");
            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //These are not yet implemented!
        // _preferenceFoldout = EditorGUILayout.Foldout(_preferenceFoldout, "Preferences:");
        // if (_preferenceFoldout)
        // {
        //     _keepMesh = EditorGUILayout.Toggle("Save created mesh:", _keepMesh);

        //     EditorGUILayout.LabelField("Save mesh to project folder:");
        //     EditorGUILayout.BeginHorizontal();
        //     EditorGUIUtility.labelWidth = 0.1f;
        //     GUI.enabled = false;
        //     EditorGUILayout.LabelField("Assets/");
        //     EditorGUIUtility.labelWidth = 0f;
        //     GUI.enabled = true;
        //     EditorGUILayout.TextField("");
        //     EditorGUILayout.EndHorizontal();

        //     _keepAnimator = EditorGUILayout.Toggle("Save created animator:", _keepAnimator);

        //     EditorGUILayout.LabelField("Save animator to project folder:");
        //     EditorGUILayout.BeginHorizontal();
        //     EditorGUIUtility.labelWidth = 0.1f;
        //     GUI.enabled = false;
        //     EditorGUILayout.LabelField("Assets/");
        //     EditorGUIUtility.labelWidth = 0f;
        //     GUI.enabled = true;
        //     EditorGUILayout.TextField("");
        //     EditorGUILayout.EndHorizontal();

        // }
    }

    private void CaptureMesh(int i)
    {
        if (Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>() != null)
        {
            SkinnedMeshRenderer sk;
            Mesh snapshotMesh = new Mesh();

            sk = Selection.gameObjects[i].GetComponent<SkinnedMeshRenderer>();
            sk.BakeMesh(snapshotMesh);

            string fileName = ConstructName();

            GameObject _newSnapshot = new GameObject(fileName);
            _newSnapshot.transform.position = Selection.gameObjects[i].GetComponent<Transform>().position;
            _newSnapshot.transform.rotation = Selection.gameObjects[i].GetComponent<Transform>().rotation;
            _newSnapshot.transform.localScale = Selection.gameObjects[i].GetComponent<Transform>().localScale;

            _newSnapshot.AddComponent<MeshFilter>();
            _newSnapshot.AddComponent<MeshRenderer>();
            _newSnapshot.GetComponent<MeshFilter>().sharedMesh = snapshotMesh;
            _newSnapshot.GetComponent<MeshRenderer>().sharedMaterial = sk.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
            AssetDatabase.CreateAsset(snapshotMesh, "Assets/"+fileName+".asset");
        }
        else
        {
            EditorUtility.DisplayDialog("Invalid Selection", "Selection needs to have at least one GameObject with SkinnedMeshRenderer component!", "OK");
        }

    }

    private void CloneWithRig(int i)
    {
        Mesh snapshotMesh = new Mesh();
        GameObject clone;
        clone = Instantiate(Selection.transforms[i].gameObject, null);

        Component[] components = clone.GetComponentsInChildren<Component>(true);
        
        for (int j = 0; j < components.Length; j++)
        {
            ArrayList excludedComponents = new ArrayList() { typeof(Transform), typeof(SkinnedMeshRenderer), typeof(MeshFilter), typeof(MeshRenderer) };
        
            if (!excludedComponents.Contains(components[j].GetType()))
            {
                DestroyImmediate(components[j]);
            }
        }
        string fileName = ConstructName();
        clone.name = clone.name.ToString().Replace("(Clone)", fileName);

    }

    private Animator CreateAnimator()
    {
        return new Animator();
    }

    private string ConstructName()
    {
        string dateString;
        DateTime dateTime = DateTime.Now;
        dateString = dateTime.ToString("yyMMddHHmmss");
        string fileName = "Snapshot_" + dateString;

        return fileName;
    }
}

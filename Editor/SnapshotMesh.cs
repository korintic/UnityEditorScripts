using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;
using System;
using System.IO;

public class SnapshotMesh : EditorWindow
{

    private bool _keepRig;
    private bool _preferencesFoldout;
    private bool _keepMesh;
    private bool _createAnimator;
    private bool _captureHierarchy;
    private bool _duplicateMaterials;
    private string _materialDestination = "";
    private string _animatorDestination = "";
    private string _meshDestination = "";

    private void Awake()
    {
        _keepRig = false;
        _keepMesh = true;
        _captureHierarchy = true;
        _duplicateMaterials = false;
        _createAnimator = false;
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

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Take Snapshot", GUILayout.Width(Screen.width / 2), GUILayout.Height(40)))
        {
            if(Selection.transforms.Length != 0)
            {

                for (int i = 0; i < Selection.transforms.Length; i++)
                {
                    if (Selection.transforms[i].GetComponentsInChildren<SkinnedMeshRenderer>() != null)
                    {
                        if (!_keepRig)
                        {
                            if (_captureHierarchy)
                            {
                                SkinnedMeshRenderer[] skArray = Selection.transforms[i].GetComponentsInChildren<SkinnedMeshRenderer>();

                                for (int j = 0; j < skArray.Length; j++)
                                {
                                    GameObject selectedObject;
                                    selectedObject = skArray[j].gameObject;

                                    CaptureMesh(selectedObject, _keepMesh, _duplicateMaterials);
                                }
                            }
                            else
                            {
                                CaptureMesh(Selection.activeTransform.gameObject, _keepMesh, _duplicateMaterials);
                            }
                        }
                        else
                        {
                            GameObject selectedObject = Selection.transforms[i].gameObject;
                            CloneWithRig(selectedObject, _createAnimator);
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

        _preferencesFoldout = EditorGUILayout.Foldout(_preferencesFoldout, "Preferences:");
        if (_preferencesFoldout)
        {
            _keepRig = EditorGUILayout.Toggle("Keep rig:", _keepRig);
            _captureHierarchy = EditorGUILayout.Toggle("Capture hierarchy:", _captureHierarchy);

            EditorGUILayout.Space();
            
            _keepMesh = EditorGUILayout.Toggle("Save created meshes:", _keepMesh);
            EditorGUILayout.LabelField("Destination:");
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 0.1f;
            GUI.enabled = false;
            EditorGUILayout.LabelField("Assets/");
            EditorGUIUtility.labelWidth = 0f;
            GUI.enabled = true;
            _meshDestination = EditorGUILayout.TextField(_meshDestination);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            _duplicateMaterials = EditorGUILayout.Toggle("Duplicate materials:", _duplicateMaterials);
            EditorGUILayout.LabelField("Destination:");
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 0.1f;
            GUI.enabled = false;
            EditorGUILayout.LabelField("Assets/");
            EditorGUIUtility.labelWidth = 0f;
            GUI.enabled = true;
            _materialDestination = EditorGUILayout.TextField(_materialDestination);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            _createAnimator = EditorGUILayout.Toggle("Create animator:", _createAnimator);
            EditorGUILayout.LabelField("Destination:");
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 0.1f;
            GUI.enabled = false;
            EditorGUILayout.LabelField("Assets/");
            EditorGUIUtility.labelWidth = 0f;
            GUI.enabled = true;
            _animatorDestination = EditorGUILayout.TextField(_animatorDestination);
            EditorGUILayout.EndHorizontal();

        }
    }

    private void CaptureMesh(GameObject selectedObject, bool keepMesh, bool duplicateMaterials)
    {
        SkinnedMeshRenderer sk;
        Mesh snapshotMesh = new Mesh();

        if (selectedObject.GetComponent<SkinnedMeshRenderer>() != null)
        {
            sk = selectedObject.GetComponent<SkinnedMeshRenderer>();
        }
        else
        {
            return;
        }

        sk.BakeMesh(snapshotMesh);
        string newName = ConstructName(sk.name);
        
        GameObject _newSnapshot = new GameObject(newName);
        _newSnapshot.transform.position = sk.GetComponent<Transform>().position;
        _newSnapshot.transform.rotation = sk.GetComponent<Transform>().rotation;
        _newSnapshot.transform.localScale = sk.GetComponent<Transform>().localScale;
        
        _newSnapshot.AddComponent<MeshFilter>();
        _newSnapshot.AddComponent<MeshRenderer>();
        _newSnapshot.GetComponent<MeshFilter>().sharedMesh = snapshotMesh;
        _newSnapshot.GetComponent<MeshRenderer>().sharedMaterials = sk.GetComponent<SkinnedMeshRenderer>().sharedMaterials;

        if (duplicateMaterials)
        {
            Material[] newMaterials = DuplicateMaterials(selectedObject);
            _newSnapshot.GetComponent<MeshRenderer>().sharedMaterials = newMaterials;
            for (int i = 0; i < _newSnapshot.GetComponent<MeshRenderer>().sharedMaterials.Length; i++)
            {
                Material currentMaterial = _newSnapshot.GetComponent<MeshRenderer>().sharedMaterials[i];
                SaveAsset(currentMaterial, currentMaterial.name, _materialDestination);
            }
        }

        if (keepMesh)
        {
            SaveAsset(snapshotMesh, newName, _meshDestination);
        }
       
    }

    private void CloneWithRig(GameObject selectedObject, bool createAnimator)
    {
        GameObject clone;
        clone = Instantiate(selectedObject, null);

        Component[] components = clone.GetComponentsInChildren<Component>(true);
        
        for (int j = 0; j < components.Length; j++)
        {
            ArrayList excludedComponents = new ArrayList() { typeof(Transform), typeof(SkinnedMeshRenderer), typeof(MeshFilter), typeof(MeshRenderer) };
        
            if (!excludedComponents.Contains(components[j].GetType()))
            {
                DestroyImmediate(components[j]);
            }
        }

        if (createAnimator)
        {
            Animator newAnimator = clone.AddComponent<Animator>();
            newAnimator = CreateAnimator(selectedObject, clone.GetComponent<Animator>(), ConstructName(clone.name.Replace("(Clone)", "")));
        }

        clone.name = ConstructName(clone.name.Replace("(Clone)", ""));

    }

    private Material[] DuplicateMaterials(GameObject selectedObject)
    {
        Material[] newMaterials = selectedObject.GetComponent<Renderer>().sharedMaterials;

        for(int i = 0; i < selectedObject.GetComponent<Renderer>().sharedMaterials.Length; i++)
        {
            Material newMaterial;
            newMaterial = Instantiate(selectedObject.GetComponent<Renderer>().sharedMaterials[i]);
            newMaterial.name = ConstructName(newMaterial.name);
            newMaterials[i] = newMaterial;
        }
        return newMaterials;
    }

    private void SaveAsset(Mesh assetType, string assetName, string assetPath)
    {
        if (!Directory.Exists((Path.Combine(Application.dataPath, assetPath))))
        {
            EditorUtility.DisplayDialog("Invalid Destination", "Folder doesn't exist", "OK");
            return;
        }
        assetPath = "Assets/" + assetPath + "/";
        AssetDatabase.CreateAsset(assetType, assetPath + assetName + ".asset");
    }

    private void SaveAsset(Material assetType, string assetName, string assetPath)
    {
        if (!Directory.Exists((Path.Combine(Application.dataPath, assetPath))))
        {
            EditorUtility.DisplayDialog("Invalid Destination", "Folder doesn't exist", "OK");
            return;
        }
        assetPath = "Assets/" + assetPath + "/";
        AssetDatabase.CreateAsset(assetType, assetPath + assetName.Replace("(Clone)", "") + ".mat");
    }

    private void SaveAsset(AnimatorController assetType, string assetName, string assetPath)
    {
        if (!Directory.Exists((Path.Combine(Application.dataPath, assetPath))))
        {
            EditorUtility.DisplayDialog("Invalid Destination", "Folder doesn't exist", "OK");
            return;
        }
        assetPath = "Assets/" + assetPath + "/";
        AssetDatabase.CreateAsset(assetType, "Assets/" + assetName + ".controller");
    }

    private Animator CreateAnimator(GameObject selectedObject, Animator newAnimator, string assetName)
    {
        if(selectedObject.GetComponent<Animator>() != null)
        {
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/"+_animatorDestination+"/"+assetName+".controller");
            var rootStateMachine = controller.layers[0].stateMachine;
            var defaultState = rootStateMachine.AddState("Default");
           
            defaultState.motion = selectedObject.GetComponent<Animator>().runtimeAnimatorController.animationClips[0];
            newAnimator.runtimeAnimatorController = controller;

            return newAnimator;
        }
        return null;
    }

    private string ConstructName(string name)
    {
        string dateString;
        DateTime dateTime = DateTime.Now;
        dateString = dateTime.ToString("ddHHmmss");
        string newName = name+"_SS_" + dateString;

        return newName;
    }
}
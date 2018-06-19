using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LockTransforms : EditorWindow
{
    private bool _lockedFoldout;
    private bool _lockToggle;
    private string _lockLabel;

    private List<Vector3> _lossyScale;
    private List<Vector3> _localPositions;
    private List<Vector3> _localScales;
    private List<Quaternion> _localRotations;
    private Transform[] _transformsToLock;
    private List<string> _lockedObjects;

    private void Awake()
    {
        _lossyScale = new List<Vector3>();
        _localPositions = new List<Vector3>();
        _localScales = new List<Vector3>();
        _localRotations = new List<Quaternion>();
        _lockedObjects = new List<string>();
        _lockToggle = false;
        _lockLabel = "Lock Selected";
    }

    [MenuItem("Tools/Lock Transforms")]
    private static void ShowWindow()
    {
        GetWindow<LockTransforms>("Lock Selected");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Lock transforms on selected objects.");
        GUILayout.Space(15f);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(_lockLabel, GUILayout.Width(Screen.width / 2), GUILayout.Height(40)))
        {
            if (Selection.transforms.Length != 0)
            {
                _transformsToLock = Selection.transforms;
                LockSelected();
                for (int i = 0; i < _transformsToLock.Length; i++)
                {
                    _lockedObjects.Add(_transformsToLock[i].gameObject.name);
                }
                if (!_lockToggle)
                {
                    EditorApplication.update += Update;
                    _lockLabel = "Unlock";
                    _lockToggle = true;
                }
                else
                {
                    EditorApplication.update -= Update;
                    _lockLabel = "Lock Selected";
                    _lockToggle = false;
                    _localPositions.Clear();
                    _localRotations.Clear();
                    _localScales.Clear();
                    _lockedObjects.Clear();
                    _lossyScale.Clear();
                }
            }
            else
            {
                if (_lockToggle)
                {
                    EditorApplication.update -= Update;
                    _lockLabel = "Lock Selected";
                    _lockToggle = false;
                    _localPositions.Clear();
                    _localRotations.Clear();
                    _localScales.Clear();
                    _lockedObjects.Clear();
                    _lossyScale.Clear();
                }
                else
                {
                EditorUtility.DisplayDialog("Nothing selected", "Select at least one GameObject to lock", "OK");
                }
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(15f);

        if (_lockToggle)
        {
            _lockedFoldout = EditorGUILayout.Foldout(_lockedFoldout, "Locked objects:");
            if (_lockedFoldout)
            {
                GUI.enabled = false;
                for (int i = 0; i < _transformsToLock.Length; i++)
                {
                    EditorGUILayout.LabelField(_lockedObjects[i]);
                }
                GUI.enabled = true;
            }

        }
        else
        {
            GUI.enabled = false;
            EditorGUILayout.LabelField("No objects locked.");
            GUI.enabled = true;
        }



    }

    private void OnDestroy()
    {
        EditorApplication.update -= Update;
        _localPositions.Clear();
        _localRotations.Clear();
        _localScales.Clear();
        _lockedObjects.Clear();
        _lossyScale.Clear();
    }

    private void Update()
    {
        if (_lockToggle)
        {
            for (int i = 0; i < _transformsToLock.Length; i++)
            {
                _transformsToLock[i].position = _localPositions[i];
                _transformsToLock[i].rotation = _localRotations[i];
                if (_lossyScale[i] != _transformsToLock[i].lossyScale)
                {
                    _transformsToLock[i].localScale = Vector3.one;
                    _transformsToLock[i].localScale = new Vector3(_lossyScale[i].x / _transformsToLock[i].lossyScale.x,
                                                                 _lossyScale[i].y / _transformsToLock[i].lossyScale.y,
                                                                 _lossyScale[i].z / _transformsToLock[i].lossyScale.z);
                }

            }
        }
    }

    private void LockSelected()
    {
        for(int i = 0; i < _transformsToLock.Length; i++)
        {
            _localPositions.Add(_transformsToLock[i].position);
            _localRotations.Add(_transformsToLock[i].rotation);
            _localRotations[i] = _transformsToLock[i].rotation;
            _localScales.Add(_transformsToLock[i].localScale);
            _lossyScale.Add(_transformsToLock[i].lossyScale);
        }

    }
}

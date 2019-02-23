using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LockTransforms : EditorWindow
{
    private class LockedObject
    {
        public Vector3 lockedLossyScale;
        public Vector3 lockedLocalPosition;
        public Vector2 lockedPivot;
        public Vector2 lockedAnchoredPosition;
        public Vector2 lockedAnchoredPosition3D;
        public Vector2 lockedAnchorMax;
        public Vector2 lockedAnchorMin;
        public Quaternion lockedRotation;
        public Vector3[] lockedWorldCorners;

        public LockedObject(Vector3 lossyScale, Vector3 localPosition, Quaternion rotation)
        {
            lockedLossyScale = lossyScale;
            lockedLocalPosition = localPosition;
            lockedRotation = rotation;
        }
        public void LockedRectTransform(Vector2 pivot, Vector2 anchoredPosition, Vector2 anchorePostion3D, Vector2 anchorMax, Vector2 anchorMin, Vector3[] WorldCorners)
        {
            lockedPivot = pivot;
            lockedAnchoredPosition = anchoredPosition;
            lockedAnchoredPosition3D = anchorePostion3D;
            lockedAnchorMax = anchorMax;
            lockedAnchorMin = anchorMin;
            lockedWorldCorners = WorldCorners;
        }
    }

    private string prefsShowLockIndicator = "LockTransforms._showLockIndicator";
    private string prefsLockChildren = "LockTransforms._lockChildren";

    private List<LockedObject> _lockedObjects;
    private bool _showFoldout;
    private bool _isLocked;
    private string _lockLabel;
    private Texture2D _background;
    private GUIStyle _lockIndicatorStyle;
    private GUIStyle _lockIndicatorTextStyle;
    private bool _showLockIndicator;
    private List<Transform> _transformsToLock;
    private List<string> _lockedObjectNames;
    private List<bool> _isLockedObjectSelected;
    private bool _lockChildren;
    private bool _hasChanged;

    private void Awake()
    {
        GetEditorPrefs();
        _lockedObjectNames = new List<string>();
        _isLocked = false;
        _lockLabel = "Lock";
        _isLockedObjectSelected = new List<bool>();
        _lockedObjects = new List<LockedObject>();
        _transformsToLock = new List<Transform>();

        _background = new Texture2D(1, 1);
        _background.SetPixel(0, 0, new Color(1f, 0, 0, 0.2f));
        _background.Apply();
        _lockIndicatorStyle = new GUIStyle();
        _lockIndicatorTextStyle = new GUIStyle();
        _lockIndicatorStyle.normal.background = _background;
        _lockIndicatorTextStyle.normal.textColor = Color.white;
        _lockIndicatorTextStyle.fontStyle = FontStyle.Bold;
        _lockIndicatorTextStyle.fontSize = 15;
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
        EditorGUI.BeginChangeCheck();
        GUILayout.Button(_lockLabel, GUILayout.Width(Screen.width / 2), GUILayout.Height(40));
        if (EditorGUI.EndChangeCheck())
        {
            _transformsToLock = ReturnLockedTransforms(_lockChildren);
            if (!_isLocked && _transformsToLock.Count != 0)
            {
                _hasChanged = true;
                LockSelected();
                for (int i = 0; i < _transformsToLock.Count; i++)
                {
                    UpdateLockedTransforms(i);
                }
                EditorApplication.update += OnUpdate;
                _lockLabel = "Unlock";
                _isLocked = true;
                if (_showLockIndicator && _isLocked)
                {
                    SceneView.onSceneGUIDelegate -= OnSceneGUI;
                    SceneView.onSceneGUIDelegate += OnSceneGUI;
                }
            }
            else
            {
                SceneView.onSceneGUIDelegate -= OnSceneGUI;
                EditorApplication.update -= OnUpdate;
                _lockLabel = "Lock";
                _isLocked = false;
                ClearLists();
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(15f);
        EditorGUI.BeginChangeCheck();
        _showLockIndicator = EditorGUILayout.Toggle("Show lock indicator:", _showLockIndicator);
        if (EditorGUI.EndChangeCheck())
        {
            if (_showLockIndicator && _isLocked)
            {
                SceneView.onSceneGUIDelegate -= OnSceneGUI;
                SceneView.onSceneGUIDelegate += OnSceneGUI;
                SceneView.RepaintAll();
            }
            else
            {
                SceneView.onSceneGUIDelegate -= OnSceneGUI;
                SceneView.RepaintAll();
            }
        }
        _lockChildren = EditorGUILayout.Toggle("Lock Children:", _lockChildren);
        GUILayout.Space(15f);

        if (_isLocked)
        {
            _showFoldout = EditorGUILayout.Foldout(_showFoldout, "Locked objects:");
            if (_showFoldout)
            {
                for (int i = 0; i < _transformsToLock.Count; i++)
                {
                    _isLockedObjectSelected[i] = EditorGUILayout.ToggleLeft(_lockedObjectNames[i], _isLockedObjectSelected[i]);
                }
                if (GUILayout.Button("Unlock selected", GUILayout.MaxWidth(120)))
                {
                    for (int i = _transformsToLock.Count - 1; i > -1; i--)
                    {
                        if (_isLockedObjectSelected[i])
                        {
                            _isLockedObjectSelected.RemoveAt(i);
                            _lockedObjects.RemoveAt(i);
                            _transformsToLock.RemoveAt(i);
                        }
                        if (_transformsToLock.Count == 0)
                        {
                            _isLocked = false;
                            _lockLabel = "Lock";
                            _showFoldout = false;
                            SceneView.onSceneGUIDelegate -= OnSceneGUI;
                            SceneView.RepaintAll();
                            ClearLists();
                            break;
                        }
                    }
                }
            }

        }
        else
        {
            GUI.enabled = false;
            EditorGUILayout.LabelField("No objects locked.");
            GUI.enabled = true;
        }
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear prefs"))
        {
            EditorPrefs.DeleteKey(prefsShowLockIndicator);
            EditorPrefs.DeleteKey(prefsLockChildren);

            GetEditorPrefs();

            SaveEditorPrefs();
        }
        SaveEditorPrefs();
    }

    private void OnDestroy()
    {
        EditorApplication.update -= OnUpdate;
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        _isLockedObjectSelected.Clear();
        SaveEditorPrefs();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, 40));
        var rect = EditorGUILayout.BeginVertical();
        GUILayout.Space(10f);
        GUI.Box(rect, GUIContent.none, _lockIndicatorStyle);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10f);
        GUILayout.Label("LOCKED OBJECTS: " + _transformsToLock.Count, _lockIndicatorTextStyle);
        GUILayout.EndHorizontal();
        GUILayout.Space(15f);
        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    private void ClearLists()
    {
        _lockedObjects.Clear();
        _lockedObjectNames.Clear();
        _isLockedObjectSelected.Clear();
        _lockedObjects.Clear();
        _transformsToLock.Clear();
    }

    private void OnUpdate()
    {
        if (_isLocked)
        {
            for (int i = 0; i < _transformsToLock.Count; i++)
            {
                if (_transformsToLock[i].hasChanged)
                {
                    _hasChanged = true;
                }

            }
            if (_hasChanged)
            {
                for (int i = 0; i < _transformsToLock.Count; i++)
                {
                    //TO-DO: Figure out how to get undo to work with rectTransforms so that this isn't necessary
                    if (!_transformsToLock[i].GetComponent<RectTransform>())
                    {

                        Undo.RecordObject(_transformsToLock[i], Undo.GetCurrentGroupName());
                    }
                    UpdateLockedTransforms(i);
                    _transformsToLock[i].hasChanged = false;
                    if (!_transformsToLock[i].GetComponent<RectTransform>())
                    {
                        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                    }
                }
            }
        }
    }

    private void UpdateLockedTransforms(int i)
    {
        _transformsToLock[i].rotation = _lockedObjects[i].lockedRotation;
        if (_transformsToLock[i].GetComponent<RectTransform>())
        {
            RectTransform rectTrans = _transformsToLock[i].GetComponent<RectTransform>();
            rectTrans.pivot = _lockedObjects[i].lockedPivot;
            rectTrans.anchoredPosition = _lockedObjects[i].lockedAnchoredPosition;
            rectTrans.anchoredPosition = _lockedObjects[i].lockedAnchoredPosition3D;
            rectTrans.anchorMax = _lockedObjects[i].lockedAnchorMax;
            rectTrans.anchorMin = _lockedObjects[i].lockedAnchorMin;
            rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Vector3.Distance(_lockedObjects[i].lockedWorldCorners[0], _lockedObjects[i].lockedWorldCorners[1]));
            rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Vector3.Distance(_lockedObjects[i].lockedWorldCorners[0], _lockedObjects[i].lockedWorldCorners[3]));

        }
        if (_lockedObjects[i].lockedLossyScale != _transformsToLock[i].lossyScale)
        {

            _transformsToLock[i].localScale = Vector3.one;
            _transformsToLock[i].localScale = new Vector3(_lockedObjects[i].lockedLossyScale.x / _transformsToLock[i].lossyScale.x,
                                                          _lockedObjects[i].lockedLossyScale.y / _transformsToLock[i].lossyScale.y,
                                                          _lockedObjects[i].lockedLossyScale.z / _transformsToLock[i].lossyScale.z);
        }
        _transformsToLock[i].position = _lockedObjects[i].lockedLocalPosition;
    }

    private void LockSelected()
    {
        for (int i = 0; i < _transformsToLock.Count; i++)
        {
            _lockedObjectNames.Add(_transformsToLock[i].gameObject.name);
            _isLockedObjectSelected.Add(false);

            _lockedObjects.Add(new LockedObject(_transformsToLock[i].lossyScale, _transformsToLock[i].position, _transformsToLock[i].rotation));
            if (_transformsToLock[i].GetComponent<RectTransform>())
            {
                Vector3[] worldCorners = new Vector3[4];
                _transformsToLock[i].GetComponent<RectTransform>().GetWorldCorners(worldCorners);
                _lockedObjects[i].LockedRectTransform(_transformsToLock[i].GetComponent<RectTransform>().pivot,
                                                      _transformsToLock[i].GetComponent<RectTransform>().anchoredPosition, _transformsToLock[i].GetComponent<RectTransform>().anchoredPosition3D,
                                                      _transformsToLock[i].GetComponent<RectTransform>().anchorMax, _transformsToLock[i].GetComponent<RectTransform>().anchorMin,
                                                      worldCorners);
            }
        }

    }

    private List<Transform> ReturnLockedTransforms(bool returnChildren)
    {
        List<Transform> selectedTransforms = new List<Transform>();
        for (int i = 0; i < Selection.transforms.Length; i++)
        {
            selectedTransforms.Add(Selection.transforms[i]);
        }
        if (selectedTransforms.Count == 0)
        {
            EditorUtility.DisplayDialog("Nothing selected", "Select at least one GameObject to lock", "OK");
            return selectedTransforms;
        }
        if (returnChildren)
        {
            selectedTransforms = GetImidiateChildren(selectedTransforms);
            if (selectedTransforms.Count == 0)
            {
                EditorUtility.DisplayDialog("Invalid Selection", "Selected objects have no children", "OK");
            }
        }
        return selectedTransforms;
    }

    private List<Transform> GetImidiateChildren(List<Transform> selectedTransforms)
    {
        List<Transform> imidiateChildren = new List<Transform>();
        foreach (Transform selectedTransform in selectedTransforms)
        {
            Transform[] children = selectedTransform.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child.parent == selectedTransform)
                {
                    imidiateChildren.Add(child);
                }
            }
        }
        selectedTransforms = imidiateChildren;
        return selectedTransforms;
    }

    private void GetEditorPrefs()
    {
        _showLockIndicator = EditorPrefs.GetBool(prefsShowLockIndicator, true);
        _lockChildren = EditorPrefs.GetBool(prefsLockChildren, false);
    }
    private void SaveEditorPrefs()
    {
        EditorPrefs.SetBool(prefsShowLockIndicator, _showLockIndicator);
        EditorPrefs.SetBool(prefsLockChildren, _lockChildren);
    }
}

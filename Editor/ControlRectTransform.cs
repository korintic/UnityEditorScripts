using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;
using UnityEditorInternal;

public class ControlRectTransform : EditorWindow
{
    private RectTransform _targetTransform;
    private RectTransform _tempTransform;
    private RectTransform _sourceTransform;
    private GameObject _tempGameObject;
    private GameObject _sourceGameObject;
    private Editor _sourceTransformEditor;
    private Editor _emptyTransformEditor;
    private Assembly _assembly;
    private Type _rctEditor;
    private GameObject _canvas;
    private GameObject _selected;
    private bool _isValidTarget;
    private Vector2 _center;

    [MenuItem("Tools/Control RectTransform")]
    public static void ShowWindow()
    {
        GetWindow<ControlRectTransform>("RectTransform");
    }

    public void Awake()
    {
        _center = new Vector2(0.5f, 0.5f);
        _assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        _rctEditor = _assembly.GetType("UnityEditor.RectTransformEditor");

        _canvas = new GameObject("Source Canvas", typeof(Canvas))
        {
            hideFlags = HideFlags.HideInHierarchy
        };
        _tempGameObject = new GameObject("Temp", typeof(RectTransform))
        {
            hideFlags = HideFlags.HideInHierarchy
        };
        _sourceGameObject = new GameObject("Source", typeof(RectTransform))
        {
            hideFlags = HideFlags.HideInHierarchy
        };
        _tempGameObject.SetActive(false);
        _sourceGameObject.SetActive(false);
        _tempGameObject.transform.SetParent(_canvas.transform, false);
        _sourceGameObject.transform.SetParent(_canvas.transform, false);
        _tempTransform = _tempGameObject.GetComponent<RectTransform>();
        _sourceTransform = _sourceGameObject.GetComponent<RectTransform>();

        Selection.selectionChanged += SelectionChanged;
        SetTarget();
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!Equals(_tempGameObject, _targetTransform))
        {
            CopyPasteValues(_targetTransform, _tempTransform);
            Repaint();
        }
    }

    private void OnGUI()
    {
        if (_sourceTransformEditor != null)
        {
            DestroyImmediate(_sourceTransformEditor);
        }
        _sourceTransformEditor = Editor.CreateEditor(_tempTransform, _rctEditor);
        if(_emptyTransformEditor != null)
        {
            DestroyImmediate(_emptyTransformEditor);
        }
        _emptyTransformEditor = Editor.CreateEditor(_sourceTransform, _rctEditor);
        EditorGUI.BeginChangeCheck();
        if(_isValidTarget)
        {
            GUILayout.Label("Target Transform:", EditorStyles.boldLabel);
            GUILayout.Space(10);
            _sourceTransformEditor.OnInspectorGUI();
            GUILayout.Space(15);
            if (GUILayout.Button("Apply to Source", GUILayout.Width(120), GUILayout.Height(25)))
            {
                CopyPasteValues(_tempTransform, _sourceTransform);
            }
            GUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
        if (EditorGUI.EndChangeCheck())
        {
            if(!Equals(_tempTransform, _targetTransform))
            {
                CopyPasteValues(_tempTransform, _targetTransform);
                SceneView.RepaintAll();
            }
        }
        GUILayout.Space(10);
        GUILayout.Label("Source Transform:", EditorStyles.boldLabel);
        GUILayout.Space(10);
        _emptyTransformEditor.OnInspectorGUI();
        GUILayout.Space(15);
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(!_isValidTarget);
        if(GUILayout.Button("Apply to Target", GUILayout.Width(120), GUILayout.Height(25)))
        {
            CopyPasteValues(_sourceTransform, _tempTransform);
            CopyPasteValues(_sourceTransform, _targetTransform);
        }
        EditorGUI.EndDisabledGroup();
        if (GUILayout.Button("Center Anchor Preset", GUILayout.Width(150), GUILayout.Height(25)))
        {
            _sourceTransform.anchorMax = _center;
            _sourceTransform.anchorMin = _center;
            _sourceTransform.pivot = _center;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void OnDestroy()
    {
        if (_sourceTransformEditor != null)
        {
            DestroyImmediate(_sourceTransformEditor);
        }
        if(_emptyTransformEditor != null)
        {
            DestroyImmediate(_emptyTransformEditor);
        }
        DestroyImmediate(_tempGameObject);
        DestroyImmediate(_sourceGameObject);
        DestroyImmediate(_canvas);
        Selection.selectionChanged -= SelectionChanged;
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    private void SelectionChanged()
    {
        SetTarget();
        _sourceTransformEditor.Repaint();
    }

    private void SetTarget()
    {
        _selected = Selection.activeGameObject;
        if (_selected != null && _selected.GetComponent<RectTransform>() != null)
        {
            _targetTransform = _selected.GetComponent<RectTransform>();
            _isValidTarget = true;
            _tempTransform.transform.SetParent(_targetTransform, false);
            _sourceTransform.transform.SetParent(_targetTransform, false);
            ComponentUtility.CopyComponent(_targetTransform);
            ComponentUtility.PasteComponentValues(_tempTransform);
        }
        else
        {
            _isValidTarget = false;
            _tempGameObject.transform.SetParent(_canvas.transform, false);
            _sourceGameObject.transform.SetParent(_canvas.transform, false);
        }
    }

    private void CopyPasteValues(Component source, Component target)
    {
        ComponentUtility.CopyComponent(source);
        ComponentUtility.PasteComponentValues(target);
    }
}

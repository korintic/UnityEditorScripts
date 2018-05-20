using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class Capture : EditorWindow
{
    //EDITORPREF VARIABLES
    private static string prefsIsTransparent = "Capture._isTransparent";
    private string prefsOverwriteWarning = "Capture._overwriteWarning";
    private string prefsUniqueSuffix = "Capture._uniqueSuffix";
    private string prefsSizeMultiplier = "Capture._sizeMultiplier";
    private string prefsFileName = "Capture._fileName";
    private string prefsPathName = "Capture._pathName";
    private string prefsSuffixType = "Capture._suffixTupe";

    private string _fileName;
    private string _pathName;
    private string _previousFileName;

    private int _runningNumber = 1;
    private int _sizeMultiplier;

    private bool _uniqueSuffix;
    private bool _overwriteWarning;
    private bool _isTransparent;

    private enum SUFFIXTYPE
    {
        INCREMENT = 0,
        DATE = 1
    }
    private SUFFIXTYPE _suffixType;
    private string _suffix;

    private void OnEnable()
    {
        _isTransparent = EditorPrefs.GetBool(prefsIsTransparent, false);
        _overwriteWarning = EditorPrefs.GetBool(prefsOverwriteWarning, false);
        _uniqueSuffix = EditorPrefs.GetBool(prefsUniqueSuffix, false);
        _sizeMultiplier = EditorPrefs.GetInt(prefsSizeMultiplier, 1);
        _fileName = EditorPrefs.GetString(prefsFileName, "Screenshot");
        _pathName = EditorPrefs.GetString(prefsPathName, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        _suffixType = (SUFFIXTYPE)EditorPrefs.GetInt(prefsSuffixType, 1);
    }

    [MenuItem("Tools/Capture %#t")]
    public static void ShowWindow()
    {
        GetWindow<Capture>("Capture");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Folder:", EditorStyles.boldLabel);
        if (GUILayout.Button("Select Folder"))
        {
            _pathName = EditorUtility.SaveFolderPanel("Choose destination folder", _pathName, "");
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("File name:", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _fileName = EditorGUILayout.TextField(_fileName);
        if (EditorGUI.EndChangeCheck())
        {
            if(_fileName == "")
            {
                _fileName = _previousFileName;
            }
            _runningNumber = 0;
            _fileName = Path.GetFileNameWithoutExtension(_fileName);
            _previousFileName = _fileName;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();

        GUI.enabled = false;
        EditorGUILayout.LabelField(Path.GetFullPath(Path.Combine(_pathName, Path.GetFileNameWithoutExtension(_fileName)+_suffix+".png")));
        GUI.enabled = true;

        EditorGUILayout.Space();

        _overwriteWarning = EditorGUILayout.Toggle("Overwrite warning:", _overwriteWarning);
        _uniqueSuffix = EditorGUILayout.Toggle("Add unique suffix:", _uniqueSuffix);
        if (!_uniqueSuffix)
        {
            GUI.enabled = false;
            EditorGUILayout.EnumPopup("Suffix type:", _suffixType);
            GUI.enabled = true;
            _suffix = "";

        }
        else
        {
            _suffixType = (SUFFIXTYPE)EditorGUILayout.EnumPopup("Suffix type:", _suffixType);
            _suffix = "_suffix";
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginHorizontal();

        _sizeMultiplier = EditorGUILayout.IntField("Image size multiplier:", _sizeMultiplier);
        _sizeMultiplier = Mathf.Clamp(_sizeMultiplier, 1, 10);
        if (_sizeMultiplier > 1)
        {
            _isTransparent = false;
        }
        _isTransparent = EditorGUILayout.Toggle("Capture transparency:", _isTransparent);
        if (_isTransparent)
        {
            _sizeMultiplier = 1;
        }


        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (GUILayout.Button("Capture",GUILayout.Width(Screen.width-6), GUILayout.Height(40)))
        {
            if(_pathName == null)
            {
                _pathName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            if (!Directory.Exists(_pathName))
            {
                EditorUtility.DisplayDialog("Alert", "Folder does not exist!", "OK");
            }
            else
            {
                string _tempFileName = _fileName;
                string _tempPathName = _pathName;
                string _fileNameWithSuffix;

                if (_uniqueSuffix)
                {
                    _fileNameWithSuffix = AddSuffix(_tempFileName, _suffixType, _runningNumber);
                    _tempPathName = Path.Combine(_tempPathName, _fileNameWithSuffix);
                }
                else
                {
                    _tempPathName = Path.GetFullPath(Path.Combine(_tempPathName, _tempFileName + ".png"));
                }

                if (File.Exists(_tempPathName) && _uniqueSuffix && _suffixType == SUFFIXTYPE.INCREMENT)
                {
                    while (File.Exists(_tempPathName))
                    {
                        _runningNumber++;
                        _fileNameWithSuffix = AddSuffix(_fileName, _suffixType, _runningNumber);
                        _tempPathName = Path.Combine(_pathName, _fileNameWithSuffix);
                    }
                }

                if(File.Exists(_tempPathName) && _overwriteWarning)
                {
                    int choose =EditorUtility.DisplayDialogComplex("Alert", "File exist!", "Overwrite", "Save with unique suffix", "Cancel");
                    switch (choose)
                    {
                        case 0:
                            SaveScreenShot(_tempPathName, _sizeMultiplier, _isTransparent);

                            break;
                        case 1:
                            _tempPathName = Path.Combine(_pathName, AddSuffix(_fileName, SUFFIXTYPE.DATE, 0));
                            SaveScreenShot(_tempPathName, _sizeMultiplier, _isTransparent);
                            break;
                        case 2:
                            break;
                    }
                }
                else
                {
                    SaveScreenShot(_tempPathName, _sizeMultiplier, _isTransparent);
                }

            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear prefs",GUILayout.Width(Screen.width/4)))
        {
            EditorPrefs.DeleteKey(prefsIsTransparent);
            EditorPrefs.DeleteKey(prefsOverwriteWarning);
            EditorPrefs.DeleteKey(prefsUniqueSuffix);
            EditorPrefs.DeleteKey(prefsSizeMultiplier);
            EditorPrefs.DeleteKey(prefsFileName);
            EditorPrefs.DeleteKey(prefsPathName);
            EditorPrefs.DeleteKey(prefsSuffixType);

            _isTransparent = EditorPrefs.GetBool(prefsIsTransparent, false);
            _overwriteWarning = EditorPrefs.GetBool(prefsOverwriteWarning, false);
            _uniqueSuffix = EditorPrefs.GetBool(prefsUniqueSuffix, false);
            _sizeMultiplier = EditorPrefs.GetInt(prefsSizeMultiplier, 1);
            _fileName = EditorPrefs.GetString(prefsFileName, "Screenshot");
            _pathName = EditorPrefs.GetString(prefsPathName, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            _suffixType = (SUFFIXTYPE)EditorPrefs.GetInt(prefsSuffixType, 1);
        }
    }

    private void OnDestroy()
    {
        EditorPrefs.SetBool(prefsIsTransparent, _isTransparent);
        EditorPrefs.SetBool(prefsOverwriteWarning, _overwriteWarning);
        EditorPrefs.SetBool(prefsUniqueSuffix, _uniqueSuffix);
        EditorPrefs.SetInt(prefsSizeMultiplier, _sizeMultiplier);
        EditorPrefs.SetString(prefsFileName, _fileName);
        EditorPrefs.SetString(prefsPathName, _pathName);
        EditorPrefs.SetInt(prefsSuffixType, (int)_suffixType);
    }

    private void SaveScreenShot(string pathName, int sizemultiplier, bool isTransparent)
    {
        if (sizemultiplier > 1)
        {
            if (EditorApplication.isPlaying)
            {
                //This is to make sure the capture is taken from a frame that has finished rendering.
                //No idea if this actually works like that.
                EditorApplication.isPaused = true;
                ScreenCapture.CaptureScreenshot(pathName, sizemultiplier);
                EditorApplication.isPaused = false;
            }
            ScreenCapture.CaptureScreenshot(pathName, sizemultiplier);
        }
        else
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPaused = true;
                CaptureScreen(pathName, isTransparent);
                EditorApplication.isPaused = false;
            }
            CaptureScreen(pathName, isTransparent);
        }
    }

    private string AddSuffix(string fileName, SUFFIXTYPE suffixtype, int runningNumber)
    {
        string dateString;
        DateTime dateTime = DateTime.Now;

        if(suffixtype == SUFFIXTYPE.INCREMENT)
        {

            fileName += "_" + runningNumber.ToString("D4");
            runningNumber++;
        }
        if(suffixtype == SUFFIXTYPE.DATE)
        {
            dateString = dateTime.ToString("yyMMddHHmmss");
            fileName += "_" + dateString;
        }

        //fileName = Path.GetFileNameWithoutExtension(fileName);
        fileName += ".png";
        return fileName;
    }

    private void CaptureScreen(string pathName, bool isTransparent)
    {
        Camera _mainCamera = Camera.main;
        CameraClearFlags _flag = _mainCamera.clearFlags;
        Texture2D _texture = new Texture2D(_mainCamera.pixelWidth, _mainCamera.pixelHeight, TextureFormat.RGBA32, false);
        RenderTexture _renderTexture = new RenderTexture(_mainCamera.pixelWidth, _mainCamera.pixelHeight, 32);

        if (isTransparent)
        {
            _mainCamera.clearFlags = CameraClearFlags.Depth;
        }

        _mainCamera.targetTexture = _renderTexture;
        _mainCamera.Render();
        RenderTexture.active = _renderTexture;

        _texture.ReadPixels(new Rect(0, 0, _mainCamera.pixelWidth, _mainCamera.pixelHeight), 0, 0, false);
        _texture.Apply();

        byte[] bytes = _texture.EncodeToPNG();
        File.WriteAllBytes(pathName, bytes);

        if (isTransparent)
        {
            _mainCamera.clearFlags = _flag;
        }

        RenderTexture.active = null;
        _mainCamera.targetTexture = null;
        DestroyImmediate(_renderTexture);
        DestroyImmediate(_texture);
    }

}

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
        GetEditorPrefs();
    }

    [MenuItem("Tools/Capture Options %#w")]
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
        _fileName = EditorGUILayout.TextField(_fileName, GUILayout.MaxWidth(400));
        if (EditorGUI.EndChangeCheck())
        {
            if (_fileName == "")
            {
                _fileName = _previousFileName;
            }
            _runningNumber = 1;
            _fileName = Path.GetFileNameWithoutExtension(_fileName);
            _previousFileName = _fileName;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();

        GUI.enabled = false;
        EditorGUILayout.LabelField(Path.GetFullPath(Path.Combine(_pathName, Path.GetFileNameWithoutExtension(_fileName) + _suffix + ".png")));
        GUI.enabled = true;

        EditorGUILayout.Space();

        _overwriteWarning = EditorGUILayout.Toggle("Overwrite warning:", _overwriteWarning);
        _uniqueSuffix = EditorGUILayout.Toggle("Add unique suffix:", _uniqueSuffix);
        if (!_uniqueSuffix)
        {
            GUI.enabled = false;
            EditorGUILayout.EnumPopup("Suffix type:", _suffixType, GUILayout.MaxWidth(250));
            GUI.enabled = true;
            _suffix = "";

        }
        else
        {
            _suffixType = (SUFFIXTYPE)EditorGUILayout.EnumPopup("Suffix type:", _suffixType, GUILayout.MaxWidth(250));
            _suffix = "_suffix";
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginHorizontal();

        _sizeMultiplier = EditorGUILayout.IntField("Image size multiplier:", _sizeMultiplier, GUILayout.MaxWidth(200));
        _sizeMultiplier = Mathf.Clamp(_sizeMultiplier, 1, 10);

        _isTransparent = EditorGUILayout.Toggle("Capture transparency:", _isTransparent);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (GUILayout.Button("Capture", GUILayout.Width(Screen.width - 6), GUILayout.Height(40)))
        {
            DoCapture();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear prefs", GUILayout.Width(Screen.width / 4)))
        {
            EditorPrefs.DeleteKey(prefsIsTransparent);
            EditorPrefs.DeleteKey(prefsOverwriteWarning);
            EditorPrefs.DeleteKey(prefsUniqueSuffix);
            EditorPrefs.DeleteKey(prefsSizeMultiplier);
            EditorPrefs.DeleteKey(prefsFileName);
            EditorPrefs.DeleteKey(prefsPathName);
            EditorPrefs.DeleteKey(prefsSuffixType);

            GetEditorPrefs();

            SaveEditorPrefs();
        }

        SaveEditorPrefs();
    }

    private void OnDestroy()
    {
        SaveEditorPrefs();
    }

    protected void DoCapture()
    {
        if (_pathName == null)
        {
            _pathName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
        if (!Directory.Exists(_pathName))
        {
            EditorUtility.DisplayDialog("Alert", "Folder does not exist!", "OK");
        }
        else
        {
            string tempFileName = _fileName;
            string tempPathName = _pathName;
            string fileNameWithSuffix;

            if (_uniqueSuffix)
            {
                fileNameWithSuffix = AddSuffix(tempFileName, _suffixType, _runningNumber);
                tempPathName = Path.Combine(tempPathName, fileNameWithSuffix + ".png");
            }
            else
            {
                tempPathName = Path.GetFullPath(Path.Combine(tempPathName, tempFileName + ".png"));
            }

            if (File.Exists(tempPathName) && _uniqueSuffix && _suffixType == SUFFIXTYPE.INCREMENT)
            {
                while (File.Exists(tempPathName))
                {
                    _runningNumber++;
                    fileNameWithSuffix = AddSuffix(_fileName, _suffixType, _runningNumber);
                    tempPathName = Path.Combine(_pathName, fileNameWithSuffix + ".png");
                }
            }

            if (File.Exists(tempPathName) && _overwriteWarning)
            {
                int choose = EditorUtility.DisplayDialogComplex("Alert", "File exist!", "Overwrite", "Save with unique suffix", "Cancel");
                switch (choose)
                {
                    case 0:
                        SaveScreenShot(tempPathName, _sizeMultiplier, _isTransparent);

                        break;
                    case 1:
                        tempPathName = Path.Combine(_pathName, AddSuffix(_fileName, SUFFIXTYPE.DATE, 0)+".png");
                        SaveScreenShot(tempPathName, _sizeMultiplier, _isTransparent);
                        break;
                    case 2:
                        break;
                }
            }
            else
            {
                SaveScreenShot(tempPathName, _sizeMultiplier, _isTransparent);
            }

        }
    }

    private void SaveScreenShot(string pathName, int sizemultiplier, bool isTransparent)
    {
        //This is to make sure the capture is taken from a frame that has finished rendering.
        //o idea if this actually works like that.
        if (EditorApplication.isPlaying && !EditorApplication.isPaused)
        {
            EditorApplication.isPaused = true;
            CaptureScreen(pathName, isTransparent, _sizeMultiplier);
            EditorApplication.isPaused = false;
        }
        CaptureScreen(pathName, isTransparent, _sizeMultiplier);
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

        return fileName;
    }

    private void CaptureScreen(string pathName, bool isTransparent, int sizeMultiplier)
    {
        Camera mainCamera = Camera.main;
        CameraClearFlags _flag = mainCamera.clearFlags;
        float defaultAspect = mainCamera.aspect;

        Texture2D tex;
        RenderTexture renderTex = new RenderTexture(mainCamera.pixelWidth * sizeMultiplier, mainCamera.pixelHeight * sizeMultiplier, 32);

        if (isTransparent)
        {
            tex = new Texture2D(mainCamera.pixelWidth * sizeMultiplier, mainCamera.pixelHeight * sizeMultiplier, TextureFormat.RGBA32, false);
            mainCamera.clearFlags = CameraClearFlags.Depth;
        }
        else
        {
            tex = new Texture2D(mainCamera.pixelWidth * sizeMultiplier, mainCamera.pixelHeight * sizeMultiplier, TextureFormat.RGB24, false);
        }

        mainCamera.targetTexture = renderTex;
        mainCamera.aspect = (float)(mainCamera.pixelWidth * sizeMultiplier) / (float)(mainCamera.pixelHeight * sizeMultiplier);
        mainCamera.Render();
        RenderTexture.active = renderTex;

        tex.ReadPixels(new Rect(0, 0, mainCamera.pixelWidth, mainCamera.pixelHeight), 0, 0, false);

        //Unpremultiply alpha
        if (isTransparent)
        {
            Color32[] premultCol = tex.GetPixels32();
            for(int i = 0; i < premultCol.Length; ++i)
            {
                Color col = premultCol[i];
                if(col.a != 0)
                {
                    col.r = ((col.r / col.a));
                    col.g = ((col.g / col.a));
                    col.b = ((col.b / col.a));
                }
                premultCol[i] = col;
             }
            tex.SetPixels32(premultCol);
        }
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(pathName, bytes);

        if (isTransparent)
        {
            mainCamera.clearFlags = _flag;
        }

        RenderTexture.active = null;
        mainCamera.targetTexture = null;
        mainCamera.aspect = defaultAspect;
        DestroyImmediate(renderTex);
        DestroyImmediate(tex);
    }

    private void GetEditorPrefs()
    {
        _isTransparent = EditorPrefs.GetBool(prefsIsTransparent, false);
        _overwriteWarning = EditorPrefs.GetBool(prefsOverwriteWarning, true);
        _uniqueSuffix = EditorPrefs.GetBool(prefsUniqueSuffix, false);
        _sizeMultiplier = EditorPrefs.GetInt(prefsSizeMultiplier, 1);
        _fileName = EditorPrefs.GetString(prefsFileName, "Screenshot");
        _pathName = EditorPrefs.GetString(prefsPathName, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        _suffixType = (SUFFIXTYPE)EditorPrefs.GetInt(prefsSuffixType, 0);
    }

    private void SaveEditorPrefs()
    {
        EditorPrefs.SetBool(prefsIsTransparent, _isTransparent);
        EditorPrefs.SetBool(prefsOverwriteWarning, _overwriteWarning);
        EditorPrefs.SetBool(prefsUniqueSuffix, _uniqueSuffix);
        EditorPrefs.SetInt(prefsSizeMultiplier, _sizeMultiplier);
        EditorPrefs.SetString(prefsFileName, _fileName);
        EditorPrefs.SetString(prefsPathName, _pathName);
        EditorPrefs.SetInt(prefsSuffixType, (int)_suffixType);
    }

}

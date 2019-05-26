using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ProcessIcons : EditorWindow
{
    // EDITOR PREF VARIABLES
    private string prefsExecutablePath = "ProcessIcons._executablePath";
    private string prefsScriptPath = "ProcessIcons._scriptPath";

    private string _executablePath;
    private string _scriptPath;
    private bool _isExited = false;
    private string _path = "";

    private bool _showSettingFoldout = true;
    private bool _showIconPathFoldout = false;
    private bool _showPathsFoldout = true;

    private bool _isValidExecutablePath = false;
    private bool _isValidScriptPath = false;
    
    private int _iconsToProcess;
    private List<string> _iconPaths;

    public class Config
    {
        public bool trim, removeMatte, resize, respectAspect;
        public int width, height, fuzziness;
        public float red, green, blue;
        public void Load(string savedData)
        {
            JsonUtility.FromJsonOverwrite(savedData, this);
        }
    }

    public Config config;
    private Color32 _matteColor;


    [MenuItem("Tools/Process Icons")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<ProcessIcons>("Process Icons");
        window.minSize = new Vector2(400, 500);
    }

    void OnEnable()
    {
        Selection.selectionChanged += getSelectedTextures;
        _iconPaths = new List<string>();
        config = new Config();
        ReadConfig();
        _matteColor.r = (byte)config.red;
        _matteColor.g = (byte)config.green;
        _matteColor.b = (byte)config.blue;

        GetEditorPrefs();
        if(!File.Exists(_scriptPath) || _scriptPath == Application.dataPath)
        {
            if(File.Exists(Application.dataPath+"/Editor/Resources/ProcessIcon.jsx"))
            {
                _scriptPath = Path.GetFullPath(Application.dataPath+"/Editor/Resources/ProcessIcon.jsx");
            }
        }
        if(_scriptPath.EndsWith(".jsx", true, null))
        { 
            _isValidScriptPath = true;
        }
        if(_executablePath.EndsWith("Photoshop.exe", true, null))
        { 
            _isValidExecutablePath = true;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Space(20f);
        EditorGUILayout.LabelField("Icons to process: " + _iconsToProcess, EditorStyles.boldLabel);
        _showIconPathFoldout = EditorGUILayout.Foldout(_showIconPathFoldout, "Selected icons:");
        if (_showIconPathFoldout)
        {
            if(_iconPaths.Count != 0 || _iconPaths != null)
            {
                for(int i = 0; i < _iconPaths.Count; i++)
                {
                    GUI.enabled = false;
                    EditorGUILayout.LabelField(_iconPaths[i]);
                    GUI.enabled = true;
                }
            }
        }
        GUILayout.Space(20f);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent("Process Icons", "Processes icons with Photoshop"), GUILayout.Width(Screen.width / 2 + 4), GUILayout.Height(40)))
        {
            for(int i = 0; i < _iconPaths.Count; i++)
            {
                RunCMD("/C @\"" + _executablePath + "\" \"" + Path.Combine(Application.dataPath, _iconPaths[i].Substring(7)) + "\" \"" + _scriptPath + "\"");
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(35f);
        _showSettingFoldout = EditorGUILayout.Foldout(_showSettingFoldout, "Icon processing settings:");
        if (_showSettingFoldout)
        {
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            config.removeMatte = EditorGUILayout.Toggle("Remove Matte", config.removeMatte);
            if(EditorGUI.EndChangeCheck())
            {
                WriteConfig();
            }

            EditorGUI.BeginDisabledGroup(!config.removeMatte);
            EditorGUI.BeginChangeCheck();
            _matteColor = EditorGUILayout.ColorField("Matte Color", _matteColor, GUILayout.Width(Screen.width / 1.8f));
            if(EditorGUI.EndChangeCheck())
            {
                config.red = _matteColor.r;
                config.green = _matteColor.g;
                config.blue = _matteColor.b;
                WriteConfig();
            }

            EditorGUI.BeginChangeCheck();
            config.fuzziness = EditorGUILayout.IntField("Matte Fuzziness",config.fuzziness, GUILayout.Width(Screen.width / 1.96f));
            if(EditorGUI.EndChangeCheck())
            {
                if(config.fuzziness < 0)
                {
                    config.fuzziness = 0;
                }
                else if(config.fuzziness > 200)
                {
                    config.fuzziness = 200;
                }
                WriteConfig();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            config.trim = EditorGUILayout.Toggle(new GUIContent("Trim", "Trims the icon based on transparent pixels"), config.trim);
            if(EditorGUI.EndChangeCheck())
            {
                WriteConfig();
            }
            EditorGUI.BeginChangeCheck();
            config.respectAspect = EditorGUILayout.Toggle(new GUIContent("Resize Canvas", "Resizes canvas after trim operation so that both sides equal the longer side"), config.respectAspect);
            if(EditorGUI.EndChangeCheck())
            {
                WriteConfig();
            }

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            config.resize = EditorGUILayout.Toggle("Resize Image", config.resize);
            if(EditorGUI.EndChangeCheck())
            {
                WriteConfig();
            }
            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginDisabledGroup(!config.resize);
            EditorGUI.BeginChangeCheck();
            config.width = EditorGUILayout.IntField("Width",config.width, GUILayout.Width(Screen.width / 1.96f));
            if(EditorGUI.EndChangeCheck())
            {
                if(config.width < 1)
                {
                    config.width = 1;
                }
                WriteConfig();
            }

            EditorGUI.BeginChangeCheck();
            config.height = EditorGUILayout.IntField("Height",config.height, GUILayout.Width(Screen.width / 1.96f));
            if(EditorGUI.EndChangeCheck())
            {
                if(config.height < 1)
                {
                    config.height = 1;
                }
                WriteConfig();
            }
            EditorGUI.EndDisabledGroup();
        }

        GUI.enabled = true;
        GUILayout.Space(20f);
        _showPathsFoldout = EditorGUILayout.Foldout(_showPathsFoldout, "Settings:");
        if (_showPathsFoldout)
        {
            EditorGUILayout.Space();
            GUI.enabled = false;
            EditorGUILayout.LabelField(_executablePath);
            GUI.enabled = true;
            if (GUILayout.Button(new GUIContent("Select Photoshop Path", "Select path to Photoshop.exe")))
            {
                if(_executablePath == null || _executablePath == "")
                {
                    _executablePath = Application.dataPath;
                }
                _executablePath = setFilePath(_executablePath, "Select Photoshop.exe", "exe");
                if(_executablePath.EndsWith("Photoshop.exe", true, null))
                { 
                    _isValidExecutablePath = true;
                }
                else
                {
                    _isValidExecutablePath = false;
                }
            }
            EditorGUILayout.Space();
            GUI.enabled = false;
            EditorGUILayout.LabelField(_scriptPath);
            GUI.enabled = true;
            if (GUILayout.Button(new GUIContent("Select Script", "Select path to ExtendScript to run")))
            {
                if(_scriptPath == null || _scriptPath == "")
                {
                    _scriptPath = Application.dataPath;
                }
                _scriptPath = setFilePath(_scriptPath, "Select ExtendScript", "jsx");
                if(_scriptPath.EndsWith(".jsx", true, null))
                { 
                   _isValidScriptPath = true;
                }
                else
                {
                    _isValidScriptPath = false;
                }            
            }
        }
        GUILayout.Space(20f);
        if(!_isValidExecutablePath)
        {
            EditorGUILayout.HelpBox("Path to Photoshop.exe is not valid!",MessageType.Error);
        }
        if(!_isValidScriptPath)
        {
            EditorGUILayout.HelpBox("Path to ExtendScript is not valid!",MessageType.Error);
        }
        EditorGUILayout.EndVertical();
    }

    private void Update()
    {
        if (_isExited)
        {
            AssetDatabase.Refresh();
            _isExited = false;
        }
    }

    private void OnDestroy()
    {
        Selection.selectionChanged -= getSelectedTextures;
        if(!File.Exists(_executablePath) || _executablePath == Application.dataPath || _executablePath == "")
        {
            _executablePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
        if(!File.Exists(_scriptPath) || _scriptPath == Application.dataPath || _scriptPath == "")
        {
            _scriptPath = Application.dataPath;
        }
        SaveEditorPrefs();
    }
    private void getSelectedTextures()
    {
        _iconPaths.Clear();
        _iconsToProcess = 0;
        for(int i = 0; i < Selection.objects.Length; i++)
        {
            if(AssetDatabase.Contains(Selection.objects[i]))
            {
                string assetPath = AssetDatabase.GetAssetPath(Selection.objects[i]);
                string extension = Path.GetExtension(assetPath);
                if(extension.Length > 1)
                {
                    extension = extension.Substring(1);
                    List<string> extensions = new List<string>{"BMP","EXR","GIF","HDR","IFF","JPG","JPEG","PICT","PNG","PSD","TGA","TIFF"};
                    if(extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                    {
                        _iconsToProcess ++;
                        _iconPaths.Add(assetPath);
                    }
                }
                

            }
        }
        Repaint();
    }
    
    private string setFilePath(string filePath, string text, string extension)
    {
        if (File.Exists(filePath))
        {
            _path = Path.GetDirectoryName(filePath);
        }
        filePath = EditorUtility.OpenFilePanel(text, Path.GetDirectoryName(filePath), extension);
        _path = "";
        return filePath;
    }
    private void RunCMD(string command)
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        process.EnableRaisingEvents = true;
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = command;
        process.StartInfo = startInfo;
        process.Start();
        process.Exited += new EventHandler(onExited);
    }

    private void onExited(object sender, EventArgs e)
    {
        _isExited = true;
    }

    private string AbsolutePath(UnityEngine.Object targetObject)
    {
        string assetPath = AssetDatabase.GetAssetPath(targetObject);
        string applicatioPath = Application.dataPath;
        var index = applicatioPath.LastIndexOf('/');
        applicatioPath = applicatioPath.Substring(0, index);
        assetPath = applicatioPath + "/" + assetPath;
        return assetPath;
    }

    private void GetEditorPrefs()
    {
        _executablePath = EditorPrefs.GetString(prefsExecutablePath, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        _scriptPath = EditorPrefs.GetString(prefsScriptPath, Application.dataPath);
    }

    private void SaveEditorPrefs()
    {
        EditorPrefs.SetString(prefsExecutablePath, _executablePath);
        EditorPrefs.SetString(prefsScriptPath, _scriptPath);
    }

    private void WriteConfig()
    {
        string configJson = JsonUtility.ToJson(config);
        configJson = "config = " + configJson;
        string filePath;
        if(File.Exists(_scriptPath))
        {
            filePath = Path.Combine(Path.GetDirectoryName(_scriptPath), "config.js");
        }
        else
        {
            filePath = Path.Combine(Application.dataPath,"Editor/Resources/config.js");
        }
        File.WriteAllText (filePath, configJson);
        AssetDatabase.Refresh();
    }

    private void ReadConfig()
    {
        string filePath;
        if(File.Exists(_scriptPath))
        {
            filePath = Path.Combine(Path.GetDirectoryName(_scriptPath), "config.js");
        }
        else
        {
            filePath = Path.Combine(Application.dataPath,"Editor/Resources/config.js");
        }

        if(File.Exists(filePath))
        {
            string configAsJson = File.ReadAllText(filePath); 
            configAsJson =  configAsJson.Substring(9);
            config.Load(configAsJson);
        }
        else
        {
            EditorUtility.DisplayDialog("Alert", "No config file found!", "OK");
        }
    }
}

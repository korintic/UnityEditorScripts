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
    private string _configPath = "";

    private bool _showSettingFoldout = true;
    private bool _showIconPathFoldout = false;
    private bool _showPathsFoldout = true;

    private bool _isValidExecutablePath = false;
    private bool _isValidScriptPath = false;
    
    private int _iconsToProcess;
    private List<string> _iconPaths;

    // GUI CONTENT ELEMENT NAMES AND TOOL TIPS
    private GUIContent _processIconsGC;
    private GUIContent _trimGC;
    private GUIContent _toSquareGC;
    private GUIContent _resizeGC;
    private GUIContent _fuzzinessGC;
    private GUIContent _absoluteGC;
    private GUIContent _bySideGC;
    private GUIContent _multiplierGC;
    private GUIContent _selectScriptGC;
    private GUIContent _selectPhotoshopPathGC;


    public class Config
    {
        public bool trim, removeMatte, resize, absolute, bySide, isBySideWidth, isBySideHeight, isMultiplied, toSquare;
        public int width, height, fuzziness, bySideWidth, bySideHeight;
        public float red, green, blue, multiplier;
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
        _processIconsGC = new GUIContent("Process Icons", "Processes icons with Photoshop");
        _trimGC = new GUIContent("Trim", "Trims the icon based on transparent pixels");
        _toSquareGC = new GUIContent("To Square", "Resizes canvas according to longer side \nOperation is done from middle center \nHappens after resizing");
        _absoluteGC = new GUIContent("Absolute","Resize by provided absolute side lengths in pixels");
        _bySideGC = new GUIContent("By Side","Resize by ratio based on the provided side length in pixels");
        _multiplierGC = new GUIContent("Multiplier","Resize by multiplying the side lengths with the provided multiplier");
        _selectScriptGC = new GUIContent("Select Script", "Select path to ExtendScript to run");
        _selectPhotoshopPathGC = new GUIContent("Select Photoshop Path", "Select path to Photoshop.exe");

        Selection.selectionChanged += getSelectedTextures;
        _iconPaths = new List<string>();
        config = new Config();
        config.width = 1;
        config.height = 1;
        config.bySideWidth = 1;
        config.bySideHeight = 1;
        config.multiplier = 1;
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
        if (GUILayout.Button(_processIconsGC, GUILayout.Width(Screen.width / 2 + 4), GUILayout.Height(40)))
        {
            if(Path.GetDirectoryName(_configPath) != Path.GetDirectoryName(_scriptPath))
            {
                File.Copy(_configPath, Path.Combine(Path.GetDirectoryName(_scriptPath),"config.js"), true);
            }
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
                WriteConfig(ref _configPath);
            }

            EditorGUI.BeginDisabledGroup(!config.removeMatte);
            EditorGUI.BeginChangeCheck();
            _matteColor = EditorGUILayout.ColorField("Matte Color", _matteColor, GUILayout.Width(Screen.width / 1.8f));
            if(EditorGUI.EndChangeCheck())
            {
                config.red = _matteColor.r;
                config.green = _matteColor.g;
                config.blue = _matteColor.b;
                WriteConfig(ref _configPath);
            }

            EditorGUI.BeginChangeCheck();
            config.fuzziness = EditorGUILayout.IntField("Matte Fuzziness",config.fuzziness, GUILayout.Width(Screen.width / 1.96f));
            if(EditorGUI.EndChangeCheck())
            {
                config.fuzziness = Mathf.Clamp(config.fuzziness,0,200);
                WriteConfig(ref _configPath);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            config.trim = EditorGUILayout.Toggle(_trimGC, config.trim);
            if(EditorGUI.EndChangeCheck())
            {
                WriteConfig(ref _configPath);
            }
            EditorGUI.BeginChangeCheck();
            config.toSquare = EditorGUILayout.Toggle(_toSquareGC, config.toSquare);
            if(EditorGUI.EndChangeCheck())
            {
                WriteConfig(ref _configPath);
            }

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            config.resize = EditorGUILayout.Toggle("Resize Image", config.resize);
            EditorGUI.BeginDisabledGroup(!config.resize);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_absoluteGC,GUILayout.MaxWidth(55));
            EditorGUI.BeginChangeCheck();
            config.absolute = EditorGUILayout.Toggle( config.absolute, GUILayout.MaxWidth(20));
            GUILayout.Space(10);
            if(EditorGUI.EndChangeCheck() && config.absolute && (config.isMultiplied || config.bySide))
            {
                config.isMultiplied = false;
                config.bySide = false;
            }
            EditorGUILayout.LabelField(_bySideGC,GUILayout.MaxWidth(53));
            EditorGUI.BeginChangeCheck();
            config.bySide = EditorGUILayout.Toggle(config.bySide, GUILayout.MaxWidth(20));
            GUILayout.Space(10);
            if(EditorGUI.EndChangeCheck() && config.bySide && (config.isMultiplied || config.absolute))
            {
                config.isMultiplied = false;
                config.absolute = false;
            }
            EditorGUILayout.LabelField(_multiplierGC, GUILayout.MaxWidth(55));
            EditorGUI.BeginChangeCheck();
            config.isMultiplied = EditorGUILayout.Toggle( config.isMultiplied, GUILayout.MaxWidth(20));
            GUILayout.Space(10);
            if(EditorGUI.EndChangeCheck() && config.isMultiplied && (config.absolute || config.bySide))
            {
                config.bySide = false;
                config.absolute = false;
            }
            EditorGUILayout.EndHorizontal();
            if(EditorGUI.EndChangeCheck())
            {
                WriteConfig(ref _configPath);
            }

            if(!config.resize || (!config.absolute && !config.bySide && !config.isMultiplied))
            {
                GUILayout.Space(41f);
            }
            if(config.absolute && config.resize)
            {
                GUILayout.Space(5f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Width", GUILayout.MaxWidth(146));
                EditorGUI.BeginChangeCheck();
                config.width = EditorGUILayout.IntField(config.width, GUILayout.MaxWidth(70));
                if(EditorGUI.EndChangeCheck())
                {
                    config.width = Mathf.Max(config.width, 1);
                    WriteConfig(ref _configPath);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height", GUILayout.MaxWidth(146));
                EditorGUI.BeginChangeCheck();
                config.height = EditorGUILayout.IntField(config.height, GUILayout.MaxWidth(70));
                if(EditorGUI.EndChangeCheck())
                {
                    config.height = Mathf.Max(config.height, 1);
                    WriteConfig(ref _configPath);
                }
                EditorGUILayout.EndHorizontal();
            }
            if(config.bySide && config.resize)
            {
                GUILayout.Space(5f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Width", GUILayout.MaxWidth(40));
                EditorGUI.BeginChangeCheck();
                config.isBySideWidth = EditorGUILayout.Toggle(config.isBySideWidth ,GUILayout.MaxWidth(20));
                if(EditorGUI.EndChangeCheck() && config.isBySideWidth)
                {
                    config.isBySideHeight = false;
                }
                GUILayout.Space(82);
                EditorGUI.BeginDisabledGroup(!config.isBySideWidth);
                EditorGUI.BeginChangeCheck();
                config.bySideWidth = EditorGUILayout.IntField(config.bySideWidth, GUILayout.MaxWidth(70));
                if(EditorGUI.EndChangeCheck())
                {
                    config.bySideWidth = Mathf.Max(config.bySideWidth, 1);
                    WriteConfig(ref _configPath);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Height", GUILayout.MaxWidth(40));
                config.isBySideHeight = EditorGUILayout.Toggle(config.isBySideHeight, GUILayout.MaxWidth(20));
                if(EditorGUI.EndChangeCheck() && config.isBySideHeight)
                {
                    config.isBySideWidth = false;
                }
                GUILayout.Space(82);
                EditorGUI.BeginDisabledGroup(!config.isBySideHeight);
                EditorGUI.BeginChangeCheck();
                config.bySideHeight = EditorGUILayout.IntField(config.bySideHeight, GUILayout.MaxWidth(70));
                if(EditorGUI.EndChangeCheck())
                {
                    config.bySideHeight = Mathf.Max(config.bySideHeight, 1);
                    WriteConfig(ref _configPath);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            if(config.isMultiplied && config.resize)
            {
                GUILayout.Space(5f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Multiplier", GUILayout.MaxWidth(146));
                EditorGUI.BeginChangeCheck();
                config.multiplier = EditorGUILayout.FloatField(config.multiplier, GUILayout.MaxWidth(70));
                if(EditorGUI.EndChangeCheck())
                {
                    config.multiplier = Mathf.Clamp(config.multiplier,0.01f,100);
                    WriteConfig(ref _configPath);
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(18f);
            }
            EditorGUI.EndDisabledGroup();
        }

        GUI.enabled = true;
        GUILayout.Space(20f);
        _showPathsFoldout = EditorGUILayout.Foldout(_showPathsFoldout, "Paths:");
        if (_showPathsFoldout)
        {
            EditorGUILayout.Space();
            GUI.enabled = false;
            EditorGUILayout.LabelField(_executablePath);
            GUI.enabled = true;
            if (GUILayout.Button(_selectPhotoshopPathGC))
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
            if (GUILayout.Button(_selectScriptGC))
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

    private void WriteConfig(ref string filePath)
    {
        string configJson = JsonUtility.ToJson(config);
        configJson = "config = " + configJson;
        if(File.Exists(_scriptPath))
        {
            filePath = Path.Combine(Path.GetDirectoryName(_scriptPath), "config.js");
        }
        else
        {
            filePath = Path.Combine(Path.Combine(Application.dataPath,"Editor"),"Resources");
            if(!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
                AssetDatabase.Refresh();
            }
            filePath = Path.Combine(filePath, "config.js");
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
            if(EditorUtility.DisplayDialog("No Config File Found", "Do you want to write default config in editor resources folder?", "OK", "CANCEL"))
            {
                WriteConfig(ref _configPath);
            }
        }
    }
}

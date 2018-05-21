using UnityEditor;

public class CaptureWithHotkey : Capture
{
    [MenuItem("Tools/Capture %w")]
    static void Capture()
    {
        CaptureWithHotkey captureWithHotkey = CreateInstance<CaptureWithHotkey>();

        captureWithHotkey.DoCapture();
    }
}
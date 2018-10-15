# Unity Editor Scripts
Collection of simple Unity editor scripts written to improve workflow and learn C# and Unity editor scripting at the same time.

## Editor Scripts
These scripts work when they are located in an Editor folder in a Unity project.

### Capture
For capturing screenshots. 
Uses camera with MainCamera tag.
Can take oversized screenshots, capture transparency and capture from Scene View.
Menu item is located under Tools menu.

![Capture Menu](https://github.com/korintic/UnityEditorScripts/blob/master/Images/CaptureMenu.png "Capture.cs and CaptureWithHotkey.cs")

Capture Options menu item is for changing preferences and taking screencaptures.
Capture is for taking screencaptures with a hotkey using the current preferences set.

![Capture Options](https://github.com/korintic/UnityEditorScripts/blob/master/Images/CaptureOptions.png "Capture.cs")

**Hotkey** Capture Options Ctrl+Shift+W

**Hotkey** Capture Ctrl+W

TO DO:
- [ ] See if the code can be simplified/ cleaned up
- [ ] Reorganize layout groups
- [ ] Add possibility to choose camera
- [x] Reorganize where adding the file extension happens 
- [x] Make a version that captures screenshots with a hotkey based on the preferences set
- [x] Add option to capture from SceneView
- [ ] Add check if images are being saved inside the project

### ToggleSelection
For toggling object selection between none and all.
If something is selected deselects it and if nothing is selected selects everything.
Menu item is located under the Edit menu.

**Hotkey** Ctrl+Shift+A

### SceneViewParent
For parenting objects while working in the SceneView.
Parents selected objects to the active object when SceneView is the active view.

**Hotkey** Ctrl+F

### Lock Transforms
For locking transforms of selected objects.
Made mainly to ease working with UI elements so that it is possible to transform the parent object without affecting the children.

![LockTransforms](https://github.com/korintic/UnityEditorScripts/blob/master/Images/LockTransforms.png "LockTransforms.cs")

TO DO:
- [ ] Add proper support for Undo

### Snapshot Mesh
Work in progress version of an editor script for taking snapshots of skinned meshes.

![Snapshot Mesh](https://github.com/korintic/UnityEditorScripts/blob/master/Images/SnapshotMesh.png "SnapshotMesh.cs")

TO DO:
- [ ] Clean up/ simplify code
- [ ] Add support for material duplication when keep rig selected
- [ ] Add Editorprefs support
- [ ] Rethink naming of new assets
- [ ] Check if assets paths are valid before doing any operations
- [ ] Study how to properly deleted script components that are depended on each other
- [ ] Make relationship of preferences obvious (which ones are relevant in which cases)
- [ ] Add support for Undo

### Control RectTransform
For bypassing the Unity bug that sometimes recttransform fields stay locked if you delete a layout element or group component.
Issue tracker shows that this bug should be fixed by Unity in 2018.3 beta but this editor script offers a workaround for other versions of Unity.
Target Transform fields updates the transforms imidiately and Source Transform allows for setting specific values before applying them.

![Control RectTransform](https://github.com/korintic/UnityEditorScripts/blob/master/Images/ControlRectTransform.png "ControlRectTransform.cs")

Note: The anchor preset window is not functional on the custom editor window.\
Note: Another possible approach for bypassing this bug would have been to rebuild the affected gameobject to get the fields unlocked.

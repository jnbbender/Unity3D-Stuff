using UnityEngine;

using UnityEditor;



public static class GroupWizard 
{
    [MenuItem("PBG Tools/Group Selected %g")]
    private static void GroupSelected() 
    {
        if (!Selection.activeTransform)
            return;
        var go = new GameObject(Selection.activeTransform.name + " Group");

        Undo.RegisterCreatedObjectUndo(go, "Group Selected");
        go.transform.SetParent(Selection.activeTransform.parent, false);

        foreach (var transform in Selection.transforms)
            Undo.SetTransformParent(transform, go.transform, "Group Selected");
        Selection.activeGameObject = go;
    }
}

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class SerializationHelper
{
    // Source: https://docs.unity3d.com/ScriptReference/SerializationUtility.ClearAllManagedReferencesWithMissingTypes.html
    [MenuItem("Adrian Miasik/Serialization Helper/Clear ScriptableObject References with Missing Types")]
    public static void ClearMissingTypesOnScriptableObjects()
    {
        var report = new StringBuilder();

        var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] {"Assets"});
        foreach (string guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            Object obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj != null)
            {
                if (SerializationUtility.ClearAllManagedReferencesWithMissingTypes(obj))
                {
                    report.Append("Cleared missing types from ").Append(path).AppendLine();
                }
                else
                {
                    report.Append("No missing types to clear on ").Append(path).AppendLine();
                }
            }
        }

        Debug.Log(report.ToString());
    }

    public static void PrintGlobalObjectIDs(List<Object> objects)
    {
        foreach (Object o in objects)
        {
            GlobalObjectId globalID = GlobalObjectId.GetGlobalObjectIdSlow(o);
            Debug.Log($"Object: {o}, GlobalObjectID: {globalID}", o);
        }
    }
}
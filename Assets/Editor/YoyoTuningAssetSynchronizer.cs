using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class YoyoTuningAssetSynchronizer
{
    static YoyoTuningAssetSynchronizer()
    {
        EditorApplication.delayCall += SynchronizeAssets;
    }

    [MenuItem("Yooooooooo/Synchronize Yoyo Tuning Defaults")]
    public static void SynchronizeAssets()
    {
        string[] guids = AssetDatabase.FindAssets("t:YoyoTuning");
        bool changed = false;
        YoyoTuning codeDefaults = ScriptableObject.CreateInstance<YoyoTuning>();
        FieldInfo[] tuningFields = typeof(YoyoTuning).GetFields(
            BindingFlags.Instance | BindingFlags.Public);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            YoyoTuning tuning = AssetDatabase.LoadAssetAtPath<YoyoTuning>(path);
            if (tuning == null || !tuning.automaticallySyncCodeDefaults)
                continue;

            string before = EditorJsonUtility.ToJson(tuning);
            foreach (FieldInfo field in tuningFields)
            {
                if (!field.IsNotSerialized)
                    field.SetValue(tuning, field.GetValue(codeDefaults));
            }
            string after = EditorJsonUtility.ToJson(tuning);
            if (before == after)
                continue;

            EditorUtility.SetDirty(tuning);
            changed = true;
            Debug.Log($"Synchronized YoyoTuning code defaults to '{path}'.", tuning);
        }

        Object.DestroyImmediate(codeDefaults);

        if (changed)
            AssetDatabase.SaveAssets();
    }
}

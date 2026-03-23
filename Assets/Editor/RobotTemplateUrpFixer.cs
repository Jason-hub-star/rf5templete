#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RobotTemplate.Editor
{
    public static class RobotTemplateUrpFixer
    {
        private const string SettingsFolder = "Assets/Settings";
        private const string UrpFolder = "Assets/Settings/URP";
        private const string RendererPath = "Assets/Settings/URP/RobotTemplate-Renderer.asset";
        private const string PipelinePath = "Assets/Settings/URP/RobotTemplate-URP.asset";

        [MenuItem("RobotTemplate/Fix URP Pipeline")]
        public static void Fix()
        {
            EnsureFolder(SettingsFolder);
            EnsureFolder(UrpFolder);

            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                renderer.name = "RobotTemplate-Renderer";
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.name = "RobotTemplate-URP";
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            ApplyPipelineToQualitySettings(pipeline);

            EditorUtility.SetDirty(pipeline);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[RobotTemplate] URP pipeline fixed. Renderer='{RendererPath}', Pipeline='{PipelinePath}'.");
        }

        private static void ApplyPipelineToQualitySettings(UniversalRenderPipelineAsset pipeline)
        {
            var qualityAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset").FirstOrDefault();
            if (qualityAsset == null)
            {
                Debug.LogWarning("[RobotTemplate] QualitySettings.asset not found. Graphics pipeline was still updated.");
                return;
            }

            var serializedObject = new SerializedObject(qualityAsset);
            var qualitySettings = serializedObject.FindProperty("m_QualitySettings");
            if (qualitySettings == null || !qualitySettings.isArray)
            {
                Debug.LogWarning("[RobotTemplate] Could not access quality settings array.");
                return;
            }

            for (var i = 0; i < qualitySettings.arraySize; i++)
            {
                var entry = qualitySettings.GetArrayElementAtIndex(i);
                var customRenderPipeline = entry.FindPropertyRelative("customRenderPipeline");
                if (customRenderPipeline != null)
                {
                    customRenderPipeline.objectReferenceValue = pipeline;
                }
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            var parent = System.IO.Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            var folderName = System.IO.Path.GetFileName(assetPath);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
#endif

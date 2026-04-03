#if UNITY_EDITOR
using System.IO;
using KineTutor3D.App;
using KineTutor3D.Visualization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

namespace KineTutor3D.Editor
{
    public static class FR5EndEffectorSetupTool
    {
        private const string ExternalStlPath = @"C:\Users\ezen601\Documents\카카오톡 받은 파일\PGEA-100-40.stl";
        private const string ExternalFbxPath = @"C:\Users\ezen601\Documents\카카오톡 받은 파일\PGEA-100-40.fbx";
        private const string ExternalStepPath = @"C:\Users\ezen601\Documents\카카오톡 받은 파일\PGEA-100-40-W-F_V1.0_3D_20241226.STEP";

        private const string ProjectStlFolder = "Assets/Runtime/EndEffectors/PGEA_100_40/Source";
        private const string ProjectStlPath = ProjectStlFolder + "/PGEA-100-40.stl";
        private const string ProjectFbxPath = ProjectStlFolder + "/PGEA-100-40.fbx";
        private const string ArchiveStepFolder = "archive/EndEffectors/PGEA_100_40/CAD";
        private const string ArchiveStepPath = ArchiveStepFolder + "/PGEA-100-40-W-F_V1.0_3D_20241226.STEP";

        private const string EndEffectorResourceFolder = "Assets/Runtime/Resources/EndEffectors";
        private const string EndEffectorPrefabPath = EndEffectorResourceFolder + "/PGEA_100_40.prefab";
        private const string EndEffectorMaterialPath = EndEffectorResourceFolder + "/PGEA_100_40_Visual.mat";
        private const string ExistingRobotMaterialPath = "Assets/Runtime/Robots/FAIRINO_FR5/Materials/rgba-0.89804-0.91765-0.92941-1.mat";
        private const string ControlVariantPath = "Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control_PGEA10040.prefab";
        private const string PreviewVariantPath = "Assets/Runtime/Resources/Robots/FAIRINO_FR5_PGEA10040.prefab";

        private const string AttachmentId = "PGEA_100_40";
        private const string ToolMountName = "ToolMount";
        private const float MillimetersToMeters = 0.001f;
        // ToolMount stays at identity — rotation is applied to the end effector instance.
        private static readonly Quaternion DefaultAttachmentRotation = Quaternion.identity;
        // FBX 기반: 원점이 메쉬 중심 근처(XY≈0), Y축=길이방향, Z=-3.256 오프셋.
        // FBX는 STL과 달리 편심이 거의 없으므로 최소 보정만 필요.
        private static readonly Quaternion EndEffectorLocalRotation = Quaternion.identity;
        private static readonly Vector3 EndEffectorLocalPosition = Vector3.zero;
        private const bool UseFbxSource = true;

        [MenuItem("RobotTemplate/End Effector/Install PGEA-100-40 On FR5", priority = 120)]
        public static void InstallPgea10040Menu()
        {
            var summary = InstallOrRefresh();
            Debug.Log(summary);
            EditorUtility.DisplayDialog("FR5 End Effector Setup", summary, "OK");
        }

        public static void InstallPgea10040Batch()
        {
            Debug.Log(InstallOrRefresh());
        }

        private static string InstallOrRefresh()
        {
            CopyExternalFilesIntoProject();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            GameObject modelAsset = null;

            if (UseFbxSource && File.Exists(ProjectFbxPath))
            {
                AssetDatabase.ImportAsset(ProjectFbxPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectFbxPath);
            }

            if (modelAsset == null)
            {
                StlAssetPostProcessor.PostprocessStlFile(ProjectStlPath);
                AssetDatabase.ImportAsset(ProjectStlPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(StlAssetPostProcessor.GetPrefabAssetPath(ProjectStlPath));
            }

            if (modelAsset == null)
            {
                return $"[FR5 End Effector] Model import failed. Checked FBX at '{ProjectFbxPath}' and STL at '{ProjectStlPath}'.";
            }

            EnsureFolder(EndEffectorResourceFolder);
            EnsureEndEffectorPrefab(modelAsset);
            EnsureRobotVariant(FR5TemplateSlimManifest.ControlPrefabAssetPath, ControlVariantPath);
            EnsureRobotVariant(FR5TemplateSlimManifest.PreviewPrefabAssetPath, PreviewVariantPath);
            SaveDemoSceneIfPresent();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var source = UseFbxSource && File.Exists(ProjectFbxPath) ? "FBX" : "STL";
            return $"[FR5 End Effector] Installed PGEA-100-40 from {source}, refreshed FR5 control/preview prefabs with shared TCP attachment rig.";
        }

        private static void CopyExternalFilesIntoProject()
        {
            EnsureFolder(ProjectStlFolder);
            EnsureFileFolder(ArchiveStepPath);

            if (File.Exists(ExternalFbxPath))
            {
                File.Copy(ExternalFbxPath, ProjectFbxPath, true);
            }

            if (File.Exists(ExternalStlPath))
            {
                File.Copy(ExternalStlPath, ProjectStlPath, true);
            }

            if (File.Exists(ExternalStepPath))
            {
                File.Copy(ExternalStepPath, ArchiveStepPath, true);
            }
        }

        private static void EnsureEndEffectorPrefab(GameObject modelAsset)
        {
            var root = new GameObject(AttachmentId);
            var attachment = root.AddComponent<FR5EndEffectorAttachment>();
            var visualMaterial = EnsureVisualMaterial();

            var visualRoot = new GameObject("VisualRoot").transform;
            visualRoot.SetParent(root.transform, false);

            var modelInstance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (modelInstance != null)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(modelInstance))
                {
                    PrefabUtility.UnpackPrefabInstance(modelInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

                modelInstance.name = "PGEA-100-40_Model";
                modelInstance.transform.SetParent(visualRoot, false);
                modelInstance.transform.localRotation = Quaternion.identity;
                if (UseFbxSource)
                {
                    // FBX: Unity importer가 스케일 처리, 원점이 메쉬 중심 근처
                    modelInstance.transform.localScale = Vector3.one;
                    modelInstance.transform.localPosition = Vector3.zero;
                }
                else
                {
                    // STL: mm 단위, 원점이 비표준
                    modelInstance.transform.localScale = Vector3.one * MillimetersToMeters;
                    modelInstance.transform.localPosition = ComputeMountAlignedOffset(modelInstance);
                }
                ApplyVisualMaterial(modelInstance, visualMaterial);
            }

            var tcpFrame = new GameObject("TcpFrame").transform;
            tcpFrame.SetParent(root.transform, false);
            tcpFrame.localPosition = Vector3.zero;
            tcpFrame.localRotation = Quaternion.identity;

            var tcpMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tcpMarker.name = "TcpMarker";
            tcpMarker.transform.SetParent(tcpFrame, false);
            tcpMarker.transform.localScale = Vector3.one * 0.012f;
            tcpMarker.transform.localPosition = Vector3.zero;

            var collider = tcpMarker.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            var renderer = tcpMarker.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = new Color(0.20f, 0.95f, 0.65f, 0.95f)
                };
                renderer.sharedMaterial = material;
            }

            attachment.Configure(AttachmentId, visualRoot, tcpFrame, null);
            PrefabUtility.SaveAsPrefabAsset(root, EndEffectorPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static Material EnsureVisualMaterial()
        {
            var robotMaterial = AssetDatabase.LoadAssetAtPath<Material>(ExistingRobotMaterialPath);
            if (robotMaterial != null)
            {
                return robotMaterial;
            }

            var existing = AssetDatabase.LoadAssetAtPath<Material>(EndEffectorMaterialPath);
            if (existing != null)
            {
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader)
            {
                color = new Color(0.29f, 0.31f, 0.35f, 1f)
            };

            AssetDatabase.CreateAsset(material, EndEffectorMaterialPath);
            return material;
        }

        private static void ApplyVisualMaterial(GameObject root, Material material)
        {
            if (root == null || material == null)
            {
                return;
            }

            var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].sharedMaterial = material;
            }
        }

        private static Vector3 ComputeMountAlignedOffset(GameObject modelInstance)
        {
            if (modelInstance == null)
            {
                return Vector3.zero;
            }

            var meshFilters = modelInstance.GetComponentsInChildren<MeshFilter>(true);
            if (meshFilters == null || meshFilters.Length == 0)
            {
                return Vector3.zero;
            }

            var hasBounds = false;
            var min = Vector3.zero;
            var max = Vector3.zero;

            for (var i = 0; i < meshFilters.Length; i++)
            {
                var filter = meshFilters[i];
                if (filter == null || filter.sharedMesh == null)
                {
                    continue;
                }

                var meshBounds = filter.sharedMesh.bounds;
                var corners = GetBoundsCorners(meshBounds);
                for (var c = 0; c < corners.Length; c++)
                {
                    var worldCorner = filter.transform.TransformPoint(corners[c]);
                    var localCorner = modelInstance.transform.InverseTransformPoint(worldCorner);

                    if (!hasBounds)
                    {
                        min = localCorner;
                        max = localCorner;
                        hasBounds = true;
                        continue;
                    }

                    min = Vector3.Min(min, localCorner);
                    max = Vector3.Max(max, localCorner);
                }
            }

            if (!hasBounds)
            {
                return Vector3.zero;
            }

            var center = (min + max) * 0.5f;
            // Align the flange-side face of the STL to the ToolMount origin and center X/Y.
            return new Vector3(-center.x, -center.y, -max.z) * MillimetersToMeters;
        }

        private static Vector3[] GetBoundsCorners(Bounds bounds)
        {
            var min = bounds.min;
            var max = bounds.max;
            return new[]
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };
        }

        private static void EnsureRobotVariant(string sourcePrefabPath, string variantPrefabPath)
        {
            var endEffectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EndEffectorPrefabPath);
            if (endEffectorPrefab == null)
            {
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(sourcePrefabPath);
            if (root == null)
            {
                return;
            }

            try
            {
                var baseFrame = FindChildRecursive(root.transform, "base_link");
                var mountParent = FindChildRecursive(root.transform, "wrist3_link") ?? root.transform;
                var toolMount = mountParent.Find(ToolMountName);
                if (toolMount == null)
                {
                    toolMount = new GameObject(ToolMountName).transform;
                    toolMount.SetParent(mountParent, false);
                }

                toolMount.localPosition = Vector3.zero;
                toolMount.localRotation = DefaultAttachmentRotation;

                var existing = toolMount.Find(AttachmentId);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                }

                var instance = PrefabUtility.InstantiatePrefab(endEffectorPrefab, toolMount) as GameObject;
                if (instance != null)
                {
                    instance.name = AttachmentId;
                    instance.transform.localPosition = EndEffectorLocalPosition;
                    instance.transform.localRotation = EndEffectorLocalRotation;
                    instance.transform.localScale = Vector3.one;

                    var attachment = instance.GetComponent<FR5EndEffectorAttachment>();
                    if (attachment != null)
                    {
                        attachment.SetBaseFrame(baseFrame);
                        EditorUtility.SetDirty(attachment);
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(root, variantPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void SaveDemoSceneIfPresent()
        {
            if (!File.Exists(FR5TemplateSlimManifest.DemoScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(FR5TemplateSlimManifest.DemoScenePath, OpenSceneMode.Single);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            var direct = parent.Find(childName);
            if (direct != null)
            {
                return direct;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var found = FindChildRecursive(parent.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            var folderName = Path.GetFileName(assetPath);
            if (!string.IsNullOrWhiteSpace(parent) && !string.IsNullOrWhiteSpace(folderName))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static void EnsureFileFolder(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
#endif

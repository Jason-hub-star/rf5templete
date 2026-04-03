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
        private const string ProjectStlFolder = "Assets/Runtime/EndEffectors/PGEA_100_40/Source";
        private const string ProjectBodyStlPath = ProjectStlFolder + "/PGEA-100-40_body.stl";
        private const string ProjectFingerLeftStlPath = ProjectStlFolder + "/PGEA-100-40_finger_left.stl";
        private const string ProjectFingerRightStlPath = ProjectStlFolder + "/PGEA-100-40_finger_right.stl";

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
        // Bootstrap defaults are only used before a tuned Control prefab exists.
        private static readonly Quaternion BootstrapEndEffectorLocalRotation = Quaternion.Euler(0f, 0f, -91.6f);
        private static readonly Vector3 BootstrapEndEffectorLocalPosition = new Vector3(0.003f, 0.1676f, 0.031f);
        private static readonly Vector3 BootstrapTcpLocalPosition = new Vector3(-0.0811f, 0f, -0.0325f);
        private const float BootstrapModelLocalZ = -0.031f;
        private static readonly string[] PartStlPaths = { ProjectBodyStlPath, ProjectFingerLeftStlPath, ProjectFingerRightStlPath };
        private static readonly string[] PartNames = { "body", "finger_left", "finger_right" };

        [MenuItem("RobotTemplate/End Effector/Sync Preview Variant From Control", priority = 121)]
        public static void SyncPreviewVariantFromControlMenu()
        {
            var summary = SyncPreviewVariantFromControl();
            Debug.Log(summary);
            EditorUtility.DisplayDialog("FR5 End Effector Sync", summary, "OK");
        }

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
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // Import 3 STL parts (body, finger_left, finger_right)
            var partAssets = new GameObject[PartStlPaths.Length];
            for (var i = 0; i < PartStlPaths.Length; i++)
            {
                var stlPath = PartStlPaths[i];
                if (!File.Exists(stlPath))
                {
                    return $"[FR5 End Effector] Missing STL: {stlPath}";
                }

                StlAssetPostProcessor.PostprocessStlFile(stlPath);
                AssetDatabase.ImportAsset(stlPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                partAssets[i] = AssetDatabase.LoadAssetAtPath<GameObject>(StlAssetPostProcessor.GetPrefabAssetPath(stlPath));
                if (partAssets[i] == null)
                {
                    return $"[FR5 End Effector] Import failed for {stlPath}";
                }
            }

            EnsureFolder(EndEffectorResourceFolder);
            EnsureEndEffectorPrefab(partAssets);
            var existingControlAttachment = LoadVariantAttachment(ControlVariantPath);
            EnsureRobotVariant(FR5TemplateSlimManifest.ControlPrefabAssetPath, ControlVariantPath, existingControlAttachment);
            var controlAttachment = LoadVariantAttachment(ControlVariantPath);
            EnsureRobotVariant(FR5TemplateSlimManifest.PreviewPrefabAssetPath, PreviewVariantPath, controlAttachment);
            SaveDemoSceneIfPresent();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            return "[FR5 End Effector] Installed PGEA-100-40 (3-part: body + finger_left + finger_right), refreshed FR5 control/preview prefabs.";
        }

        public static string SyncPreviewVariantFromControl()
        {
            var controlAttachment = LoadVariantAttachment(ControlVariantPath);
            if (controlAttachment == null)
            {
                return "[FR5 End Effector] Control variant attachment not found. Save FAIRINO_FR5_Control_PGEA10040 first, then sync preview.";
            }

            EnsureRobotVariant(FR5TemplateSlimManifest.PreviewPrefabAssetPath, PreviewVariantPath, controlAttachment);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return "[FR5 End Effector] Preview variant synced from FAIRINO_FR5_Control_PGEA10040.";
        }

        private static void EnsureEndEffectorPrefab(GameObject[] partAssets)
        {
            var root = new GameObject(AttachmentId);
            var attachment = root.AddComponent<FR5EndEffectorAttachment>();
            var visualMaterial = EnsureVisualMaterial();

            var visualRoot = new GameObject("VisualRoot").transform;
            visualRoot.SetParent(root.transform, false);

            var modelRoot = new GameObject("PGEA-100-40_Model").transform;
            modelRoot.SetParent(visualRoot, false);
            modelRoot.localScale = Vector3.one * MillimetersToMeters;

            Transform fingerLeftTransform = null;
            Transform fingerRightTransform = null;

            for (var i = 0; i < partAssets.Length; i++)
            {
                var partInstance = PrefabUtility.InstantiatePrefab(partAssets[i]) as GameObject;
                if (partInstance == null)
                {
                    continue;
                }

                if (PrefabUtility.IsPartOfAnyPrefab(partInstance))
                {
                    PrefabUtility.UnpackPrefabInstance(partInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

                partInstance.name = PartNames[i];
                partInstance.transform.SetParent(modelRoot, false);
                partInstance.transform.localPosition = Vector3.zero;
                partInstance.transform.localRotation = Quaternion.identity;
                partInstance.transform.localScale = Vector3.one;
                ApplyVisualMaterial(partInstance, visualMaterial);

                if (PartNames[i] == "finger_left")
                {
                    fingerLeftTransform = partInstance.transform;
                }
                else if (PartNames[i] == "finger_right")
                {
                    fingerRightTransform = partInstance.transform;
                }
            }

            // Compute mount-aligned offset using combined bounds of all parts
            modelRoot.localPosition = ComputeMountAlignedOffset(modelRoot.gameObject);

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
            attachment.SetFingers(fingerLeftTransform, fingerRightTransform);
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

        private static FR5EndEffectorAttachment LoadVariantAttachment(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            return prefab == null
                ? null
                : FR5EndEffectorAttachmentPoseSync.FindAttachment(prefab.transform, AttachmentId);
        }

        private static void EnsureRobotVariant(
            string sourcePrefabPath,
            string variantPrefabPath,
            FR5EndEffectorAttachment referenceAttachment)
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

                    var attachment = instance.GetComponent<FR5EndEffectorAttachment>();
                    if (attachment != null)
                    {
                        attachment.EnsureStructure();
                        ApplyVariantAttachmentPose(instance.transform, attachment, referenceAttachment);
                        attachment.SetBaseFrame(baseFrame);
                        EditorUtility.SetDirty(instance.transform);
                        EditorUtility.SetDirty(attachment.TcpFrame);
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

        private static void ApplyVariantAttachmentPose(
            Transform attachmentRoot,
            FR5EndEffectorAttachment targetAttachment,
            FR5EndEffectorAttachment referenceAttachment)
        {
            if (targetAttachment == null || attachmentRoot == null)
            {
                return;
            }

            if (FR5EndEffectorAttachmentPoseSync.CopyPose(referenceAttachment, targetAttachment))
            {
                return;
            }

            attachmentRoot.localPosition = BootstrapEndEffectorLocalPosition;
            attachmentRoot.localRotation = BootstrapEndEffectorLocalRotation;
            attachmentRoot.localScale = Vector3.one;

            if (targetAttachment.TcpFrame != null)
            {
                targetAttachment.TcpFrame.localPosition = BootstrapTcpLocalPosition;
                targetAttachment.TcpFrame.localRotation = Quaternion.identity;
            }

            var modelRoot = targetAttachment.VisualRoot != null
                ? targetAttachment.VisualRoot.Find("PGEA-100-40_Model")
                : null;
            if (modelRoot != null)
            {
                var localPosition = modelRoot.localPosition;
                localPosition.z = BootstrapModelLocalZ;
                modelRoot.localPosition = localPosition;
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

// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App
{
    /// <summary>
    /// FR5 슬림 템플릿 추출에 필요한 경로와 출력 규약을 정의합니다.
    /// </summary>
    public static class FR5TemplateSlimManifest
    {
        public const string DemoScenePath = "Assets/Scenes/FR5_Template_Demo.unity";
        public const string RuntimeAsmdefPath = "Assets/Scripts/KineTutor3D.Runtime.asmdef";
        public const string ControlPrefabResourcePath = "Robots/FAIRINO_FR5_Control";
        public const string ControlPrefabWithEndEffectorResourcePath = "Robots/FAIRINO_FR5_Control_PGEA10040";
        public const string PreviewPrefabResourcePath = "Robots/FAIRINO_FR5";
        public const string PreviewPrefabWithEndEffectorResourcePath = "Robots/FAIRINO_FR5_PGEA10040";
        public const string EndEffectorPrefabResourcePath = "EndEffectors/PGEA_100_40";
        public const string PreviewPrefabAssetPath = "Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab";
        public const string ControlPrefabAssetPath = "Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab";
        public const string ControlPrefabWithEndEffectorAssetPath = "Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control_PGEA10040.prefab";
        public const string PreviewPrefabWithEndEffectorAssetPath = "Assets/Runtime/Resources/Robots/FAIRINO_FR5_PGEA10040.prefab";
        public const string PreviewMaterialAssetPath = "Assets/Runtime/Resources/Robots/FAIRINO_FR5_Preview.mat";
        public const string EndEffectorPrefabAssetPath = "Assets/Runtime/Resources/EndEffectors/PGEA_100_40.prefab";
        public const string EndEffectorSourceFolder = "Assets/Runtime/EndEffectors/PGEA_100_40";
        public const string RuntimeRobotFolder = "Assets/Runtime/Robots/FAIRINO_FR5";
        public const string ExternalRoot = ".";
        public const string UnityPackageName = "FAIRINO_FR5_TEMPLATE_Slim.unitypackage";
        public const string ZipName = "FAIRINO_FR5_TEMPLATE_Slim.zip";
        public const string EvidenceFolderName = "evidence";

        private static readonly string[] PackageRootsInternal =
        {
            DemoScenePath,
            RuntimeAsmdefPath,
            "Assets/Scripts/App/FR5TemplateMinimalController.cs",
            "Assets/Scripts/App/FR5TemplatePoseCatalog.cs",
            "Assets/Scripts/App/FR5TemplateSlimManifest.cs",
            "Assets/Scripts/App/RobotKinematicsFacade.cs",
            "Assets/Scripts/Math",
            "Assets/Scripts/Kinematics",
            "Assets/Scripts/Templates/KineTutor3D.Templates.asmdef",
            "Assets/Scripts/Templates/TemplateFAIRINO_FR5.cs",
            "Assets/Scripts/Types",
            "Assets/Scripts/Visualization/FairinoUrdfJointDriver.cs",
            "Assets/Scripts/Visualization/FR5EndEffectorAttachment.cs",
            "Assets/Scripts/Visualization/FR5EndEffectorAttachmentPoseSync.cs",
            "Assets/Scripts/Visualization/Shared/JointRotationHandle.cs",
            "Assets/Scripts/Visualization/Shared/OrbitCameraController.cs",
            "Assets/Scripts/Visualization/Shared/SharedLineMaterial.cs",
            "Assets/Editor/KineTutor3D.Editor.asmdef",
            "Assets/Editor/FR5EndEffectorAttachmentEditor.cs",
            "Assets/Editor/FR5EndEffectorSetupTool.cs",
            PreviewPrefabAssetPath,
            ControlPrefabAssetPath,
            ControlPrefabWithEndEffectorAssetPath,
            PreviewPrefabWithEndEffectorAssetPath,
            PreviewMaterialAssetPath,
            EndEffectorPrefabAssetPath,
            EndEffectorSourceFolder,
            RuntimeRobotFolder
        };

        /// <summary>
        /// 패키지에 포함할 Asset 경로 목록을 반환합니다.
        /// </summary>
        public static string[] GetPackageRoots()
        {
            return (string[])PackageRootsInternal.Clone();
        }
    }
}

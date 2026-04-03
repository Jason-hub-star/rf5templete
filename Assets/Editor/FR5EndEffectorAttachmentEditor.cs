#if UNITY_EDITOR
using System;
using KineTutor3D.App;
using KineTutor3D.Visualization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KineTutor3D.Editor
{
    [CustomEditor(typeof(FR5EndEffectorAttachment))]
    public sealed class FR5EndEffectorAttachmentEditor : UnityEditor.Editor
    {
        private static EndEffectorReferenceMode referenceMode = EndEffectorReferenceMode.Tool;
        private static float stepMillimeters = 5f;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var attachment = (FR5EndEffectorAttachment)target;
            if (attachment == null)
            {
                return;
            }

            EditorGUILayout.Space();
            if (!attachment.TcpCalibrated)
            {
                EditorGUILayout.HelpBox(
                    "TCP NOT CALIBRATED — 현재 TcpFrame 위치는 시각적 정렬 값이며,\n실기 calibration 후 확정 값으로 교체해야 합니다.",
                    MessageType.Warning);
            }

            EditorGUILayout.LabelField("TCP Tuning", EditorStyles.boldLabel);
            referenceMode = (EndEffectorReferenceMode)EditorGUILayout.EnumPopup("Reference", referenceMode);
            stepMillimeters = EditorGUILayout.FloatField("Step (mm)", Mathf.Max(0.1f, stepMillimeters));

            if (IsEditingControlVariant(attachment))
            {
                EditorGUILayout.HelpBox(
                    "FAIRINO_FR5_Control_PGEA10040가 기준점입니다. Preview는 로드 시 Control 포즈를 따라가고, 아래 버튼으로 preview asset도 다시 맞출 수 있습니다.",
                    MessageType.Info);

                if (GUILayout.Button("Sync Preview Variant From Control"))
                {
                    var summary = FR5EndEffectorSetupTool.SyncPreviewVariantFromControl();
                    Debug.Log(summary);
                }
            }

            EditorGUILayout.HelpBox(
                $"TCP Local Position: {attachment.CurrentTcpLocalPosition}\nTCP Local Euler: {attachment.CurrentTcpLocalEulerAngles}",
                MessageType.Info);

            DrawNudgeRow(attachment, "X", Vector3.right);
            DrawNudgeRow(attachment, "Y", Vector3.up);
            DrawNudgeRow(attachment, "Z", Vector3.forward);

            if (GUILayout.Button("Reset TCP To Origin"))
            {
                Undo.RecordObject(attachment.TcpFrame, "Reset TCP Origin");
                attachment.ResetTcpPose();
                EditorUtility.SetDirty(attachment.TcpFrame);
                EditorUtility.SetDirty(attachment);
                PrefabUtility.RecordPrefabInstancePropertyModifications(attachment.TcpFrame);
                SyncPreviewVariantIfNeeded(attachment);
                SceneView.RepaintAll();
            }
        }

        private void DrawNudgeRow(FR5EndEffectorAttachment attachment, string axisLabel, Vector3 axis)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button($"{axisLabel}-"))
                {
                    Nudge(attachment, -axis);
                }

                if (GUILayout.Button($"{axisLabel}+"))
                {
                    Nudge(attachment, axis);
                }
            }
        }

        private void Nudge(FR5EndEffectorAttachment attachment, Vector3 direction)
        {
            if (attachment == null || attachment.TcpFrame == null)
            {
                return;
            }

            Undo.RecordObject(attachment.TcpFrame, $"Nudge TCP {referenceMode}");
            attachment.NudgeTcp(direction.normalized * (stepMillimeters * 0.001f), referenceMode);
            EditorUtility.SetDirty(attachment.TcpFrame);
            EditorUtility.SetDirty(attachment);
            PrefabUtility.RecordPrefabInstancePropertyModifications(attachment.TcpFrame);
            SyncPreviewVariantIfNeeded(attachment);
            SceneView.RepaintAll();
        }

        private static void SyncPreviewVariantIfNeeded(FR5EndEffectorAttachment attachment)
        {
            if (!IsEditingControlVariant(attachment))
            {
                return;
            }

            Debug.Log(FR5EndEffectorSetupTool.SyncPreviewVariantFromControl());
        }

        private static bool IsEditingControlVariant(FR5EndEffectorAttachment attachment)
        {
            var assetPath = GetOwningPrefabAssetPath(attachment);
            return string.Equals(
                assetPath,
                FR5TemplateSlimManifest.ControlPrefabWithEndEffectorAssetPath,
                StringComparison.Ordinal);
        }

        private static string GetOwningPrefabAssetPath(Component component)
        {
            if (component == null)
            {
                return string.Empty;
            }

            var prefabStage = PrefabStageUtility.GetPrefabStage(component.gameObject);
            if (prefabStage != null)
            {
                return prefabStage.assetPath;
            }

            var nearestRoot = PrefabUtility.GetNearestPrefabInstanceRoot(component.gameObject);
            if (nearestRoot != null)
            {
                return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(nearestRoot);
            }

            return AssetDatabase.GetAssetPath(component.gameObject);
        }
    }
}
#endif

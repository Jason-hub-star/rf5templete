// Folder: Visualization - 3D rendering helpers for robot joint/link display.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    public enum EndEffectorReferenceMode
    {
        Tool = 0,
        Base = 1,
        World = 2
    }

    /// <summary>
    /// FR5에 부착된 엔드이펙터의 visual root와 TCP frame을 한 곳에서 관리합니다.
    /// 팀이 같은 prefab 기준으로 장착 위치와 TCP 오프셋을 조정할 수 있도록 SSOT 역할을 맡습니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FR5EndEffectorAttachment : MonoBehaviour
    {
        [SerializeField] private string attachmentId = "PGEA_100_40";
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform tcpFrame;
        [SerializeField] private Transform baseFrame;
        [SerializeField] private Transform fingerLeft;
        [SerializeField] private Transform fingerRight;
        [SerializeField] private float gizmoAxisLength = 0.05f;
        [SerializeField] private float gizmoSphereRadius = 0.008f;
        [SerializeField] private bool tcpCalibrated;
        [SerializeField, Range(0f, 1f)] private float gripperOpenRatio;

        // Stroke in model-space units (mm). Parent PGEA-100-40_Model has scale 0.001,
        // so 1 localPosition unit = 1mm in world space after scaling.
        private const float StrokeMm = 40f; // 40mm total stroke
        private Vector3 fingerLeftClosed;
        private Vector3 fingerRightClosed;
        private bool fingerBaseCaptured;

        public string AttachmentId => attachmentId;
        public bool TcpCalibrated => tcpCalibrated;
        public Transform VisualRoot => visualRoot;
        public Transform TcpFrame => tcpFrame;
        public Transform BaseFrame => baseFrame;
        public Transform FingerLeft => fingerLeft;
        public Transform FingerRight => fingerRight;
        public float GripperOpenRatio => gripperOpenRatio;

        public Vector3 CurrentTcpLocalPosition => tcpFrame != null ? tcpFrame.localPosition : Vector3.zero;
        public Vector3 CurrentTcpLocalEulerAngles => tcpFrame != null ? tcpFrame.localEulerAngles : Vector3.zero;

        public void Configure(string id, Transform visual, Transform tcp, Transform baseReference)
        {
            attachmentId = string.IsNullOrWhiteSpace(id) ? attachmentId : id;
            visualRoot = visual;
            tcpFrame = tcp;
            baseFrame = baseReference;
        }

        public void SetFingers(Transform left, Transform right)
        {
            fingerLeft = left;
            fingerRight = right;
            fingerBaseCaptured = false;
        }

        public void SetGripperOpen(float ratio)
        {
            gripperOpenRatio = Mathf.Clamp01(ratio);
            ApplyGripperPose();
        }

        public void SetBaseFrame(Transform baseReference)
        {
            baseFrame = baseReference;
        }

        public void SetTcpCalibrated(bool value)
        {
            tcpCalibrated = value;
        }

        public void EnsureStructure()
        {
            if (visualRoot == null)
            {
                var existingVisual = transform.Find("VisualRoot");
                if (existingVisual == null)
                {
                    existingVisual = new GameObject("VisualRoot").transform;
                    existingVisual.SetParent(transform, false);
                }

                visualRoot = existingVisual;
            }

            if (tcpFrame == null)
            {
                var existingTcp = transform.Find("TcpFrame");
                if (existingTcp == null)
                {
                    existingTcp = new GameObject("TcpFrame").transform;
                    existingTcp.SetParent(transform, false);
                }

                tcpFrame = existingTcp;
            }
        }

        private void RefreshExistingReferences()
        {
            visualRoot ??= transform.Find("VisualRoot");
            tcpFrame ??= transform.Find("TcpFrame");

            if (fingerLeft == null || fingerRight == null)
            {
                var model = visualRoot != null ? visualRoot.Find("PGEA-100-40_Model") : null;
                if (model != null)
                {
                    fingerLeft ??= model.Find("finger_left");
                    fingerRight ??= model.Find("finger_right");
                }
            }
        }

        public void ResetTcpPose()
        {
            EnsureStructure();

            tcpFrame.localPosition = Vector3.zero;
            tcpFrame.localRotation = Quaternion.identity;
        }

        public void NudgeTcp(Vector3 deltaMeters, EndEffectorReferenceMode mode)
        {
            EnsureStructure();

            var parent = tcpFrame.parent != null ? tcpFrame.parent : transform;
            var worldDelta = ResolveWorldDelta(deltaMeters, mode);
            var localDelta = parent.InverseTransformVector(worldDelta);

            tcpFrame.localPosition += localDelta;
        }

        private Vector3 ResolveWorldDelta(Vector3 deltaMeters, EndEffectorReferenceMode mode)
        {
            switch (mode)
            {
                case EndEffectorReferenceMode.Tool:
                    return tcpFrame != null ? tcpFrame.TransformVector(deltaMeters) : transform.TransformVector(deltaMeters);
                case EndEffectorReferenceMode.Base:
                    return baseFrame != null ? baseFrame.TransformVector(deltaMeters) : transform.TransformVector(deltaMeters);
                case EndEffectorReferenceMode.World:
                default:
                    return deltaMeters;
            }
        }

        private void CaptureFingerBase()
        {
            if (fingerBaseCaptured || fingerLeft == null || fingerRight == null)
            {
                return;
            }

            // Only capture when ratio is 0 (closed state) to avoid poisoning base position.
            if (gripperOpenRatio > 0.001f)
            {
                return;
            }

            fingerLeftClosed = fingerLeft.localPosition;
            fingerRightClosed = fingerRight.localPosition;
            fingerBaseCaptured = true;
        }

        public void RecaptureFingerBase()
        {
            fingerBaseCaptured = false;
            CaptureFingerBase();
        }

        private void ApplyGripperPose()
        {
            if (fingerLeft == null || fingerRight == null)
            {
                return;
            }

            CaptureFingerBase();

            if (!fingerBaseCaptured)
            {
                return;
            }

            // Fingers slide along local X axis (parallel jaw open direction in model space).
            // StrokeMm is in mm (model-space units); each finger moves half-stroke.
            var halfStroke = StrokeMm * 0.5f * gripperOpenRatio;
            fingerLeft.localPosition = fingerLeftClosed + new Vector3(halfStroke, 0f, 0f);
            fingerRight.localPosition = fingerRightClosed + new Vector3(-halfStroke, 0f, 0f);
        }

        private void OnValidate()
        {
            RefreshExistingReferences();
            ApplyGripperPose();
        }

        private void OnDrawGizmos()
        {
            if (tcpFrame == null)
            {
                return;
            }

            Gizmos.color = tcpCalibrated ? Color.white : Color.yellow;
            Gizmos.DrawWireSphere(tcpFrame.position, gizmoSphereRadius);

            DrawAxis(tcpFrame, Vector3.right, Color.red);
            DrawAxis(tcpFrame, Vector3.up, Color.green);
            DrawAxis(tcpFrame, Vector3.forward, Color.blue);
        }

        private void DrawAxis(Transform axisFrame, Vector3 axis, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(axisFrame.position, axisFrame.position + axisFrame.TransformDirection(axis) * gizmoAxisLength);
        }
    }
}

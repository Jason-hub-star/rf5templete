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
        [SerializeField] private float gizmoAxisLength = 0.05f;
        [SerializeField] private float gizmoSphereRadius = 0.008f;
        [SerializeField] private bool tcpCalibrated;

        public string AttachmentId => attachmentId;
        public bool TcpCalibrated => tcpCalibrated;
        public Transform VisualRoot => visualRoot;
        public Transform TcpFrame => tcpFrame;
        public Transform BaseFrame => baseFrame;

        public Vector3 CurrentTcpLocalPosition => tcpFrame != null ? tcpFrame.localPosition : Vector3.zero;
        public Vector3 CurrentTcpLocalEulerAngles => tcpFrame != null ? tcpFrame.localEulerAngles : Vector3.zero;

        public void Configure(string id, Transform visual, Transform tcp, Transform baseReference)
        {
            attachmentId = string.IsNullOrWhiteSpace(id) ? attachmentId : id;
            visualRoot = visual;
            tcpFrame = tcp;
            baseFrame = baseReference;
        }

        public void SetBaseFrame(Transform baseReference)
        {
            baseFrame = baseReference;
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

        private void OnValidate()
        {
            RefreshExistingReferences();
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

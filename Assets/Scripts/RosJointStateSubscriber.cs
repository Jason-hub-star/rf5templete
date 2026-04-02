// ============================================================
// RosJointStateSubscriber.cs
// ------------------------------------------------------------
// 역할: ROS2의 /joint_states 토픽을 Unity에서 구독하여
//       FR5 로봇 모델의 관절을 실시간으로 동기화합니다.
//
// 연동 흐름:
//   ROS2 /joint_states 퍼블리시
//       → ROS-TCP-Endpoint (노트북 192.168.0.99:10000)
//       → (와이파이)
//       → ROSConnection (Unity ROS-TCP-Connector)
//       → OnJointState() 콜백
//       → FR5TemplateMinimalController.SetJointAnglesDeg()
//       → Unity FR5 모델 관절 동기화
//
// 사전 조건:
//   1. Unity Package Manager에 ROS-TCP-Connector 설치
//   2. Robotics → ROS Settings에서 ROS IP/Port 설정
//   3. Assembly Definition에 Unity.Robotics.ROSTCPConnector,
//      KineTutor3D.Runtime 참조 추가
//   4. FR5TemplateDemoRoot에 이 컴포넌트 추가 후
//      Inspector의 Controller 슬롯에 FR5TemplateDemoRoot 드래그
// ============================================================

using System;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using KineTutor3D.App;

public class RosJointStateSubscriber : MonoBehaviour
{
    // Inspector에서 FR5TemplateDemoRoot 오브젝트를 드래그해서 연결
    [SerializeField] private FR5TemplateMinimalController controller;

    // /joint_states 메시지에서 관절값을 읽을 순서 (j1 ~ j6)
    // ROS2에서 발행되는 name 배열 순서가 달라도 이름 기반으로 올바르게 매핑
    // 예: 메시지가 [j1, j2, j4, j5, j3, j6] 순서로 와도 정상 동작
    private static readonly string[] JointOrder = { "j1", "j2", "j3", "j4", "j5", "j6" };

    void Start()
    {
        // Unity Play 시작 시 /joint_states 토픽 구독 등록
        // 메시지가 도착할 때마다 OnJointState() 콜백 호출
        ROSConnection.GetOrCreateInstance().Subscribe<JointStateMsg>(
            "/joint_states", OnJointState);
    }

    // /joint_states 메시지 수신 시 호출되는 콜백
    void OnJointState(JointStateMsg msg)
    {
        // 유효성 검사: position, name 배열이 없거나 6개 미만이면 무시
        if (msg.position == null || msg.name == null || msg.position.Length < 6) return;

        var deg = new double[6];

        for (int i = 0; i < JointOrder.Length; i++)
        {
            // 이름 기반으로 인덱스 검색 (순서가 달라도 올바르게 매핑)
            int idx = System.Array.IndexOf(msg.name, JointOrder[i]);

            // 해당 관절 이름이 없거나 인덱스 범위 초과 시 무시
            if (idx < 0 || idx >= msg.position.Length) return;

            // 라디안 → 도(degree) 변환
            // ROS2는 라디안, Unity FR5TemplateMinimalController는 도 단위 사용
            deg[i] = msg.position[idx] * (180.0 / Math.PI);
        }

        // FR5 모델 관절에 각도 적용
        // controller가 null이면 안전하게 무시 (Inspector 미연결 방어)
        controller?.SetJointAnglesDeg(deg);
    }
}

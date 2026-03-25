using System.Collections.Generic;

namespace KineTutor3D.App.Fairino
{
    public sealed class FairinoErrorTranslator
    {
        private readonly Dictionary<int, string> messages = new Dictionary<int, string>();

        public FairinoErrorTranslator()
        {
            messages[0] = "OK";
            messages[-1] = "연결 실패: IP 주소 또는 네트워크 상태를 확인하세요.";
            messages[-2] = "로봇 비활성 상태입니다. Enable 버튼을 눌러주세요.";
            messages[-3] = "관절 제한 초과: 목표 각도가 허용 범위를 벗어났습니다.";
            messages[-4] = "비상 정지 활성 상태입니다. 해제 후 재시도하세요.";
            messages[-5] = "모션 충돌 감지: 이전 동작이 완료되지 않았습니다.";
            messages[-6] = "통신 타임아웃: 네트워크 연결을 확인하세요.";
        }

        public string Translate(int errorCode)
        {
            if (messages.TryGetValue(errorCode, out var message))
            {
                return message;
            }

            return $"알 수 없는 에러 (코드: {errorCode})";
        }

        public FairinoResult ToResult(int errorCode)
        {
            return new FairinoResult(errorCode, Translate(errorCode));
        }
    }
}

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.App.Fairino
{
    [DisallowMultipleComponent]
    public sealed class FairinoLiveSmokePanel : MonoBehaviour
    {
        private const string DefaultIp = "192.168.58.2";

        [Header("Inputs")]
        [SerializeField] private InputField ipInput;
        [SerializeField] private InputField portInput;

        [Header("Actions")]
        [SerializeField] private Button runButton;

        [Header("Outputs")]
        [SerializeField] private Text statusText;
        [SerializeField] private Text detailText;

        [Header("Labels")]
        [SerializeField] private string idleLabel = "미실행";
        [SerializeField] private string runningLabel = "테스트 중";
        [SerializeField] private string successLabel = "연결 성공";
        [SerializeField] private string partialSuccessLabel = "부분 성공";
        [SerializeField] private string failLabel = "연결 실패";
        [SerializeField] private string runtimeErrorLabel = "실행 오류";

        [Header("Colors")]
        [SerializeField] private Color idleColor = new Color(0.78f, 0.83f, 0.90f);
        [SerializeField] private Color runningColor = new Color(0.99f, 0.84f, 0.32f);
        [SerializeField] private Color successColor = new Color(0.41f, 0.88f, 0.52f);
        [SerializeField] private Color failColor = new Color(0.98f, 0.45f, 0.37f);

        private bool isRunning;

        private void Reset()
        {
            AutoBind();
            ApplyDefaultsIfEmpty();
            SetStatus(idleLabel, idleColor);
            SetDetails("버튼을 눌러 연결 신호만 확인하세요.");
        }

        private void Awake()
        {
            AutoBind();
            ApplyDefaultsIfEmpty();
            BindButton();

            if (string.IsNullOrWhiteSpace(GetStatusText()))
            {
                SetStatus(idleLabel, idleColor);
            }

            if (string.IsNullOrWhiteSpace(GetDetailText()))
            {
                SetDetails("버튼을 눌러 연결 신호만 확인하세요.");
            }
        }

        private void OnDestroy()
        {
            UnbindButton();
        }

        public void RunSmokeFromUi()
        {
            if (!isRunning)
            {
                StartCoroutine(RunSmokeRoutine());
            }
        }

        private IEnumerator RunSmokeRoutine()
        {
            isRunning = true;
            SetButtonInteractable(false);
            SetStatus(runningLabel, runningColor);
            SetDetails("연결 신호 확인을 시작합니다...");
            yield return null;

            var report = new StringBuilder();
            LiveFairinoClient client = null;

            try
            {
                var ip = string.IsNullOrWhiteSpace(GetIpText()) ? DefaultIp : GetIpText().Trim();
                client = new LiveFairinoClient();

                AppendLine(report, $"시간: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                AppendLine(report, $"대상 IP: {ip}");
                AppendLine(report, "연결 방식: FAIRINO C# SDK RPC(ip) - 포트 입력은 사용하지 않음");
                AppendLine(report, string.Empty);

                var connect = client.Connect(ip);
                AppendStep(report, "Connect", connect.IsSuccess, connect.Message, connect.ErrorCode);
                if (!connect.IsSuccess)
                {
                    SetStatus(failLabel, failColor);
                    SetDetails(report.ToString());
                    yield break;
                }

                var version = client.GetVersion();
                if (version.IsSuccess)
                {
                    AppendStep(report, "GetVersion", true, version.Message, version.ErrorCode);
                    AppendLine(report, $"SDK: {version.Value.SdkVersion}");
                    AppendLine(report, $"Software: {version.Value.SoftwareVersion}");
                    AppendLine(report, $"Controller: {version.Value.ControllerVersion}");
                    AppendLine(report, $"ControllerIP: {version.Value.ControllerIp}");
                    AppendLine(report, $"Firmware: {version.Value.FirmwareVersion}");
                    AppendLine(report, $"Hardware: {version.Value.HardwareVersion}");
                }
                else
                {
                    AppendStep(report, "GetVersion", false, version.Message, version.ErrorCode);
                }

                AppendLine(report, string.Empty);

                var state = client.ReadState();
                if (state.IsSuccess)
                {
                    AppendStep(report, "ReadState", true, state.Message, state.ErrorCode);
                    AppendLine(report, $"Joints: {FormatArray(state.Value.JointPosDeg)}");
                    AppendLine(report, $"TCP: {FormatArray(state.Value.TcpPose)}");
                    AppendLine(report, $"RobotMode: {state.Value.RobotMode}");
                    AppendLine(report, $"SafetyCode: {state.Value.SafetyCode}");
                    AppendLine(report, $"RealtimeSampleMs: {state.Value.RealtimeStateSamplePeriodMs}");
                    AppendLine(report, $"EmergencyStop: {state.Value.IsEmergencyStop}");
                    AppendLine(report, $"Collision: {state.Value.IsCollisionDetected}");
                    AppendLine(report, $"RobotEnabled: {state.Value.IsRobotEnabled}");
                }
                else
                {
                    AppendStep(report, "ReadState", false, state.Message, state.ErrorCode);
                }

                AppendLine(report, string.Empty);
                var disconnect = client.Disconnect();
                AppendStep(report, "Disconnect", disconnect.IsSuccess, disconnect.Message, disconnect.ErrorCode);

                var success = version.IsSuccess && state.IsSuccess;
                SetStatus(success ? successLabel : partialSuccessLabel, success ? successColor : runningColor);
                SetDetails(report.ToString());
            }
            catch (Exception ex)
            {
                AppendLine(report, string.Empty);
                AppendLine(report, $"예외: {ex.Message}");
                SetStatus(runtimeErrorLabel, failColor);
                SetDetails(report.ToString());
            }
            finally
            {
                client?.Disconnect();
                SetButtonInteractable(true);
                isRunning = false;
            }
        }

        private void AutoBind()
        {
            if (ipInput == null)
            {
                ipInput = transform.Find("Panel/IpInput")?.GetComponent<InputField>();
            }

            var legacyPortInput = portInput ?? transform.Find("Panel/PortInput")?.GetComponent<InputField>();
            if (legacyPortInput != null)
            {
                portInput = legacyPortInput;
                legacyPortInput.interactable = false;
            }

            if (runButton == null)
            {
                runButton = transform.Find("Panel/RunButton")?.GetComponent<Button>();
            }

            if (statusText == null)
            {
                statusText = transform.Find("Panel/StatusValue")?.GetComponent<Text>();
            }

            if (detailText == null)
            {
                detailText = transform.Find("Panel/DetailValue")?.GetComponent<Text>();
            }
        }

        private void ApplyDefaultsIfEmpty()
        {
            if (ipInput != null && string.IsNullOrWhiteSpace(ipInput.text))
            {
                ipInput.text = ReadEnvOrDefault("FAIRINO_IP", DefaultIp);
            }
        }

        private void BindButton()
        {
            if (runButton == null)
            {
                return;
            }

            runButton.onClick.RemoveListener(RunSmokeFromUi);
            runButton.onClick.AddListener(RunSmokeFromUi);
        }

        private void UnbindButton()
        {
            if (runButton != null)
            {
                runButton.onClick.RemoveListener(RunSmokeFromUi);
            }
        }

        private void SetButtonInteractable(bool value)
        {
            if (runButton != null)
            {
                runButton.interactable = value;
            }
        }

        private void SetStatus(string text, Color color)
        {
            if (statusText != null)
            {
                statusText.text = text ?? string.Empty;
                statusText.color = color;
            }
        }

        private void SetDetails(string text)
        {
            if (detailText != null)
            {
                detailText.text = text ?? string.Empty;
            }
        }

        private string GetIpText()
        {
            return ipInput != null ? ipInput.text : DefaultIp;
        }

        private string GetStatusText()
        {
            return statusText != null ? statusText.text : string.Empty;
        }

        private string GetDetailText()
        {
            return detailText != null ? detailText.text : string.Empty;
        }

        private static void AppendStep(StringBuilder sb, string step, bool success, string message, int errorCode)
        {
            var state = success ? "성공" : "실패";
            AppendLine(sb, $"{step}: {state} (code={errorCode})");
            if (!string.IsNullOrWhiteSpace(message))
            {
                AppendLine(sb, $"  {message}");
            }
        }

        private static void AppendLine(StringBuilder sb, string line)
        {
            sb.AppendLine(line ?? string.Empty);
        }

        private static string ReadEnvOrDefault(string key, string fallback)
        {
            var value = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string FormatArray(double[] values)
        {
            if (values == null || values.Length == 0)
            {
                return "-";
            }

            var parts = new string[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                parts[i] = values[i].ToString("0.###", CultureInfo.InvariantCulture);
            }

            return string.Join(", ", parts);
        }
    }
}

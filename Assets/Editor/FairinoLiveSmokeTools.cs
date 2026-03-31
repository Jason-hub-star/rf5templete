#if UNITY_EDITOR
using System;
using KineTutor3D.App.Fairino;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor
{
    public static class FairinoLiveSmokeTools
    {
        private const string DefaultIp = "192.168.58.2";

        [MenuItem("RobotTemplate/Run FAIRINO Live Smoke Test", priority = 180)]
        public static void RunMenu()
        {
            Debug.Log(RunSmoke());
        }

        public static string RunSmoke()
        {
            var ip = Environment.GetEnvironmentVariable("FAIRINO_IP");
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = DefaultIp;
            }

            var client = new LiveFairinoClient(new FairinoErrorTranslator());
            var connect = client.Connect(ip);
            if (!connect.IsSuccess)
            {
                return $"[FAIRINO LIVE SMOKE] CONNECT_FAIL ip={ip} code={connect.ErrorCode} msg={connect.Message}";
            }

            try
            {
                var version = client.GetVersion();
                var state = client.ReadState();

                var versionText = version.IsSuccess
                    ? $"controller_ip={version.Value.ControllerIp} fw={version.Value.FirmwareVersion} hw={version.Value.HardwareVersion} sdk={version.Value.SdkVersion}"
                    : $"version_fail code={version.ErrorCode} msg={version.Message}";

                var stateText = state.IsSuccess
                    ? $"joints=[{string.Join(", ", state.Value.JointPosDeg)}] tcp=[{string.Join(", ", state.Value.TcpPose)}]"
                    : $"state_fail code={state.ErrorCode} msg={state.Message}";

                return $"[FAIRINO LIVE SMOKE] CONNECT_OK ip={ip} {versionText} {stateText}";
            }
            finally
            {
                client.Disconnect();
            }
        }
    }
}
#endif

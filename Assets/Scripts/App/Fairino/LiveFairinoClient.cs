using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    public sealed class LiveFairinoClient : IFairinoRobotClient
    {
        private const byte CurrentStateFlag = 0;
        private readonly FairinoErrorTranslator errorTranslator;
        private bool connected;
        private bool enabled;
        private object sdkRobot;

        public bool IsConnected => connected;
        public bool IsEnabled => enabled;

        public LiveFairinoClient(FairinoErrorTranslator translator = null)
        {
            errorTranslator = translator ?? new FairinoErrorTranslator();
        }

        public FairinoResult Connect(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                return FairinoResult.Fail(-1, "IP 주소가 비어 있습니다.");
            }

            try
            {
                var robotType = FindSdkType("fairino.Robot");
                if (robotType == null)
                {
                    return FairinoResult.Fail(-1, "SDK DLL이 로드되지 않았습니다. Assets/Plugins/Fairino/에 libfairino.dll을 배치하세요.");
                }

                sdkRobot = Activator.CreateInstance(robotType);
                var result = InvokeSdk("RPC", ip);
                if (!result.IsSuccess)
                {
                    sdkRobot = null;
                    connected = false;
                    return FairinoResult.Fail(
                        result.ErrorCode,
                        $"RPC 연결 실패 (ip={ip}, code={result.ErrorCode}). 컨트롤러 전원, 네트워크, SDK 호환 버전을 확인하세요.");
                }

                connected = true;
                enabled = false;
                return FairinoResult.Ok($"연결 성공: {ip}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] 연결 실패: {ex.Message}");
                return FairinoResult.Fail(-6, ex.Message);
            }
        }

        public FairinoResult Disconnect()
        {
            connected = false;
            enabled = false;

            if (sdkRobot != null && HasMethod("CloseRPC"))
            {
                try
                {
                    InvokeSdk("CloseRPC");
                }
                catch
                {
                }
            }

            sdkRobot = null;
            return FairinoResult.Ok("연결 해제");
        }

        public FairinoResult Enable()
        {
            if (!connected) return errorTranslator.ToResult(-1);
            var result = InvokeSdk("RobotEnable", (byte)1);
            if (result.IsSuccess) enabled = true;
            return result;
        }

        public FairinoResult Disable()
        {
            if (!connected) return errorTranslator.ToResult(-1);
            var result = InvokeSdk("RobotEnable", (byte)0);
            if (result.IsSuccess) enabled = false;
            return result;
        }

        public FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent)
        {
            return FairinoResult.Fail(-6, "robottemplete에서는 read-only live smoke만 지원합니다.");
        }

        public FairinoResult ServoJ(double[] jointPosDeg)
        {
            return FairinoResult.Fail(-6, "robottemplete에서는 read-only live smoke만 지원합니다.");
        }

        public FairinoResult<FairinoRobotState> ReadState()
        {
            if (!connected)
            {
                return FairinoResult<FairinoRobotState>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            try
            {
                if (TryReadRealtimeState(out var realtimeState))
                {
                    return FairinoResult<FairinoRobotState>.Ok(realtimeState, "실시간 상태 읽기 성공");
                }

                var joints = ReadJointPositionsFallback();
                var tcpPose = ReadTcpPoseFallback();
                return FairinoResult<FairinoRobotState>.Ok(new FairinoRobotState(joints, tcpPose), "기본 상태 읽기 성공");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] 상태 읽기 실패: {ex.Message}");
                return FairinoResult<FairinoRobotState>.Fail(-6, ex.Message);
            }
        }

        public FairinoResult MoveL(double[] tcpPose, int speedPercent, int accPercent)
        {
            return FairinoResult.Fail(-6, "robottemplete에서는 read-only live smoke만 지원합니다.");
        }

        public FairinoResult StopMotion()
        {
            return FairinoResult.Fail(-6, "robottemplete에서는 read-only live smoke만 지원합니다.");
        }

        public FairinoResult<FairinoVersionInfo> GetVersion()
        {
            if (!connected)
            {
                return FairinoResult<FairinoVersionInfo>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            try
            {
                var sdkVersion = ReadSingleStringByRef("GetSDKVersion");
                var softwareVersion = ReadStringsByRef("GetSoftwareVersion", string.Empty, string.Empty, string.Empty);
                var hardwareVersion = ReadStringsByRef(
                    "GetHardwareVersion",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty);
                var firmwareVersion = ReadStringsByRef(
                    "GetFirmwareVersion",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty);

                var controllerIp = ReadSingleStringByRef("GetControllerIP");
                var firmwareSummary = JoinNonEmpty(firmwareVersion);
                var softwareSummary = JoinNonEmpty(softwareVersion);
                var hardwareSummary = JoinNonEmpty(hardwareVersion);
                var controllerVersion = softwareVersion.Length > 2 ? softwareVersion[2] : string.Empty;

                if (string.IsNullOrWhiteSpace(firmwareSummary))
                {
                    firmwareSummary = "Live";
                }

                if (string.IsNullOrWhiteSpace(sdkVersion))
                {
                    sdkVersion = "Unknown SDK";
                }

                return FairinoResult<FairinoVersionInfo>.Ok(
                    new FairinoVersionInfo(
                        firmwareSummary,
                        sdkVersion,
                        softwareSummary,
                        controllerVersion,
                        hardwareSummary,
                        controllerIp));
            }
            catch (Exception ex)
            {
                return FairinoResult<FairinoVersionInfo>.Fail(-6, ex.Message);
            }
        }

        public FairinoResult<int> GetSafetyCode()
        {
            if (!connected)
            {
                return FairinoResult<int>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            try
            {
                var code = InvokeSdkRaw("GetSafetyCode", Array.Empty<object>());
                return FairinoResult<int>.Ok(ConvertSdkReturnCode(code, "GetSafetyCode"));
            }
            catch (Exception ex)
            {
                return FairinoResult<int>.Fail(-6, ex.Message);
            }
        }

        public FairinoResult<int> GetRealtimeStateSamplePeriod()
        {
            if (!connected)
            {
                return FairinoResult<int>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            try
            {
                var args = new object[] { 0 };
                var code = InvokeSdkRaw("GetRobotRealtimeStateSamplePeriod", args);
                var errCode = ConvertSdkReturnCode(code, "GetRobotRealtimeStateSamplePeriod");
                if (errCode != 0)
                {
                    return FairinoResult<int>.Fail(errCode, errorTranslator.Translate(errCode));
                }

                return FairinoResult<int>.Ok(Convert.ToInt32(args[0]));
            }
            catch (Exception ex)
            {
                return FairinoResult<int>.Fail(-6, ex.Message);
            }
        }

        public FairinoResult SetRealtimeStateSamplePeriod(int periodMs)
        {
            return FairinoResult.Fail(-6, "robottemplete에서는 read-only live smoke만 지원합니다.");
        }

        public FairinoResult ClearMotionQueue()
        {
            return FairinoResult.Fail(-6, "robottemplete에서는 read-only live smoke만 지원합니다.");
        }

        public FairinoResult SetMode(int mode)
        {
            return FairinoResult.Fail(-6, "robottemplete에서는 read-only live smoke만 지원합니다.");
        }

        private FairinoResult InvokeSdk(string methodName, params object[] args)
        {
            try
            {
                var code = InvokeSdkRaw(methodName, args);
                return errorTranslator.ToResult(ConvertSdkReturnCode(code, methodName));
            }
            catch (Exception ex)
            {
                return FairinoResult.Fail(-6, ex.Message);
            }
        }

        private object InvokeSdkRaw(string methodName, object[] args)
        {
            if (sdkRobot == null)
            {
                throw new InvalidOperationException("SDK 인스턴스가 없습니다.");
            }

            var method = ResolveBestMethod(methodName, args);
            if (method == null)
            {
                throw new MissingMethodException($"SDK에 {methodName} 메서드가 없습니다.");
            }

            return method.Invoke(sdkRobot, args);
        }

        private MethodInfo ResolveBestMethod(string methodName, object[] args)
        {
            return sdkRobot.GetType()
                .GetMethods()
                .Where(m => m.Name == methodName)
                .OrderBy(m => ScoreMethod(m, args))
                .FirstOrDefault();
        }

        private static int ScoreMethod(MethodInfo method, object[] args)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != args.Length)
            {
                return int.MaxValue;
            }

            var score = 0;
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }

                var arg = args[i];
                if (arg == null)
                {
                    score += 1;
                    continue;
                }

                var argType = arg.GetType();
                if (parameterType == argType)
                {
                    continue;
                }

                if (parameterType.IsAssignableFrom(argType))
                {
                    score += 1;
                    continue;
                }

                if (parameterType == typeof(float) && argType == typeof(int))
                {
                    score += 2;
                    continue;
                }

                if (parameterType == typeof(int) && argType == typeof(byte))
                {
                    score += 2;
                    continue;
                }

                if (parameterType == typeof(byte) && argType == typeof(int))
                {
                    score += 3;
                    continue;
                }

                score += 10;
            }

            return score;
        }

        private bool HasMethod(string methodName)
        {
            return sdkRobot != null && sdkRobot.GetType().GetMethods().Any(m => m.Name == methodName);
        }

        private bool TryReadRealtimeState(out FairinoRobotState state)
        {
            state = FairinoRobotState.Zero();
            if (!HasMethod("GetRobotRealTimeState"))
            {
                return false;
            }

            var pkgType = FindSdkType("fairino.ROBOT_STATE_PKG");
            if (pkgType == null)
            {
                return false;
            }

            var pkg = Activator.CreateInstance(pkgType);
            var args = new[] { pkg };
            var result = InvokeSdkRaw("GetRobotRealTimeState", args);
            var errCode = ConvertSdkReturnCode(result, "GetRobotRealTimeState");
            if (errCode != 0)
            {
                throw new InvalidOperationException(errorTranslator.Translate(errCode));
            }

            var payload = args[0];
            var joints = ReadDoubleArrayField(payload, "jt_cur_pos", 6);
            var tcp = ReadDoubleArrayField(payload, "tl_cur_pos", 6);
            state = new FairinoRobotState(
                joints,
                tcp,
                robotMode: Convert.ToInt32(GetFieldValue(payload, "robot_mode")),
                motionQueueLength: Convert.ToInt32(GetFieldValue(payload, "mc_queue_len")),
                safetyCode: ReadSafetyCodeOrDefault(),
                realtimeStateSamplePeriodMs: ReadRealtimeStatePeriodOrDefault(),
                isEmergencyStop: Convert.ToByte(GetFieldValue(payload, "EmergencyStop")) != 0,
                isCollisionDetected: Convert.ToByte(GetFieldValue(payload, "collisionState")) != 0,
                isRobotEnabled: Convert.ToInt32(GetFieldValue(payload, "rbtEnableState")) != 0);
            return true;
        }

        private int ReadSafetyCodeOrDefault()
        {
            var result = GetSafetyCode();
            return result.IsSuccess ? result.Value : 0;
        }

        private int ReadRealtimeStatePeriodOrDefault()
        {
            var result = GetRealtimeStateSamplePeriod();
            return result.IsSuccess ? result.Value : 0;
        }

        private double[] ReadJointPositionsFallback()
        {
            var jointPos = CreateSdkJointPos(new double[6]);
            var args = new[] { (object)CurrentStateFlag, jointPos };
            var result = InvokeSdkRaw("GetActualJointPosDegree", args);
            var errCode = ConvertSdkReturnCode(result, "GetActualJointPosDegree");
            if (errCode != 0)
            {
                throw new InvalidOperationException(errorTranslator.Translate(errCode));
            }

            return ReadDoubleArrayField(args[1], "jPos", 6);
        }

        private double[] ReadTcpPoseFallback()
        {
            var descPose = CreateSdkDescPose(new double[6]);
            var args = new[] { (object)CurrentStateFlag, descPose };
            var result = InvokeSdkRaw("GetActualTCPPose", args);
            var errCode = ConvertSdkReturnCode(result, "GetActualTCPPose");
            if (errCode != 0)
            {
                throw new InvalidOperationException(errorTranslator.Translate(errCode));
            }

            return ReadPoseFromDescPose(args[1]);
        }

        private string ReadSingleStringByRef(string methodName)
        {
            var args = new object[] { string.Empty };
            var result = InvokeSdkRaw(methodName, args);
            var errCode = ConvertSdkReturnCode(result, methodName);
            if (errCode != 0)
            {
                throw new InvalidOperationException(errorTranslator.Translate(errCode));
            }

            return args[0]?.ToString() ?? string.Empty;
        }

        private string[] ReadStringsByRef(string methodName, params string[] initialValues)
        {
            var args = initialValues.Cast<object>().ToArray();
            var result = InvokeSdkRaw(methodName, args);
            var errCode = ConvertSdkReturnCode(result, methodName);
            if (errCode != 0)
            {
                throw new InvalidOperationException(errorTranslator.Translate(errCode));
            }

            return args.Select(a => a?.ToString() ?? string.Empty).ToArray();
        }

        private static object CreateSdkJointPos(double[] joints)
        {
            var jointPosType = FindSdkType("fairino.JointPos")
                ?? throw new InvalidOperationException("fairino.JointPos 타입을 찾을 수 없습니다.");
            var instance = Activator.CreateInstance(jointPosType);
            SetField(instance, "jPos", (double[])joints.Clone());
            return instance;
        }

        private static object CreateSdkDescPose(double[] tcpPose)
        {
            var descPoseType = FindSdkType("fairino.DescPose")
                ?? throw new InvalidOperationException("fairino.DescPose 타입을 찾을 수 없습니다.");
            var descTranType = FindSdkType("fairino.DescTran")
                ?? throw new InvalidOperationException("fairino.DescTran 타입을 찾을 수 없습니다.");
            var rpyType = FindSdkType("fairino.Rpy")
                ?? throw new InvalidOperationException("fairino.Rpy 타입을 찾을 수 없습니다.");

            var descPose = Activator.CreateInstance(descPoseType);
            var tran = Activator.CreateInstance(descTranType);
            var rpy = Activator.CreateInstance(rpyType);

            SetField(tran, "x", tcpPose[0]);
            SetField(tran, "y", tcpPose[1]);
            SetField(tran, "z", tcpPose[2]);
            SetField(rpy, "rx", tcpPose[3]);
            SetField(rpy, "ry", tcpPose[4]);
            SetField(rpy, "rz", tcpPose[5]);
            SetField(descPose, "tran", tran);
            SetField(descPose, "rpy", rpy);
            return descPose;
        }

        private static double[] ReadPoseFromDescPose(object descPose)
        {
            var tran = GetFieldValue(descPose, "tran");
            var rpy = GetFieldValue(descPose, "rpy");
            return new[]
            {
                Convert.ToDouble(GetFieldValue(tran, "x")),
                Convert.ToDouble(GetFieldValue(tran, "y")),
                Convert.ToDouble(GetFieldValue(tran, "z")),
                Convert.ToDouble(GetFieldValue(rpy, "rx")),
                Convert.ToDouble(GetFieldValue(rpy, "ry")),
                Convert.ToDouble(GetFieldValue(rpy, "rz"))
            };
        }

        private static double[] ReadDoubleArrayField(object instance, string fieldName, int expectedLength)
        {
            var raw = GetFieldValue(instance, fieldName) as double[];
            if (raw == null)
            {
                return new double[expectedLength];
            }

            if (raw.Length == expectedLength)
            {
                return (double[])raw.Clone();
            }

            var copy = new double[expectedLength];
            Array.Copy(raw, copy, System.Math.Min(raw.Length, expectedLength));
            return copy;
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName);
            if (field == null)
            {
                throw new MissingFieldException(instance.GetType().FullName, fieldName);
            }

            field.SetValue(instance, value);
        }

        private static object GetFieldValue(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName);
            if (field == null)
            {
                throw new MissingFieldException(instance.GetType().FullName, fieldName);
            }

            return field.GetValue(instance);
        }

        private static int ConvertSdkReturnCode(object sdkResult, string methodName)
        {
            if (sdkResult is int intCode)
            {
                return intCode;
            }

            if (sdkResult is byte byteCode)
            {
                return byteCode;
            }

            throw new InvalidOperationException($"{methodName} returned unsupported result type '{sdkResult?.GetType().FullName ?? "null"}'.");
        }

        private static string JoinNonEmpty(params string[] values)
        {
            return string.Join(" | ", values.Where(v => !string.IsNullOrWhiteSpace(v)));
        }

        private static Type FindSdkType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}

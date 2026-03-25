namespace KineTutor3D.App.Fairino
{
    public interface IFairinoRobotClient
    {
        bool IsConnected { get; }
        bool IsEnabled { get; }

        FairinoResult Connect(string ip, int port);
        FairinoResult Disconnect();
        FairinoResult Enable();
        FairinoResult Disable();
        FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent);
        FairinoResult ServoJ(double[] jointPosDeg);
        FairinoResult<FairinoRobotState> ReadState();
        FairinoResult MoveL(double[] tcpPose, int speedPercent, int accPercent);
        FairinoResult StopMotion();
        FairinoResult<FairinoVersionInfo> GetVersion();
        FairinoResult<int> GetSafetyCode();
        FairinoResult<int> GetRealtimeStateSamplePeriod();
        FairinoResult SetRealtimeStateSamplePeriod(int periodMs);
        FairinoResult ClearMotionQueue();
        FairinoResult SetMode(int mode);
    }
}

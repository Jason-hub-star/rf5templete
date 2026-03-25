namespace KineTutor3D.App.Fairino
{
    public readonly struct FairinoRobotState
    {
        public double[] JointPosDeg { get; }
        public double[] TcpPose { get; }
        public int RobotMode { get; }
        public int MotionQueueLength { get; }
        public int SafetyCode { get; }
        public int RealtimeStateSamplePeriodMs { get; }
        public bool IsEmergencyStop { get; }
        public bool IsCollisionDetected { get; }
        public bool IsRobotEnabled { get; }

        public FairinoRobotState(
            double[] jointPosDeg,
            double[] tcpPose,
            int robotMode = 0,
            int motionQueueLength = 0,
            int safetyCode = 0,
            int realtimeStateSamplePeriodMs = 0,
            bool isEmergencyStop = false,
            bool isCollisionDetected = false,
            bool isRobotEnabled = false)
        {
            JointPosDeg = jointPosDeg != null ? (double[])jointPosDeg.Clone() : new double[6];
            TcpPose = tcpPose != null ? (double[])tcpPose.Clone() : new double[6];
            RobotMode = robotMode;
            MotionQueueLength = motionQueueLength;
            SafetyCode = safetyCode;
            RealtimeStateSamplePeriodMs = realtimeStateSamplePeriodMs;
            IsEmergencyStop = isEmergencyStop;
            IsCollisionDetected = isCollisionDetected;
            IsRobotEnabled = isRobotEnabled;
        }

        public static FairinoRobotState Zero()
        {
            return new FairinoRobotState(new double[6], new double[6]);
        }
    }
}

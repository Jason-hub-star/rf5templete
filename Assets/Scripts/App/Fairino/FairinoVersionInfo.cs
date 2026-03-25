namespace KineTutor3D.App.Fairino
{
    public readonly struct FairinoVersionInfo
    {
        public string FirmwareVersion { get; }
        public string SdkVersion { get; }
        public string SoftwareVersion { get; }
        public string ControllerVersion { get; }
        public string HardwareVersion { get; }

        public FairinoVersionInfo(string firmwareVersion, string sdkVersion)
            : this(firmwareVersion, sdkVersion, string.Empty, string.Empty, string.Empty)
        {
        }

        public FairinoVersionInfo(
            string firmwareVersion,
            string sdkVersion,
            string softwareVersion,
            string controllerVersion,
            string hardwareVersion)
        {
            FirmwareVersion = firmwareVersion ?? string.Empty;
            SdkVersion = sdkVersion ?? string.Empty;
            SoftwareVersion = softwareVersion ?? string.Empty;
            ControllerVersion = controllerVersion ?? string.Empty;
            HardwareVersion = hardwareVersion ?? string.Empty;
        }
    }
}

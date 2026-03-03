namespace HardwareMode
{
    /// <summary>
    /// Specifies the hardware execution mode for devices.
    /// </summary>
    public enum HardwareModeEnum
    {
        /// <summary>
        /// Control real hardware.
        /// </summary>
        Real = 0,

        /// <summary>
        /// Emulated mode for testing without hardware (outputs to console).
        /// </summary>
        Emulated = 1,

        /// <summary>
        /// Emulated mode for testing without hardware, but with less verbose output than Emulated mode.
        /// </summary>
        EmulatedSilent = 2
    }
}

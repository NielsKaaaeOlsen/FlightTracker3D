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
        Real,

        /// <summary>
        /// Emulated mode for testing without hardware (outputs to console).
        /// </summary>
        Emulated,

        /// <summary>
        /// Emulated mode for testing without hardware, but with less verbose output than Emulated mode.
        EmulatedSilent
        /// </summary>
    }
}

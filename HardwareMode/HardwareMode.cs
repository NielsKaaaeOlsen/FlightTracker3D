namespace HardwareMode
{
    /// <summary>
    /// Specifies the hardware execution mode for devices.
    /// </summary>
    public enum HardwareMode
    {
        /// <summary>
        /// Control real hardware.
        /// </summary>
        Real,

        /// <summary>
        /// Emulated mode for testing without hardware (outputs to console).
        /// </summary>
        Emulated
    }
}

namespace VolkanTurkutCase.Runtime.Core
{
    /// <summary>
    /// Defines the type of interaction required for an interactable object.
    /// </summary>
    public enum InteractionType
    {
        /// <summary>
        /// Single press interaction, executes immediately.
        /// </summary>
        Instant,

        /// <summary>
        /// Requires holding the interaction key for a duration.
        /// </summary>
        Hold,

        /// <summary>
        /// Toggles between on/off states.
        /// </summary>
        Toggle
    }
}

namespace VolkanTurkutCase.Runtime.Core
{
    /// <summary>
    /// Interface for all interactable objects in the world.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Gets the type of interaction this object requires.
        /// </summary>
        InteractionType InteractionType { get; }

        /// <summary>
        /// Gets the duration required for Hold interactions.
        /// </summary>
        float HoldDuration { get; }

        /// <summary>
        /// Determines if the player can currently interact with this object.
        /// </summary>
        /// <returns>True if interaction is possible, false otherwise.</returns>
        bool CanInteract();

        /// <summary>
        /// Executes the interaction logic.
        /// </summary>
        void Interact();

        /// <summary>
        /// Called every frame while hold interaction is in progress.
        /// </summary>
        /// <param name="progress">Current progress from 0 to 1.</param>
        void OnHoldProgress(float progress);

        /// <summary>
        /// Called when hold interaction is cancelled.
        /// </summary>
        void OnHoldCancelled();

        /// <summary>
        /// Gets the prompt message to display to the player.
        /// </summary>
        /// <returns>Localized prompt string.</returns>
        string GetPromptMessage();
    }
}

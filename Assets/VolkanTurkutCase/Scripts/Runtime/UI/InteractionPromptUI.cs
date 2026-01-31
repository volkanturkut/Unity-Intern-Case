using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VolkanTurkutCase.Runtime.Core;
using VolkanTurkutCase.Runtime.Player;

namespace VolkanTurkutCase.Runtime.UI
{
    /// <summary>
    /// UI component that displays interaction prompts and hold progress.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        #region Fields

        private const float k_DefaultFadeSpeed = 10f;

        [Header("References")]
        [SerializeField] private InteractionDetector m_InteractionDetector;
        [SerializeField] private CanvasGroup m_CanvasGroup;

        [Header("Prompt Elements")]
        [SerializeField] private TextMeshProUGUI m_PromptText;
        [SerializeField] private GameObject m_PromptContainer;

        [Header("Progress Bar")]
        [SerializeField] private GameObject m_ProgressBarContainer;
        [SerializeField] private Image m_ProgressBarFill;
        [SerializeField] private TextMeshProUGUI m_ProgressText;

        [Header("Settings")]
        [SerializeField] private float m_FadeSpeed = k_DefaultFadeSpeed;

        private bool m_IsVisible;
        private float m_TargetAlpha;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (m_InteractionDetector == null)
            {
                m_InteractionDetector = FindFirstObjectByType<InteractionDetector>();
                if (m_InteractionDetector == null)
                {
                    Debug.LogError("[InteractionPromptUI] InteractionDetector not found!");
                }
            }

            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = GetComponent<CanvasGroup>();
            }

            HideImmediate();
        }

        private void OnEnable()
        {
            if (m_InteractionDetector != null)
            {
                m_InteractionDetector.OnInteractableDetected += HandleInteractableDetected;
                m_InteractionDetector.OnInteractableLost += HandleInteractableLost;
                m_InteractionDetector.OnHoldProgressChanged += HandleHoldProgress;
                m_InteractionDetector.OnHoldCancelled += HandleHoldCancelled;
            }
        }

        private void OnDisable()
        {
            if (m_InteractionDetector != null)
            {
                m_InteractionDetector.OnInteractableDetected -= HandleInteractableDetected;
                m_InteractionDetector.OnInteractableLost -= HandleInteractableLost;
                m_InteractionDetector.OnHoldProgressChanged -= HandleHoldProgress;
                m_InteractionDetector.OnHoldCancelled -= HandleHoldCancelled;
            }
        }

        private void Update()
        {
            UpdateAlpha();
            UpdatePromptText();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles when a new interactable is detected.
        /// </summary>
        private void HandleInteractableDetected(IInteractable interactable)
        {
            Show();
            UpdatePromptText();

            bool isHoldType = interactable.InteractionType == InteractionType.Hold;
            SetProgressBarVisible(isHoldType);
            
            if (isHoldType)
            {
                UpdateProgressBar(0f);
            }
        }

        /// <summary>
        /// Handles when the interactable is lost.
        /// </summary>
        private void HandleInteractableLost()
        {
            Hide();
            SetProgressBarVisible(false);
        }

        /// <summary>
        /// Handles hold progress updates.
        /// </summary>
        private void HandleHoldProgress(float progress)
        {
            UpdateProgressBar(progress);
        }

        /// <summary>
        /// Handles when hold interaction is cancelled.
        /// </summary>
        private void HandleHoldCancelled()
        {
            UpdateProgressBar(0f);
        }

        /// <summary>
        /// Updates the prompt text based on current target.
        /// </summary>
        private void UpdatePromptText()
        {
            if (m_InteractionDetector == null || m_InteractionDetector.CurrentTarget == null)
            {
                return;
            }

            if (m_PromptText != null)
            {
                m_PromptText.text = m_InteractionDetector.CurrentTarget.GetPromptMessage();
            }
        }

        /// <summary>
        /// Updates the progress bar display.
        /// </summary>
        private void UpdateProgressBar(float progress)
        {
            if (m_ProgressBarFill != null)
            {
                m_ProgressBarFill.fillAmount = progress;
            }

            if (m_ProgressText != null)
            {
                m_ProgressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }

        /// <summary>
        /// Sets the visibility of the progress bar.
        /// </summary>
        private void SetProgressBarVisible(bool visible)
        {
            if (m_ProgressBarContainer != null)
            {
                m_ProgressBarContainer.SetActive(visible);
            }
        }

        /// <summary>
        /// Shows the interaction prompt.
        /// </summary>
        public void Show()
        {
            m_IsVisible = true;
            m_TargetAlpha = 1f;

            if (m_PromptContainer != null)
            {
                m_PromptContainer.SetActive(true);
            }
        }

        /// <summary>
        /// Hides the interaction prompt.
        /// </summary>
        public void Hide()
        {
            m_IsVisible = false;
            m_TargetAlpha = 0f;
        }

        /// <summary>
        /// Immediately hides the prompt without animation.
        /// </summary>
        private void HideImmediate()
        {
            m_IsVisible = false;
            m_TargetAlpha = 0f;

            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 0f;
            }

            if (m_PromptContainer != null)
            {
                m_PromptContainer.SetActive(false);
            }

            SetProgressBarVisible(false);
        }

        /// <summary>
        /// Updates the alpha for fade animation.
        /// </summary>
        private void UpdateAlpha()
        {
            if (m_CanvasGroup == null)
            {
                return;
            }

            if (Mathf.Approximately(m_CanvasGroup.alpha, m_TargetAlpha))
            {
                if (!m_IsVisible && m_PromptContainer != null)
                {
                    m_PromptContainer.SetActive(false);
                }
                return;
            }

            m_CanvasGroup.alpha = Mathf.MoveTowards(
                m_CanvasGroup.alpha,
                m_TargetAlpha,
                Time.deltaTime * m_FadeSpeed
            );
        }

        #endregion
    }
}

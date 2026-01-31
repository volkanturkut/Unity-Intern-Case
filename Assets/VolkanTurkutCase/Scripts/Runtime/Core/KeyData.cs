using UnityEngine;

namespace VolkanTurkutCase.Runtime.Core
{
    /// <summary>
    /// ScriptableObject defining a key that can unlock doors.
    /// </summary>
    [CreateAssetMenu(fileName = "NewKey", menuName = "VolkanTurkutCase/Items/Key Data")]
    public class KeyData : ItemData
    {
        #region Fields

        [Header("Key Settings")]
        [SerializeField] private string m_KeyId = "key_default";
        [SerializeField] private Color m_KeyColor = Color.yellow;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique identifier for this key type.
        /// </summary>
        public string KeyId => m_KeyId;

        /// <summary>
        /// Gets the color of this key for visual distinction.
        /// </summary>
        public Color KeyColor => m_KeyColor;

        #endregion
    }
}

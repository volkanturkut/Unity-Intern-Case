using UnityEngine;

namespace VolkanTurkutCase.Runtime.Core
{
    /// <summary>
    /// Base ScriptableObject for all item definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "VolkanTurkutCase/Items/Item Data")]
    public class ItemData : ScriptableObject
    {
        #region Fields

        [Header("Item Info")]
        [SerializeField] private string m_ItemName = "New Item";
        [SerializeField] private string m_ItemDescription = "";
        [SerializeField] private Sprite m_Icon;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the display name of the item.
        /// </summary>
        public string ItemName => m_ItemName;

        /// <summary>
        /// Gets the description of the item.
        /// </summary>
        public string ItemDescription => m_ItemDescription;

        /// <summary>
        /// Gets the icon sprite for UI display.
        /// </summary>
        public Sprite Icon => m_Icon;

        #endregion
    }
}

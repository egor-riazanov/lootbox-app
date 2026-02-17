using UnityEngine;

namespace Lootbox
{
    /// <summary>
    ///     Abstracts slot item creation and sprite assignment,
    ///     allowing different factory implementations to be swapped in.
    /// </summary>
    public interface ISlotItemFactory
    {
        /// <summary>
        ///     Number of unique sprite varieties used by the factory.
        /// </summary>
        int VarietyCount { get; }

        /// <summary>
        ///     Creates a new slot item as a child of <paramref name="parent" />.
        /// </summary>
        RectTransform Create(RectTransform parent);

        /// <summary>
        ///     Assigns the next sprite in sequence to an existing slot item.
        /// </summary>
        void AssignNextSprite(RectTransform item);
    }
}

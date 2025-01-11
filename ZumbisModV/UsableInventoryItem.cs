using System;
using ZumbisModV.Interfaces;

namespace ZumbisModV
{
    [Serializable]
    public class UsableInventoryItem : InventoryItemBase, ICraftable
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="UsableInventoryItem"/>.
        /// </summary>
        /// <param name="amount">A quantidade inicial do item.</param>
        /// <param name="maxAmount">A quantidade máxima permitida do item.</param>
        /// <param name="id">O identificador do item.</param>
        /// <param name="description">A descrição do item.</param>
        /// <param name="itemEvents">Os eventos utilizáveis associados ao item.</param>
        public UsableItemEvent[] ItemEvents { get; set; }
        public CraftableItemComponent[] RequiredComponents { get; set; }

        public UsableInventoryItem(
            int amount,
            int maxAmount,
            string id,
            string description,
            UsableItemEvent[] itemEvents
        )
            : base(amount, maxAmount, id, description)
        {
            ItemEvents = itemEvents;
        }
    }
}

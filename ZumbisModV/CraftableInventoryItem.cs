using System;
using ZumbisModV.Interfaces;

namespace ZumbisModV
{
    [Serializable]
    public class CraftableInventoryItem : InventoryItemBase, ICraftable, IValidatable
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CraftableInventoryItem"/>.
        /// </summary>
        /// <param name="amount">A quantidade inicial do item.</param>
        /// <param name="maxAmount">A quantidade máxima permitida do item.</param>
        /// <param name="id">O identificador único do item.</param>
        /// <param name="description">A descrição detalhada do item.</param>
        /// <param name="validation">Critérios de validação associados ao item.</param>
        ///

        public CraftableItemComponent[] RequiredComponents { get; set; }
        public Func<bool> Validation { get; set; }

        public CraftableInventoryItem(
            int amount,
            int maxAmount,
            string id,
            string description,
            Func<bool> validation
        )
            : base(amount, maxAmount, id, description)
        {
            Validation = validation;
        }
    }
}

using System;
using LemonUI.Menus;
using ZumbisModV.Interfaces;

namespace ZumbisModV
{
    [Serializable]
    public class InventoryItemBase : IIdentifier
    {
        [NonSerialized]
        public NativeItem MenuItem;

        public void CreateMenuItem() => MenuItem = new NativeItem(Id, Description);

        public int Amount { get; set; }
        public int MaxAmount { get; set; }

        public string Id { get; set; }
        public string Description { get; set; }

        /// <summary>
        ///  Inicializa uma nova instância da classe <see cref="InventoryItemBase"/>.
        /// </summary>
        /// <param name="amount">A quantidade inicial do item.</param>
        /// <param name="maxAmount">A quantidade máxima permitida do item.</param>
        /// <param name="id">O identificador do item.</param>
        /// <param name="description">A descrição do item.</param>
        public InventoryItemBase(int amount, int maxAmount, string id, string description)
        {
            Amount = amount;
            MaxAmount = maxAmount;
            Id = id;
            Description = description;
        }
    }
}

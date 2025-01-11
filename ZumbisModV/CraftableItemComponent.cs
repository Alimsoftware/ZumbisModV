using System;

namespace ZumbisModV
{
    [Serializable]
    public class CraftableItemComponent
    {
        public CraftableItemComponent(InventoryItemBase resource, int requiredAmount)
        {
            this.Resource = resource;
            this.RequiredAmount = requiredAmount;
        }

        public InventoryItemBase Resource { get; set; }

        public int RequiredAmount { get; set; }
    }
}

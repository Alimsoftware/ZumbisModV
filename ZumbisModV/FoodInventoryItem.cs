using System;
using GTA;
using ZumbisModV.Interfaces;

namespace ZumbisModV
{
    [Serializable]
    public class FoodInventoryItem : InventoryItemBase, IFood, IAnimatable, ICraftable
    {
        public string AnimationDict { get; set; }
        public string AnimationName { get; set; }
        public AnimationFlags AnimationFlags { get; set; }
        public int AnimationDuration { get; set; }
        public FoodType FoodType { get; set; }
        public float RestorationAmount { get; set; }
        public CraftableItemComponent[] RequiredComponents { get; set; }
        public NearbyResource NearbyResource { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="FoodInventoryItem"/>.
        /// </summary>
        /// <param name="amount">A quantidade inicial do item.</param>
        /// <param name="maxAmount">A quantidade máxima permitida do item.</param>
        /// <param name="id">O identificador único do item.</param>
        /// <param name="description">A descrição detalhada do item.</param>
        /// <param name="animationDict">O dicionário de animação associado ao item.</param>
        /// <param name="animationName">O nome da animação a ser utilizada para o item.</param>
        /// <param name="animationFlags">As flags que controlam o comportamento da animação.</param>
        /// <param name="animationDuration">A duração da animação em milissegundos.</param>
        /// <param name="foodType">O tipo de alimento representado pelo item.</param>
        /// <param name="restorationAmount">A quantidade de restauração proporcionada pelo item.</param>

        public FoodInventoryItem(
            int amount,
            int maxAmount,
            string id,
            string description,
            string animationDict,
            string animationName,
            AnimationFlags animationFlags,
            int animationDuration,
            FoodType foodType,
            float restorationAmount
        )
            : base(amount, maxAmount, id, description)
        {
            AnimationDict = animationDict;
            AnimationName = animationName;
            AnimationFlags = animationFlags;
            AnimationDuration = animationDuration;
            FoodType = foodType;
            RestorationAmount = restorationAmount;
        }
    }
}

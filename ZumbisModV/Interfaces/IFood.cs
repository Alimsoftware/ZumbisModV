using System;

namespace ZumbisModV.Interfaces
{
    public interface IFood : IAnimatable
    {
        FoodType FoodType { get; set; }

        float RestorationAmount { get; set; }
    }
}

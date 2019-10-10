using System;
using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface IConsumable
    {
        bool IsFruit { get; set; }
        int FoodValue { get; set; }
        int TicksToEat { get; set; }
        (string Name, int Degree) Effect { get; set; }  // IUsable encloses...
        uint PlantID { get; set; }
        string ConsumeVerb { get; set; }
    }
}

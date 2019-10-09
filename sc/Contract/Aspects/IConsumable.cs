﻿using System;
using System.Collections.Generic;

namespace CopperBend.Contract
{
    public interface IConsumable
    {
        bool IsFruit { get; set; }
        int FoodValue { get; set; }
        int TicksToEat { get; set; }
        (string Name, int Degree) Effect { get; set; }
        uint PlantID { get; set; }
        string ConsumeVerb { get; set; }
    }

    /// <summary> Highly speculative </summary>
    public interface IUsable
    {
        List<IUse> Uses { get; set; }
    }
    public interface IUse
    {
        string Verb { get; set; }
        List<UseCost> Costs { get; set; }
        List<UseEffect> Effects { get; set; }

        bool IsExpended { get; }
        UseTargetFlags Targets { get; set; }

        bool TakesDirection { get; }
        // => Targets.HasFlag(UseTargetType.Direction);
    }

    public struct UseCost 
    {
        public string Substance { get; set; }
        public int Amount { get; set; }
        public UseCost(string substance, int amount)
        {
            Substance = substance;
            Amount = amount;
        }
    }
    public struct UseEffect
    {
        public string Effect { get; set; }
        public int Amount { get; set; }
        public UseEffect(string effect, int amount)
        {
            Effect = effect;
            Amount = amount;
        }
    }
    
    [Flags]
    public enum UseTargetFlags
    {
        Unset = 0,
        Self = 1,
        Being = 2,
        Direction = 4,
        Location = 8,
        Item = 16,
    }
}

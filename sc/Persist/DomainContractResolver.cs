using System;
using CopperBend.Contract;
using CopperBend.Fabric;
using CopperBend.Model;
using Newtonsoft.Json.Serialization;

namespace CopperBend.Persist
{
    //  Throwing stuff at the wall  --  This is all mad dog prototyping
    public class DomainContractResolver : DefaultContractResolver
    {
        public static readonly DomainContractResolver Instance = new DomainContractResolver();

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            if (objectType == typeof(ItemMap))
            {
                contract.Converter = new JConv_ItemMap();
            }
            if (typeof(IItem).IsAssignableFrom(objectType))
            {
                contract.Converter = new JConv_IItem();
            }
            if (typeof(ISpace).IsAssignableFrom(objectType))
            {
                contract.Converter = new JConv_ISpace();
            }
            if (typeof(IAreaBlight).IsAssignableFrom(objectType))
            {
                contract.Converter = new JConv_IAreaBlight();
            }

            return contract;
        }
    }
}

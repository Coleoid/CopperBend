using System;
using System.Text;
using CopperBend.Contract;
using CopperBend.Persist;
using GoRogue;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace CopperBend.Fabric
{
    public class Cartographer
    {
        public CompoundMapData CompoundMap_ToCompoundMapData(ICompoundMap map)
        {
            var cmd = new CompoundMapData();


            return cmd;
        }

        public void LoadCompoundMap_FromCompoundMapData(ICompoundMap map, CompoundMapData data)
        {
        }



        //TODO: Only resurrect CompoundMapData_ToYaml() if TagMapping alone doesn't work
        //public void CompoundMapData_ToYaml(IEmitter emitter, CompoundMapData map)
        //{
        //    emitter.Emit(new Scalar(null, "CompoundMap"));
        //    emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

        //    emitter.EmitKVP("Name", map.Name);
        //    emitter.EmitKVP("Width", map.Width.ToString());
        //    emitter.EmitKVP("Height", map.Height.ToString());

        //    //SpaceMap_ToYaml(emitter, map.SpaceMap);
        //    //RotMap_ToYaml(emitter, map.RotMap);
        //    //BeingMap_ToYaml(emitter, map.BeingMap);
        //    //ItemMap_ToYaml(emitter, map.ItemMap);

        //    emitter.Emit(new MappingEnd());
        //}

        //public void SpaceMap_ToYaml(IEmitter emitter, ISpaceMap spaceMap)
        //{
        //    emitter.Emit(new Scalar(null, "SpaceMap"));
        //    emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

        //    emitter.EmitKVP("PlayerStartPoint", spaceMap.PlayerStartPoint.ToString());

        //    emitter.Emit(new Scalar(null, "Thing"));
        //    emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
        //    for (int y = 0; y < spaceMap.Height; y++)
        //    {
        //        var row = new StringBuilder(spaceMap.Width);

        //        for (int x = 0; x < spaceMap.Width; x++)
        //        {
        //            var ch = (char)spaceMap.GetItem((x, y)).Terrain.Cell.Glyph;
        //            row.Append(ch);
        //        }

        //    }


        //    emitter.Emit(new MappingEnd());
        //}

        //public void RotMap_ToYaml(IEmitter emitter, IRotMap rotMap)
        //{
        //    emitter.Emit(new Scalar(null, "RotMap"));
        //    emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));



        //    emitter.Emit(new MappingEnd());
        //}

        //public void BeingMap_ToYaml(IEmitter emitter, MultiSpatialMap<IBeing> beingMap)
        //{
        //    emitter.Emit(new Scalar(null, "BeingMap"));
        //    emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));



        //    emitter.Emit(new MappingEnd());
        //}

        //public void ItemMap_ToYaml(IEmitter emitter, IItemMap itemMap)
        //{
        //    emitter.Emit(new Scalar(null, "ItemMap"));
        //    emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));



        //    emitter.Emit(new MappingEnd());
        //}



        //internal object CompoundMap_FromYaml(IParser parser)
        //{
        //    throw new NotImplementedException();
        //}

        //internal object SpaceMap_FromYaml(IParser parser)
        //{
        //    throw new NotImplementedException();
        //}

        //internal object RotMap_FromYaml(IParser parser)
        //{
        //    throw new NotImplementedException();
        //}

        //internal object BeingMap_FromYaml(IParser parser)
        //{
        //    throw new NotImplementedException();
        //}

        //internal object ItemMap_FromYaml(IParser parser)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

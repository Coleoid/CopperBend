using System;
using System.Collections.Generic;
using System.Text;
using CopperBend.Contract;
using SadConsole.Entities;

namespace CopperBend.Engine
{
    public class Director
    {
        //  Main use scenario:
        // get script (who parses? start tdd with tree?)
        // get tree
        // get entities involved
        // push new command sources onto them
        // start interpreting incident script
        // figure out when steps occur
        // send step commands to appropriate entities
        // finish
        // pull off script command sources
        // maybe send "refresh plan" to existing, reactivated CSes

        public static IEntityFactory EntityFactory { get; set; }
        public Dictionary<string, IEntity> Cast { get; }

        public Director()
        {
            Cast = new Dictionary<string, IEntity>();
        }

        public void RunScript(ScriptTree scriptTree)
        {
            CastEntities(scriptTree.EntitiesWanted);
        }

        public void CastEntities(List<string> entitiesWanted)
        {
            foreach (var entityWanted in entitiesWanted)
            {
                Cast[entityWanted] = FindEntity(entityWanted);
            }
        }

        //  may belong elsewhere soon
        public IEntity FindEntity(string entityWanted)
        {
            IEntity entity = null;

            // build a rat for test one
            // dwuh? entity = EntityFactory.GetSadCon()

            return entity;
        }
    }


    public class ScriptTree
    {
        public List<string> EntitiesWanted { get; internal set; }
        public ScriptTree()
        {
            EntitiesWanted = new List<string>();
        }
    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using CopperBend.Contract;
using CopperBend.Model;
using CopperBend.Fabric;

namespace CopperBend.Logic
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

        public Dictionary<string, IBeing> Cast { get; }

        public Director()
        {
            Cast = new Dictionary<string, IBeing>();
        }

        public void RunScript(ScriptTree scriptTree)
        {
            CastBeings(scriptTree.BeingsWanted);

            SetStage(scriptTree.Location);
        }

        public void CastBeings(List<string> beingsWanted)
        {
            foreach (var beingWanted in beingsWanted)
            {
                Cast[beingWanted] = FindBeing(beingWanted) ?? BuildNewBeing(beingWanted);
            }
        }

        public void SetStage(string location)
        {

        }

        //  may belong elsewhere soon
        public IBeing FindBeing(string entityWanted)
        {
            IBeing being = null;

            // Phase 2: ?

            return being;
        }

        //  definitely belongs elsewhere soon
        public Being BuildNewBeing(string entityWanted)
        {
            var creator = Engine.Compendium.BeingCreator;

            // add method which takes name only
            Being being = creator.CreateBeing(Color.Purple, Color.Black, 'r');
            being.Name = "rat";

            return being;
        }
    }


    public class ScriptTree
    {
        public List<string> BeingsWanted { get; internal set; }
        public string Location { get; internal set; }

        public ScriptTree()
        {
            BeingsWanted = new List<string>();
        }
    }
}

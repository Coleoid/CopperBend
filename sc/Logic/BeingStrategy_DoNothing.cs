using System.Collections.Generic;
using CopperBend.Contract;

#pragma warning disable CA2227 // YAML s'zn wants collection setters
namespace CopperBend.Logic
{
    public class BeingStrategy_DoNothing : IBeingStrategy
    {
        private readonly IControlPanel controls;
        public string SubType => "Do Nothing";

        public Dictionary<string, string> Storage { get; set; }

        public BeingStrategy_DoNothing(IControlPanel con)
        {
            Storage = new Dictionary<string, string>();
            controls = con;
        }

        public void GiveCommand(IBeing being)
        {
            var cmd = new Command(CmdAction.Wait, CmdDirection.None);
            controls.CommandBeing(being, cmd);
        }
    }
}
#pragma warning restore CA2227 // YAML s'zn wants collection setters

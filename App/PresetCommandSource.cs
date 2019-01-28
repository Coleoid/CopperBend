using System;
using System.Collections;
using System.Collections.Generic;

namespace CopperBend.App
{
    public class PresetCommandSource : ICommandSource
    {
        public Queue<Command> CmdQ = new Queue<Command>();

        public Command GetCommand()
        {
            if (CmdQ.Count > 0)
                return CmdQ.Dequeue();
            else
                return new Command(CmdAction.Wait, CmdDirection.None);
        }

        public void AddCommand(Command command)
        {
            CmdQ.Enqueue(command);
        }
    }

}

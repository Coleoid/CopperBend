using System;

namespace CopperBend.Fabric
{
    public class Guard
    {
        public static void Against(bool condition, string message = "Condition violated")
        {
            if (condition) throw new Exception(message);
        }

        public static void AgainstNullArgument(object argument, string message = "Argument null")
        {
            if (argument == null) throw new Exception(message);
        }

    }
}

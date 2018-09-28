using System;

namespace CopperBend.App
{
    public class Guard
    {
        public static void Against(bool condition, string message = "Condition violated")
        {
            if (!condition) throw new Exception(message);
        }

    }
}

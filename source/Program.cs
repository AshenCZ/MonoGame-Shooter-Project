#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace NPRG2_Shooter
{
#if WINDOWS || LINUX
    // The main class
    public static class Program
    {
        // The main entry point for the application
        [STAThread]
        static void Main()
        {
            using (var game = new ShooterGame())
                game.Run();
        }
    }
#endif
}

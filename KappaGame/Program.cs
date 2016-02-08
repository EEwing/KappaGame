using Kappa.server;
using System;

namespace Kappa {
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //TODO: Update argument checking to allow for extensible argument parameters
            if(args.Length > 0 && args[0] == "-server") {
                Server server = new Server();
                server.Run();
            } else {
                using (var game = new KappaGame())
                    game.Run();
            }
        }
    }
#endif
}

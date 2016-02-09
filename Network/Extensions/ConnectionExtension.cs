using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Network.Extensions
{
    /// <summary>
    /// Connection extensions. Provides some methods to handle a connection.
    /// </summary>
    internal static class ConnectionExtension
    {
        private static SemaphoreSlim slimSemaphore = new SemaphoreSlim(1);
        private static int counter;

        /// <summary>
        /// Generates a unique hashCode for the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>System.Int32.</returns>
        internal static int GenerateUniqueHashCode(this Connection connection)
        {
            try
            {
                slimSemaphore.Wait();
                counter++;
                return counter;
            }
            catch(ObjectDisposedException ode)
            {
                throw ode;
            }
            finally
            {
                slimSemaphore.Release();
            }
        }
    }
}

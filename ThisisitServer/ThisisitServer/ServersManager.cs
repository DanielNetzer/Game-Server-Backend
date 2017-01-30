using System.Threading.Tasks;

namespace ServersManager
{
    class ServersManager
    {
        static void Main(string[] args)
        {
            Parallel.Invoke(() =>
                {
                    LoginServer.StartServer();
                },

                () =>
                {
                    HTTPServer.StartServer();
                }
            );
        }

    }
}


using System.Net;
using CS3500.Networking;

// TODO add logging.
class Program
{

    private static NetworkConnection connection; 
    static void Main(string[ ] args)
    {
        int port = 80;
        HttpListener listener = new ();
        listener.Prefixes.Add(string.Format("http://localhost:{0}/", port));

        listener.Start();
        Console.WriteLine("Starting web server on localhost, port " + port);

        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string responseString = "<html><body><h1>Welcome to the Snake Game Web Server!</h1></body></html>";

        }
    }
}
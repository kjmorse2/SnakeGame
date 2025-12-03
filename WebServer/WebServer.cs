using Microsoft.Data.SqlClient; 
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using CS3500.Networking;

namespace WebServer;
class WebServer
{
    
    static string SnakeSecrets = JsonDocument.Parse(File.ReadAllText("secrets.json")).RootElement.GetProperty("ConnectionSTring").GetString()!;
    public static void HandleConnection(NetworkConnection connection)
    {

        string lineFromBrowser = string.Empty;
        while (true) {
            try
            {
                lineFromBrowser = connection.ReceiveLine();
                if (lineFromBrowser.Length == 0)
                {
                    break;

                }

                string[ ] parts = lineFromBrowser.Split(' ');
                string path = parts.Length > 1 ? parts[ 1 ] : "/";
                string html;

                if (path == "/")
                {
                    html = HomePage();

                }
                else if (path == "/games")
                {
                    html = GamesPage();
                }
            }
            catch
            {
                return;
            }


        } }

    private static string HomePage()
        {
         return "<html>\n<h3>Welcome to the Snake Games Database!</h3>\n<a href=\"/games\">View Games</a>\n</html>";
        }

    private static string GamesPage()
    {
        StringBuilder sb = new();
        sb.Append("<html>");
        sb.Append("<table border=\"1\">");
        sb.Append("<thead><tr><td>ID</td><td>Start</td><td>End</td></tr></thead><tbody><tr>");

        
        using (SqlConnection sqlConn = new(SnakeSecrets))
        {
            try
            {
                sqlConn.Open();
                Console.WriteLine("Connection to Database opened.");
                string queryString = "";
                SqlCommand command = new SqlCommand();
            }
            catch { }
        }

        return sb.ToString();
    }

    public static
}

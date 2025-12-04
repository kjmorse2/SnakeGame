using System.Text;
using System.Text.Json;
using CS3500.Networking;
using Microsoft.Data.SqlClient;

namespace CS3500.WebServer;

public class WebServer
{
    private static readonly string SnakeSecrets = JsonDocument.Parse(File.ReadAllText("secrets.json")).RootElement
        .GetProperty("ConnectionSTring").GetString()!;


    public static void HandleConnection(NetworkConnection connection)
    {
        string lineFromBrowser = string.Empty;
        while (true)
        {
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
                    html = AllGamesPage();
                }

                else
                {
                    html = SpecficGamesPage();
                }
            }

            catch
            {
                return;
            }
        }
    }

    private static string AllGamesPage()
    {
        StringBuilder sb = new();
        sb.Append("<html>");
        sb.Append("<table border=\"1\">");
        sb.Append("<thead><tr><td>ID</td><td>Start</td><td>End</td></tr></thead><tbody>");


        using (SqlConnection sqlConn = new(SnakeSecrets))
        {
            try
            {
                sqlConn.Open();
                Console.WriteLine("Connection to Database opened.");
                SqlCommand command = new("SELECT * FROM GameTable", sqlConn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int gameID = reader.GetInt32(0);
                        DateTime start = reader.GetDateTime(1);
                        DateTime? end = reader.GetDateTime(2);
                        sb.Append("<tr>");
                        sb.Append($"<td><a href=\"/games?gameID={gameID}\">{gameID}</a></td>");
                        sb.Append($"<td>{start} </td>");
                        sb.Append($"<td>{(end.HasValue ? end.Value.ToString() : string.Empty)}</td>");
                        sb.Append("</tr>");
                    }
                }
            }
            catch { }
        }

        return sb.ToString();
    }


    private static string HomePage()
    {
        return "<html>\n<h3>Welcome to the Snake Games Database!</h3>\n<a href=\"/games\">View Games</a>\n</html>";
    }

    private static string SpecficGamesPage()
    {
        StringBuilder sb = new();
        sb.Append("<html>");
        sb.Append("<table border=\"1\">");
        sb.Append("<thead><tr><td>ID</td><td>Start</td><td>End</td></tr></thead><tbody>");


        using (SqlConnection sqlConn = new(SnakeSecrets))
        {
            try
            {
                sqlConn.Open();
                Console.WriteLine("Connection to Database opened.");
                SqlCommand command = new("SELECT * FROM GameTable", sqlConn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int gameID = reader.GetInt32(0);
                        DateTime start = reader.GetDateTime(1);
                        DateTime? end = reader.GetDateTime(2);
                        sb.Append("<tr>");
                        sb.Append($"<td><a href=\"/games?gameID={gameID}\">{gameID}</a></td>");
                        sb.Append($"<td>{start} </td>");
                        sb.Append($"<td>{(end.HasValue ? end.Value.ToString() : string.Empty)}</td>");
                        sb.Append("</tr>");
                    }
                }
            }
            catch { }
        }

        return sb.ToString();
    }
}

using System.Text;
using Projekat3.Models;

namespace Projekat3.WebServer;

public static class HtmlGen {

    public static byte[] GenerateErrorTemplate(string[] messages) {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("    <title>Error</title>");
        sb.AppendLine("    <style>");

        sb.AppendLine("        .error-container {");
        sb.AppendLine("            background-color: #fff;");
        sb.AppendLine("            padding: 30px;");
        sb.AppendLine("            border-radius: 5px;");
        sb.AppendLine("            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);");
        sb.AppendLine("            text-align: center;");
        sb.AppendLine("        }");

        sb.AppendLine("        .error-container h1 {");
        sb.AppendLine("            font-size: 24px;");
        sb.AppendLine("            margin-bottom: 10px;");
        sb.AppendLine("        }");

        sb.AppendLine("        .error-details {");
        sb.AppendLine("            display: none;");
        sb.AppendLine("            font-size: 14px;");
        sb.AppendLine("            color: #333;");
        sb.AppendLine("            margin-top: 10px;");
        sb.AppendLine("        }");

        sb.AppendLine("        .error-container a.show-more {");
        sb.AppendLine("            display: inline-block;");
        sb.AppendLine("            padding: 5px 10px;");
        sb.AppendLine("            background-color: #3498db;");
        sb.AppendLine("            color: #fff;");
        sb.AppendLine("            text-decoration: none;");
        sb.AppendLine("            border-radius: 5px;");
        sb.AppendLine("            transition: background-color 0.2s ease-in-out;");
        sb.AppendLine("        }");

        sb.AppendLine("        .error-container a.show-more:hover {");
        sb.AppendLine("            background-color: #2980b9;");
        sb.AppendLine("        }");

        sb.AppendLine("        .error-container a.show-less {");
        sb.AppendLine("            display: none;");
        sb.AppendLine("            padding: 5px 10px;");
        sb.AppendLine("            background-color: #95a5a6;");
        sb.AppendLine("            color: #fff;");
        sb.AppendLine("            text-decoration: none;");
        sb.AppendLine("            border-radius: 5px;");
        sb.AppendLine("            transition: background-color 0.2s ease-in-out;");
        sb.AppendLine("        }");
        sb.AppendLine("        .error-container a.show-less:hover {");
        sb.AppendLine("            background-color: #8899a6;");
        sb.AppendLine("        }");
        sb.AppendLine("    </style>");
        sb.AppendLine("    <script>");
        sb.AppendLine("function handleShowMoreClick() {");
        sb.AppendLine("  const showMoreBtn = document.querySelector(\".show-more\");");
        sb.AppendLine("  const showLessBtn = document.querySelector(\".show-less\");");
        sb.AppendLine("  const errorDetails = document.querySelector(\".error-details\");");
        sb.AppendLine("");
        sb.AppendLine("  errorDetails.style.display = \"block\";");
        sb.AppendLine("  showMoreBtn.style.display = \"none\";");
        sb.AppendLine("  showLessBtn.style.display = \"inline-block\";");
        sb.AppendLine("}");
        sb.AppendLine("");
        sb.AppendLine("function handleShowLessClick() {");
        sb.AppendLine("  const showMoreBtn = document.querySelector(\".show-more\");");
        sb.AppendLine("  const showLessBtn = document.querySelector(\".show-less\");");
        sb.AppendLine("  const errorDetails = document.querySelector(\".error-details\");");
        sb.AppendLine("");
        sb.AppendLine("  errorDetails.style.display = \"none\";");
        sb.AppendLine("  showMoreBtn.style.display = \"inline-block\";");
        sb.AppendLine("  showLessBtn.style.display = \"none\";");
        sb.AppendLine("}");
        sb.AppendLine("    </script>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class=\"error-container\">");
        sb.AppendLine("        <h1>Error</h1>");
        sb.AppendLine("        <p>An error has occurred.</p>");
        sb.AppendLine("        <a href=\"#\" class=\"show-more\" onclick=\"handleShowMoreClick()\">Show Details</a>");
        sb.AppendLine("        <div class=\"error-details\">");
        foreach (var v in messages) {
            sb.AppendLine($"            <p>{v}</p>");
        }
        sb.AppendLine("        </div>");
        sb.AppendLine("        <a href=\"#\" class=\"show-less\" onclick=\"handleShowLessClick()\">Show Less</a>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public static byte[] GenerateOverviewByTopic(Dictionary<string, List<GithubInfo>> results) {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("    <title>Github Repositories by Topic</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        table {");
        sb.AppendLine("            border-collapse: collapse;");
        sb.AppendLine("            width: 100%;");
        sb.AppendLine("        }");
        sb.AppendLine("        th, td {");
        sb.AppendLine("            padding: 10px;");
        sb.AppendLine("            border: 1px solid #ddd;");
        sb.AppendLine("            text-align: left;");
        sb.AppendLine("        }");
        sb.AppendLine("        th {");
        sb.AppendLine("            background-color: #f2f2f2;");
        sb.AppendLine("        }");
        sb.AppendLine("        tr:nth-child(even) {");
        sb.AppendLine("            background-color: #f9f9f9;");
        sb.AppendLine("        }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <h1>Github Repositories by Topic</h1>");
        sb.AppendLine("    <table>");
        sb.AppendLine("        <thead>");
        sb.AppendLine("            <tr>");

        foreach (var topic in results.Keys) {
            sb.AppendLine($"             <th>{topic}</th>");
        }
        sb.AppendLine("            </tr>");
        sb.AppendLine("        </thead>");
        sb.AppendLine("        <tbody>");

        int maxRows = results.Values.Max(list => list.Count);

        for (int i = 0; i < maxRows; i++) {
            sb.AppendLine("            <tr>");
            foreach (var topic in results.Keys) {
                if (i < results[topic].Count) {
                    var repo = results[topic][i];
                    sb.AppendLine(
                        $"                <td>{repo.Name} (Stars: {repo.Stars}, Forks: {repo.Forks}, Size: {repo.Size})</td>");
                }
                else {
                    sb.AppendLine(
                        "                <td></td>");
                }
            }
            sb.AppendLine("            </tr>");
        }
        sb.AppendLine("        </tbody>");
        sb.AppendLine("    </table>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
using System.Text.RegularExpressions;
using System.Net;

string inputFIlename = "intput.txt";

string cookieEnvName = "ADC_COOKIE_VALUE";

string cookieValue = GetAndSetEnvVar(cookieEnvName);

if (!File.Exists(inputFIlename))
{
    GetInputFile(cookieValue, inputFIlename);
}

var content = File.ReadAllLines(inputFIlename);


#region Solutions 

Regex regexStartPacket = new Regex(@"([a-z])(?!\1)([a-z])(?!\1|\2)([a-z])(?!\1|\2|\3)([a-z])");

// (?x) : free spacing mode modifier
// This pattern work for the second part but to mix it up a bit I used the queue solution below
Regex regexStartMessage = new Regex(@"(?x)  ([a-z])(?!\1)
                                            ([a-z])(?!\1|\2)
                                            ([a-z])(?!\1|\2|\3)
                                            ([a-z])(?!\1|\2|\3|\4)
                                            ([a-z])(?!\1|\2|\3|\4|\5)
                                            ([a-z])(?!\1|\2|\3|\4|\5|\6)
                                            ([a-z])(?!\1|\2|\3|\4|\5|\6|\7)
                                            ([a-z])(?!\1|\2|\3|\4|\5|\6|\7|\8)
                                            ([a-z])(?!\1|\2|\3|\4|\5|\6|\7|\8|\9)
                                            ([a-z])(?!\1|\2|\3|\4|\5|\6|\7|\8|\9|\10)
                                            ([a-z])(?!\1|\2|\3|\4|\5|\6|\7|\8|\9|\10|\11)
                                            ([a-z])(?!\1|\2|\3|\4|\5|\6|\7|\8|\9|\10|\11|\12)
                                            ([a-z])(?!\1|\2|\3|\4|\5|\6|\7|\8|\9|\10|\11|\12|\13)
                                            ([a-z])");

Match matchStartPacket = regexStartPacket.Match(content[0]);
//Match matchStartMessage = regexStartMessage.Match(content[0]);

Queue<char> chars = new();
int index = 0;

foreach (char c in content[0])
{
    chars.Enqueue(c);
    index++;

    if (chars.Count == 14)
    {
        if (chars.Distinct().Count() == 14) {
            break;
        }
        else {
            chars.Dequeue();
        }
    }
}


Console.WriteLine($"Part One : {matchStartPacket.Index + matchStartPacket.Length}");
Console.WriteLine($"Part Two : {index}");

#endregion


string GetAndSetEnvVar(string cookieEnvName)
{
    string? cookieVal;

    if (Environment.GetEnvironmentVariable(cookieEnvName, EnvironmentVariableTarget.User) == null)
    {
        // Visual Studio Code : launch.json : "console": "integratedTerminal", for Console.ReadLine to work while debugging
        Console.WriteLine("Paste the value of your Advent of Code session cookie that will be saved as an user env. var. :");
        cookieVal = Console.ReadLine();

        Environment.SetEnvironmentVariable(cookieEnvName, cookieVal, EnvironmentVariableTarget.User);
    }
    else
    {
        cookieVal = Environment.GetEnvironmentVariable(cookieEnvName, EnvironmentVariableTarget.User);
    }

    if (cookieVal == null)
    {
        throw new NullReferenceException("You have to set the value of the cookie to get your input");
    }

    return cookieVal;
}

bool GetInputFile(string cookieValue, string inputFilename)
{
    string pattern = @"\d{4}-\d{2}";

    Regex regex = new(pattern);

    Match match = regex.Match(Environment.CurrentDirectory);

    string url = $"https://adventofcode.com/{match.Value[0..4]}/day/{match.Value[6..7]:0}/input";

    var client = new HttpClient();

    var request = new HttpRequestMessage
    {
        RequestUri = new Uri(url),
        Method = HttpMethod.Get
    };

    var cookie = new Cookie("session", cookieValue);

    request.Headers.Add("Cookie", cookie.ToString());

    var response = client.Send(request);

    if (response.IsSuccessStatusCode)
    {
        var fileContent = response.Content.ReadAsByteArrayAsync().Result;

        File.WriteAllBytes(inputFilename, fileContent);

        return true;
    }
    else
    {
        return false;
    }
}

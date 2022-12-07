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

Regex regexSteps = new Regex(@"move (\d+) from (\d) to (\d)");

string messageForTheElves;

var stacksSol1 = new Stack<string>[9];
var stacksSol2 = new Stack<string>[9];

// We get the intial stacks
for (int i = 7; i >= 0; i--)
{

    for (int k = 0; k <= 8; k++)
    {
        if (i == 7)
        {
            // It would have been better to realize a deep copy of stacksSol1 after but at the first glance its not that simple
            stacksSol1[k] = new Stack<string>();
            stacksSol2[k] = new Stack<string>();
        }

        // Stacks have variable length
        if (content[i][(k*4)+1] != ' ')
        {
            stacksSol1[k].Push(content[i][(k*4)+1].ToString());
            stacksSol2[k].Push(content[i][(k*4)+1].ToString());
        }
    }
}

// We apply the steps
for (int i = 10; i < content.Count(); i++)
{
    Match matchSteps = regexSteps.Match(content[i]);

    int moveQty = int.Parse(matchSteps.Groups[1].Value);

    int fromStack = int.Parse(matchSteps.Groups[2].Value);

    int toStack = int.Parse(matchSteps.Groups[3].Value);

   List<string> pickedCrates = new();

    for (int k = 1; k <= moveQty; k++)
    {
        stacksSol1[toStack-1].Push(stacksSol1[fromStack-1].Pop());

        pickedCrates.Add(stacksSol2[fromStack-1].Pop());
    }

    for (int k = moveQty - 1; k >= 0; k--)
    {
        stacksSol2[toStack-1].Push(pickedCrates.ToArray()[k]);
    }
}

// We craft the message with the first crates of each stack

messageForTheElves = string.Join("", stacksSol1.Select<Stack<string>, string>(x => x.Peek()));

Console.WriteLine($"Part One : {messageForTheElves}");


messageForTheElves = string.Join("", stacksSol2.Select<Stack<string>, string>(x => x.Peek()));

Console.WriteLine($"Part Two : {messageForTheElves}");


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


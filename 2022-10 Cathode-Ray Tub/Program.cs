using System.Text.RegularExpressions;
using System.Net;

string inputFIlename = "input.txt";

string cookieEnvName = "ADC_COOKIE_VALUE";

string cookieValue = GetAndSetEnvVar(cookieEnvName);

if (!File.Exists(inputFIlename))
{
    GetInputFile(cookieValue, inputFIlename);
}

var content = File.ReadAllLines(inputFIlename);


#region Solutions 

Queue<int> checkpoints = new(new [] { 20, 60, 100, 140, 180, 220 });
int nextCheckpoint = checkpoints.Dequeue();
var registerDuringCycle = new int[241];
int currentCycle = 0;
registerDuringCycle[currentCycle] = 1;

long SignalStrengthsSum = 0;
string crtDisplay = "";

foreach (string line in content)
{
    string instruction = line.Substring(0, 4);

    switch (instruction)
    {
        case "addx":
            currentCycle++;
            registerDuringCycle[currentCycle] = registerDuringCycle[currentCycle - 1];

            currentCycle++;
            registerDuringCycle[currentCycle] = registerDuringCycle[currentCycle - 1] + int.Parse(line.Substring(5));
            break;

        case "noop":
            currentCycle++;
            registerDuringCycle[currentCycle] = registerDuringCycle[currentCycle - 1];
            break;
    }
}

for (int i = 0; i < registerDuringCycle.Count(); i++)
{
    // Array base zero
    currentCycle = i + 1;

    if (currentCycle == nextCheckpoint)
    {
        SignalStrengthsSum += currentCycle * registerDuringCycle[i];

        if (checkpoints.Count > 0)
            nextCheckpoint = checkpoints.Dequeue();
    }

    int position = (i % 40);

    if (position <= registerDuringCycle[i] + 1 && position >= registerDuringCycle[i] - 1) {
        crtDisplay += 'X';
    }
    else {
        crtDisplay += '.';
    }
}

Console.WriteLine($"Part one: {SignalStrengthsSum}");


var screen = crtDisplay.Remove(crtDisplay.Length - 1, 1).Select((c, index) => new {c, index})
    .GroupBy(a => a.index / 40)
    .Select(g => g.Select(x => x.c))
    .Select(chars => new string(chars.ToArray()));

Console.WriteLine("Part two:");

foreach (string line in screen)
    Console.WriteLine(line);

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
    string pattern = @"(?'year'\d{4})-(?'month'\d{2})";

    Regex regex = new(pattern);

    Match match = regex.Match(Environment.CurrentDirectory);

    string url = $"https://adventofcode.com/{match.Groups["year"].Value}/day/{match.Groups["month"].Value:0}/input";

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
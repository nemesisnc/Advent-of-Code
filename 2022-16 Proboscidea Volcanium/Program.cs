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

int maxTime = 30;

Regex regex = new(@"Valve (?'valve'[A-Z]{2}) has flow rate=(?'flowrate'\d+); tunnel(s)? lead(s)? to valve(s)? (?:(?'leads'[A-Z]{2})(, )?)+");

Dictionary<string, (string Valve, int FlowRate, List<string> Leads)> Valves = new ();

int index = 0;
string startingValve = "";
foreach (string line in content)
{
    Match match = regex.Match(line);

    List<string> Leads = new();

    foreach (Capture capture in match.Groups["leads"].Captures)
    {
        Leads.Add(capture.Value);
    }

    Valves.Add(match.Groups["valve"].Value,
        (match.Groups["valve"].Value, int.Parse(match.Groups["flowrate"].Value), Leads));

    if (index == 0)
    {
        startingValve = match.Groups["valve"].Value;
    }

    index++;
}

//Dictionary<string, (int Counter, int PressureRelease)> Passages = new();
Dictionary<string, int> passageValves = new();
bool maxPassageDone = false;
int maxPressureRelease = 0;
string bestWay = "";
int passageCounter = 0;
int calcWin;

// Doesnt work, I have to implement Breadth-First Search (BFS)
while (!maxPassageDone)
{
    int time = 0;
    string currentValve = "";
    string currentWay = "";
    string nextValve = "";
    int pressureRelease = 0;
    //(int Counter, int PressureRelease) passage;
    Dictionary<string, int> openValves = new();

    while (time <= maxTime)
    {
        int maxPassage = 99999999;

        if (currentValve == "")
        {
            currentValve = startingValve;
            currentWay += currentValve;

            if (!passageValves.ContainsKey(currentValve))
                passageValves.Add(currentValve, 0);
        }
        else
        {
            currentValve = nextValve;
            currentWay += currentValve;
        }
        passageValves[currentValve]++;

        if (Valves[currentValve].FlowRate > 0 && !openValves.ContainsKey(currentValve))
        {
            time++;
            pressureRelease += (maxTime - time) * Valves[currentValve].FlowRate;
            openValves.Add(currentValve, 0);
        }

        foreach(var lead in Valves[currentValve].Leads)
        {
            if (!passageValves.ContainsKey(lead))
                passageValves.Add(lead, 0);

            int priority = 0;
            if (!openValves.ContainsKey(lead))
            {
                priority = Valves[lead].FlowRate;
            }
                
            if (passageValves[lead] / Math.Max(priority, 1) <= maxPassage)
            {
                maxPassage = passageValves[lead] / Math.Max(priority, 1);
                nextValve = lead;
            }
        }
        time++;

        if (nextValve == "")
        {
            maxPassageDone = true;
            time = 31;
        }

        // Debug: to check if we are going in the right way (winning way for the example)
        if (currentWay == "AADDCCBBAAIIJJ") //AADDCCBBAAIIJJIIAADDEEFFGGHHGGFFEEDDCC
        {
            calcWin = pressureRelease;
        }
    }

    if (pressureRelease > maxPressureRelease)
    {
        maxPressureRelease = pressureRelease;
        bestWay = currentWay;
    }

    passageCounter++;
}

Console.WriteLine($"{bestWay} = {maxPressureRelease}");

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

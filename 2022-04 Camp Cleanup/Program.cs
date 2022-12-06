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

Regex regex = new(@"(\d+)-(\d+),(\d+)-(\d+)");

int sumOfTotalOverlapPairs = 0;
int sumOfPartialOverlapPairs = 0;

foreach (string line in content)
{
    Match match = regex.Match(line);

    var elf1 = new RangeOfSections(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
    var elf2 = new RangeOfSections(int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value));

    if ((elf1.IdStart <= elf2.IdStart && elf1.IdEnd >= elf2.IdEnd) 
        || (elf2.IdStart <= elf1.IdStart && elf2.IdEnd >= elf1.IdEnd))
    {
        sumOfTotalOverlapPairs++;
        sumOfPartialOverlapPairs++;
    }
    else if ((elf1.IdStart <= elf2.IdStart && elf1.IdEnd >= elf2.IdStart) 
        || (elf2.IdStart <= elf1.IdStart && elf2.IdEnd >= elf1.IdStart)
        || (elf1.IdStart <= elf2.IdEnd && elf1.IdEnd >= elf2.IdEnd) 
        || (elf2.IdStart <= elf1.IdEnd && elf2.IdEnd >= elf1.IdEnd))
    {
        sumOfPartialOverlapPairs++;
    }
}

Console.WriteLine($"Part One : {sumOfTotalOverlapPairs}");

Console.WriteLine($"Part Two : {sumOfPartialOverlapPairs}");


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

public class RangeOfSections {
    public int IdStart {get; set;}
    public int IdEnd {get; set;}

    public RangeOfSections(int idStart, int idEnd)
    {
        IdStart = idStart;
        IdEnd = idEnd;
    }
}
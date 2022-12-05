using System.Text.RegularExpressions;
using System.Net;

string inputFIlename = "intput.txt";

string cookieEnvName = "ADC_COOKIE_VALUE";

string cookieValue = GetAndSetEnvVar(cookieEnvName);

if (!File.Exists(inputFIlename))
{
    GetInputFile(cookieValue, inputFIlename);
}

int sumOfPriorities = 0;

var content = File.ReadAllLines(inputFIlename);

// Solution 1
foreach (string line in content)
{
    string firstCompartment = line.Substring(0, line.Length / 2);
    string secondCompartment = line.Substring(line.Length / 2);
    bool charFound = false;

    foreach (char ch1 in firstCompartment)
    {
        foreach (char ch2 in secondCompartment)
        {
            if (ch1 == ch2)
            {
                if (Char.IsBetween(ch1, 'a', 'z'))
                {
                    sumOfPriorities += (int)ch1 - 96;
                }
                else // Upper Case
                {
                    sumOfPriorities += (int)ch1 - 38;
                }

                charFound = true;
                break;
            }
        }

        if (charFound)
            break;
    }
}

Console.WriteLine(sumOfPriorities);


// Solution 2

sumOfPriorities = 0;
List<string> rucksacks = new();

foreach (string line in content)
{
    rucksacks.Add(line);

    if (rucksacks.Count == 3)
    {
        bool charFound = false;

        foreach (char ch1 in rucksacks[0])
        {
            foreach (char ch2 in rucksacks[1])
            {
                if (ch1 == ch2)
                {
                    foreach (char ch3 in rucksacks[2])
                    {
                        if (ch1 == ch3)
                        {
                            if (Char.IsBetween(ch1, 'a', 'z'))
                            {
                                sumOfPriorities += (int)ch1 - 96;
                            }
                            else // Upper Case
                            {
                                sumOfPriorities += (int)ch1 - 38;
                            }

                            charFound = true;
                            break;
                        }
                    }
                }

                if (charFound)
                    break;
            }

            if (charFound)
                break;
        }

        rucksacks = new();
    }
}

Console.WriteLine(sumOfPriorities);



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
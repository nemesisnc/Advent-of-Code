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


Console.WriteLine($"Part one: {ComputeSolution(content, false)}");

Console.WriteLine($"Part two: {ComputeSolution(content, true)}");


static int ComputeSolution(string[] content, bool withFloor)
{
    int maxX = 1000;
    int maxY = 200;
    int sandPouringFromX = 500;
    int floorY = 0;

    //var slice = new char[500,maxY];

    // We use a jagged array instead to get the benefit of range and indices 
    char[][] slice = new char[maxX][];
    for (int i = 0; i < maxX; i++)
    {
        slice[i] = new char[maxY];
    }

    Regex regex = new(@"(?:(?'x'\d+),(?'y'\d+)(?: -> )?)+");

    foreach (string line in content)
    {
        Match match = regex.Match(line);

        for (int i = 0; i < match.Groups["x"].Captures.Count; i++)
        {
            int currentX = int.Parse(match.Groups["x"].Captures[i].Value);
            int currentY = int.Parse(match.Groups["y"].Captures[i].Value);

            if (currentY > floorY)
                floorY = currentY;

            if (i == 0)
            {
                slice[currentX][currentY] = 'X';
            }
            else
            {
                int lastX = int.Parse(match.Groups["x"].Captures[i - 1].Value);
                int lastY = int.Parse(match.Groups["y"].Captures[i - 1].Value);

                if (lastX != currentX)
                {
                    int startX = lastX > currentX ? currentX : lastX;
                    int endX = lastX > currentX ? lastX : currentX;

                    for (int x = startX; x <= endX; x++)
                    {
                        slice[x][currentY] = 'X';
                    }
                }
                else
                {
                    int startY = lastY > currentY ? currentY : lastY;
                    int endY = lastY > currentY ? lastY : currentY;

                    for (int y = startY; y <= endY; y++)
                    {
                        slice[currentX][y] = 'X';
                    }
                }
            }
        }
    }

    if (withFloor)
    {
        floorY += 2;
        for (int x = 0; x < maxX; x++)
        {
            slice[x][floorY] = 'X';
        }
    }

    int unit = 0;
    bool fallToAbyss = false;

    while (!fallToAbyss)
    {
        int x = sandPouringFromX;
        int y = -1;
        bool retry = true;
        int retryCount = 0;

        while (retry)
        {
            int lastY = y;
            y += slice[x][(y + 1)..].Select((c, index) => new { c, index }).Where(p => p.c != 0).Select(pair => pair.index).FirstOrDefault(maxY - 1 - y);

            // Fall to abyss or unit of sand come to rest from where the sand pour from
            if (y == maxY - 1 || slice[sandPouringFromX][0] == 'o')
            {
                fallToAbyss = true;
                unit -= 1;
                retry = false;
            }
            // Fall down to the left
            else if (slice[x - 1][y + 1] == 0 && lastY != y)
            {
                x--;
            }
            // Fall down to the right
            else if (slice[x + 1][y + 1] == 0 && lastY != y)
            {
                x++;
            }
            // Cant fall down
            else
            {
                slice[x][y] = 'o';
                retry = false;
            }

            retryCount++;
        }

        unit++;
    }


    string outputPath = $@".\output-withFloor-{withFloor}.txt";

    using (var sw = File.CreateText(outputPath))
    {
        for (int y = 0; y < maxY; y++)
        {
            for (int x = 0; x < maxX; x++)
            {
                char c = slice[x][y] == 0 ? '.' : slice[x][y];
                sw.Write(c);

            }
            sw.WriteLine();
        }
    }

    return unit;
}


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

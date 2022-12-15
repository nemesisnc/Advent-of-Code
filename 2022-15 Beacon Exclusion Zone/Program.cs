using System.Text.RegularExpressions;
using System.Net;
using System.Text;

string inputFIlename = "input.txt";

string cookieEnvName = "ADC_COOKIE_VALUE";

string cookieValue = GetAndSetEnvVar(cookieEnvName);

if (!File.Exists(inputFIlename))
{
    GetInputFile(cookieValue, inputFIlename);
}

var content = File.ReadAllLines(inputFIlename);


#region Solutions 

int lookingRow = 2_000_000;
//int lookingRow = 10;

Regex regex = new(@"Sensor at x=(?'sensorX'-?\d+), y=(?'sensorY'\d+): closest beacon is at x=(?'beaconX'-?\d+), y=(?'beaconY'-?\d+)");

List<(int x1, int y1, int x2, int y2)> points = new();

foreach (string line in content)
{
    Match match = regex.Match(line);
    points.Add((
        int.Parse(match.Groups["sensorX"].Value),
        int.Parse(match.Groups["sensorY"].Value),
        int.Parse(match.Groups["beaconX"].Value),
        int.Parse(match.Groups["beaconY"].Value)
    ));
}

int minX = Math.Min(points.Select(p => p.x1).Min(), points.Select(p => p.x2).Min());
int minY = Math.Min(points.Select(p => p.y1).Min(), points.Select(p => p.y2).Min());
int maxX = Math.Max(points.Select(p => p.x1).Max(), points.Select(p => p.x2).Max());
int maxY = Math.Max(points.Select(p => p.y1).Max(), points.Select(p => p.y2).Max());
int maxDistance = points.Select(p => ManhattanDistance(p)).Max();

// Part one
char[] map = new char[maxX-minX+maxDistance*2];

foreach (var point in points)
{
    if (point.y1 == lookingRow)
        map[point.x1-minX+maxDistance] = 'S';

    if (point.y2 == lookingRow)
        map[point.x2-minX+maxDistance] = 'B';

    int mDistance = ManhattanDistance(point);

    for (int x = (mDistance - Math.Abs(lookingRow - point.y1)); x >= 0; x--)
    {
        if (map[point.x1 + x-minX+maxDistance] == 0)
            map[point.x1 + x-minX+maxDistance] = '#';

        if (map[point.x1 - x-minX+maxDistance] == 0)
            map[point.x1 - x-minX+maxDistance] = '#';
    }
}

Console.WriteLine($"Part one: {map.Where(c => c == '#').Count()}");
//Console.WriteLine(map);


// Part two method OK for small arrays (i.e. example)
// var map2 = new int[lookingRow*2][];
// for (int i = 0; i < lookingRow*2; i++)
// {
//     map2[i] = new int[lookingRow*2];
// }


// int tunningFrequency = 0;

// foreach (var point in points)
// {
//     if (point.y1 >= 0 && point.y1 < lookingRow*2 && point.x1 >= 0 && point.x1 < lookingRow*2)
//         map2[point.x1][point.y1] = -1;

//     if (point.y2 >= 0 && point.y2 < lookingRow*2 && point.x2 >= 0 && point.x2 < lookingRow*2)
//         map2[point.x2][point.y2] = -1;

//     int mDistance = ManhattanDistance(point);

//     for (int y = 0; y < lookingRow*2; y++)
//     {
//         for (int x = 0; x < lookingRow*2; x++)
//         {
//             if (mDistance - (Math.Abs(y - point.y1) + Math.Abs(x - point.x1)) >= 0 && map2[x][y] >= 0)
//                 map2[x][y]++;
//         }
//     }

//     for (int y = 0; y < lookingRow*2; y++)
//     {
//         for (int x = 0; x < lookingRow*2; x++)
//         {
//             if (map2[x][y] == 0)
//                 tunningFrequency = x * 4_000_000 + y;
//         }
//     }
// }

// Console.WriteLine($"Part two: {tunningFrequency}");

// string outputPath = $@".\output.txt";

// using (var sw = File.CreateText(outputPath))
// {
//     for (int y = 0; y < lookingRow*2; y++)
//     {
//         for (int x = 0; x < lookingRow*2; x++)
//         {
//             if (map2[x][y] == -1) {
//                 sw.Write('X');
//             }
//             else {
//                 sw.Write(map2[x][y]);
//             }
//         }
//         sw.WriteLine();
//     }
// }


// Part two, without arrays?
// TODO


static int ManhattanDistance((int x1, int y1, int x2, int y2) point)
{
    return Math.Abs(point.x1 - point.x2) + Math.Abs(point.y1 - point.y2);
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

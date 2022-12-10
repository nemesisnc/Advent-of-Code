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

Console.WriteLine($"Part One: {ComputeSolution(2)}");
Console.WriteLine($"Part Two: {ComputeSolution(10)}");

// That's not the right answer; your answer is too high. If you're stuck, make sure you're using the full input data; there are also some general tips on the about page, or you can ask for hints on the subreddit. Please wait one minute before trying again. (You guessed 2356.) [Return to Day 9]


int ComputeSolution(int numberOfKnots)
{
    var knots = new (int X, int Y)[numberOfKnots];

    var tailPositions = new List<(int X, int Y)>();
    tailPositions.Add((0,0));

    foreach (string line in content)
    {
        char direction = line[0];
        int distance = int.Parse(line.Substring(2));

        for (int k = 1;k <= distance; k++)
        {
            switch  (direction)
            {
                case 'U':
                    knots[0].Y++;
                    break;
                case 'D':
                    knots[0].Y--;
                    break;
                case 'L':
                    knots[0].X--;
                    break;
                case 'R':
                    knots[0].X++;
                    break;
            }

            for (int i = 0;i < numberOfKnots - 1;i++)
            {
                if (knots[i].X - knots[i+1].X > 1)
                {
                    knots[i+1].X++;
                    knots[i+1].Y = knots[i].Y;
                }
                else if (knots[i].X - knots[i+1].X < -1)
                {
                    knots[i+1].X--;
                    knots[i+1].Y = knots[i].Y;
                }
                else if (knots[i].Y - knots[i+1].Y > 1)
                {
                    knots[i+1].Y++;
                    knots[i+1].X = knots[i].X;
                }
                else if (knots[i].Y - knots[i+1].Y < -1)
                {
                    knots[i+1].Y--;
                    knots[i+1].X = knots[i].X;
                }

                tailPositions.Add(knots[numberOfKnots-1]);
            }
        }
    }

    return tailPositions.Distinct().Count();
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

class knot {

}

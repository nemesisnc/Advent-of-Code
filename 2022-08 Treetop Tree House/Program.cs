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

int nbColumns = content[0].Length;
int nbRows = content.Count();

List<Tree> trees = new();

for (int row = 0; row < nbRows; row++)
{
    for (int col = 0; col < nbColumns; col++)
    {
        bool visible = false;

        if (row == 0 || col == 0 || row == nbRows - 1 || col == nbColumns - 1)
        {
            visible = true;
        }

        trees.Add(new Tree(row, col, short.Parse(content[row][col].ToString()), visible));            
    }
}

int viewingDistance = 0;

// Scenic score is zero for those on the edges, no need to calculate them
foreach (Tree tree in trees)
{
    // From the top
    for (int row = 0; row < tree.Row; row++)
    {
        if (short.Parse(content[row][tree.Column].ToString()) >= tree.Height)
        {
            viewingDistance = tree.Row - row;
        }

        if (row == tree.Row - 1 && viewingDistance == 0)
        {
            viewingDistance = tree.Row - 0;
            tree.Visible = true;
        } 
    }
    tree.ScenicScore = tree.ScenicScore == 0 ? viewingDistance : tree.ScenicScore * viewingDistance;
    viewingDistance = 0;

    
    // From the bottom
    for (int row = nbRows - 1; row > tree.Row; row--)
    {
        if (short.Parse(content[row][tree.Column].ToString())>= tree.Height)
        {
            viewingDistance = row - tree.Row;
        }

        if (row == tree.Row + 1 && viewingDistance == 0)
        {
            viewingDistance = nbRows - 1 - tree.Row;
            tree.Visible = true;
        } 
    }
    tree.ScenicScore = tree.ScenicScore * viewingDistance;
    viewingDistance = 0;

    // From the left
    for (int col = 0; col < tree.Column; col++)
    {
        if (short.Parse(content[tree.Row][col].ToString()) >= tree.Height)
        {
            viewingDistance = tree.Column - col;
        }

        if (col == tree.Column - 1 && viewingDistance == 0)
        {
            viewingDistance = tree.Column - 0;
            tree.Visible = true;
        } 
    }
    tree.ScenicScore = tree.ScenicScore * viewingDistance;
    viewingDistance = 0;
    
    // From the right
    for (int col = nbColumns - 1; col > tree.Column; col--)
    {
        if (short.Parse(content[tree.Row][col].ToString()) >= tree.Height)
        {
            viewingDistance = col - tree.Column;
        }

        if (col == tree.Column + 1 && viewingDistance == 0)
        {
            viewingDistance = nbColumns - 1 - tree.Column;
            tree.Visible = true;
        } 
    }
    tree.ScenicScore = tree.ScenicScore * viewingDistance;
    viewingDistance = 0;
}

Console.WriteLine($"Part one : {trees.Where(x => x.Visible).Count()}");

Console.WriteLine($"Part two : {trees.MaxBy(x => x.ScenicScore)?.ScenicScore}");

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

record Tree {
    public int Row;
    public int Column;
    public short Height;
    public bool Visible;
    public int ScenicScore;

    public Tree(int row, int column, short height, bool visible)
    {
        Row = row;
        Column = column;
        Height = height;
        Visible = visible;
    } 
}

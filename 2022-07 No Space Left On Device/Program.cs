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

const string cdCommand = "$ cd ";
const string lsCommand = "$ ls";
const string listedDir = "dir ";

Regex regexListedFile = new Regex(@"(?<size>\d+) (?<name>\w+)");

int lineNumber = 0;
Directory outermostDirectory = new(null, "/");
Directory currentDirectory = outermostDirectory;
bool listedFolder = false;

foreach (string line in content)
{
    lineNumber++;

    if (line.StartsWith(cdCommand))
    {   
        string directoryName = line.Substring(cdCommand.Length);

        if (directoryName == ".." && currentDirectory.ParentDirectory != null) {
            currentDirectory = currentDirectory.ParentDirectory;
            listedFolder = false;
        }
        else if (directoryName != outermostDirectory.Name) {
            currentDirectory = currentDirectory.Directories[directoryName];
            listedFolder = false;
        }
    }
    else if (line.StartsWith(lsCommand))
    {
        listedFolder = true;
    }
    else if (line.StartsWith(listedDir) && listedFolder)
    {
        string directoryName = line.Substring(listedDir.Length);
        currentDirectory.Directories.Add(directoryName, new(currentDirectory, directoryName));
    }
    else if (listedFolder) { // Its a listed file
        Match match = regexListedFile.Match(line);
        string name = match.Groups["name"].Value;
        int size = int.Parse(match.Groups["size"].Value);

        currentDirectory.Files.Add((name, size));

        Directory? directory = currentDirectory;
        while (directory != null)
        {
            directory.Size += size;
            directory = directory.ParentDirectory;
        } 
    }
    else {
        throw new NotImplementedException();
    }
}

long totalSizeOfSmallDirs = Directory.Flatten(outermostDirectory).Where(x => x.Size <= 100000).Sum(x => x.Size);

long missingSize = 30_000_000 - (70_000_000 - outermostDirectory.Size);

int smallestDirectoryToDelete = Directory.Flatten(outermostDirectory).Where(x => x.Size >= missingSize).MinBy(x => x.Size)?.Size ?? 0;


Console.WriteLine($"Part One : {totalSizeOfSmallDirs}");
Console.WriteLine($"Part Two : {smallestDirectoryToDelete}");

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

class Directory {
    public string Name {get;}

    public int Size {get; set;}
    public Directory? ParentDirectory {get; set;}
    public Dictionary<string, Directory> Directories {get; set;}

    public List<(string Name, int Size)> Files {get; set;}

    public Directory(Directory? parentDirectory, string name)
    {
        ParentDirectory = parentDirectory;
        Name = name;
        Directories = new();
        Files = new();
    }

    // Source : https://stackoverflow.com/a/22988237
    public static IEnumerable<Directory> Flatten(Directory directory)
    {
        yield return directory;
        if (directory.Directories != null)
        {
            foreach(var child in directory.Directories)
                foreach(var descendant in Flatten(child.Value))
                    yield return descendant;
        }
    }
}
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

int part = 0;

foreach (List<Monkey> monkeys in new [] {ComputeSolution(content, 20, 3), ComputeSolution(content, 10000, 1)})
{
    part++;
    Console.WriteLine($"Part n°{part}: {monkeys.OrderByDescending(m => m.InspectedItemsCounter).Take(2).Select(m => m.InspectedItemsCounter).Aggregate((x, y) => x * y)}");
}


static List<Monkey> ComputeSolution(string[] content, int numberOfRounds, int worryLevelDividedBy)
{
    var monkeyInputs = content.Select((line, index) => new { line, index })
        .GroupBy(x => x.index / 7)
        .Select(g => g.Select(x => x.line))
        .Select(e => e.Aggregate((x, y) => x + '\n' + y));

    List<Monkey> monkeys = new();

    foreach (var monkeyDefinition in monkeyInputs)
    {
        if (monkeyDefinition != null)
            monkeys.Add(new Monkey(monkeyDefinition));
    }

    int superModulo = monkeys.Select(m => m.DivisibleBy).Aggregate((x, y) => x * y);

    for (int i = 1; i <= numberOfRounds; i++)
    {
        foreach (Monkey monkey in monkeys)
        {
            while (monkey.Items.Count > 0)
            {
                (int ThrowedToMonkeyId, Item ThrowedItem) throwValue = monkey.ThrowItem(superModulo, worryLevelDividedBy);

                monkeys.SingleOrDefault(m => m.Id == throwValue.ThrowedToMonkeyId).Items.Enqueue(throwValue.ThrowedItem);
            }
        }
    }

    return monkeys;
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

class Monkey
{
    public Queue<Item> Items;

    public int Id;
    public long InspectedItemsCounter;
    public int DivisibleBy;

    Operation computeExpression;
    int operand;
    int monkeyIdIfTrue;
    int monkeyIdIfFalse;

    public Monkey(string input)
    {
        Items = new();

        Regex regex = new Regex(@"Monkey (?'id'\d+):\n"
        + @"  Starting items: ((?'items'\d+)(, )?)+\n"
        + @"  Operation: new = ((?'pow'old \* old)|(?'mul'old \* \d+)|(?'add'old \+ \d+))\n"
        + @"  Test: divisible by (?'divisibleBy'\d+)\n"
        + @"    If true: throw to monkey (?'ifTrue'\d+)\n"
        + @"    If false: throw to monkey (?'ifFalse'\d+)");

        Match match = regex.Match(input);

        if (match.Success)
        {
            Id = int.Parse(match.Groups["id"].Value);

            foreach (Capture capture in match.Groups["items"].Captures)
            {
                Items.Enqueue(new(int.Parse(capture.Value)));
            }

            if (match.Groups["pow"].Value != "")
            {
                this.computeExpression = Operation.Power;
            }
            else if (match.Groups["mul"].Value != "")
            {
                this.computeExpression = Operation.Multiplication;
                this.operand = int.Parse(match.Groups["mul"].Value.Substring(5));
            }
            else if (match.Groups["add"].Value != "")
            {
                this.computeExpression = Operation.Addition;
                this.operand = int.Parse(match.Groups["add"].Value.Substring(5));
            }
            else
            {
                throw new NotImplementedException();
            }


            DivisibleBy = int.Parse(match.Groups["divisibleBy"].Value);
            this.monkeyIdIfTrue = int.Parse(match.Groups["ifTrue"].Value); ;
            this.monkeyIdIfFalse = int.Parse(match.Groups["ifFalse"].Value); ; ;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public (int ThrowedToMonkeyId, Item ThrowedItem) ThrowItem(int superModulo, int worryLevelDividedBy)
    {
        InspectedItemsCounter++;

        Item item = Items.Dequeue();

        switch (computeExpression)
        {
            case Operation.Power:
                item.WorryLevel = Math.Round((item.WorryLevel * item.WorryLevel) / worryLevelDividedBy, MidpointRounding.ToZero);
                break;

            case Operation.Multiplication:
                item.WorryLevel = Math.Round((item.WorryLevel * operand) / worryLevelDividedBy, MidpointRounding.ToZero);
                break;

            case Operation.Addition:
                item.WorryLevel = Math.Round((item.WorryLevel + operand) / worryLevelDividedBy, MidpointRounding.ToZero);
                break;

            default:
                throw new NotImplementedException();
        }

        // To 'keep your worry levels manageable', I got the solution with help, I need to learn modular arithmetic
        // without this, worry Level get superior than double.MaxValue when raised to the power
        item.WorryLevel %= superModulo;

        if (item.WorryLevel % DivisibleBy == 0)
        {
            return (monkeyIdIfTrue, item);
        }
        else
        {
            return (monkeyIdIfFalse, item);
        }
    }

    public enum Operation
    {
        Power,
        Multiplication,
        Addition
    }

}

struct Item
{
    public double WorryLevel { get; set; }

    public Item(int worryLevel) => WorryLevel = worryLevel;
}
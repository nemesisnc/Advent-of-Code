string path = @".\input.txt";

var content = File.ReadAllLines(path);

int sumCal = 0;
List<int> packedCal = new();

foreach (string line in content)
{
    if (line != "")
    {
        sumCal += Int32.Parse(line);
    }
    else
    {
        packedCal.Add(sumCal);
        sumCal = 0;
    }
}

// Solution 1
Console.WriteLine(packedCal.Max());

// Solution 2
Console.WriteLine(packedCal.OrderByDescending(x => x).Take(3).Sum());
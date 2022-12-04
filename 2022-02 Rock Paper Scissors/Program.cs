string path = "./input.txt";

var content = File.ReadAllLines(path);

int scoreSolution1 = 0;
int scoreSolution2 = 0;

foreach (string line in content)
{
    var plays = line.Split(' ');

    // For solution n°1
    switch (line)
    {
        // Draw
        case "A X":
        case "B Y":
        case "C Z":
            scoreSolution1 += 3 + (int)Enum.Parse<Draw.MyPlayPoint>(plays[1]);
            break;
        // Win
        case "C X":
        case "A Y":
        case "B Z":
            scoreSolution1 += 6 + (int)Enum.Parse<Draw.MyPlayPoint>(plays[1]);
            break;
        // Lose
        default:
            scoreSolution1 += 0 + (int)Enum.Parse<Draw.MyPlayPoint>(plays[1]);
            break;

    }

    // For solution n°2
    switch (line)
    {
        // When I play Rock
        case "B X":
        case "A Y":
        case "C Z":
            scoreSolution2 += 1 + (int)Enum.Parse<Draw.ScoreResult>(plays[1]);
            break;
        // When I play Paper
        case "C X":
        case "B Y":
        case "A Z":
            scoreSolution2 += 2 + (int)Enum.Parse<Draw.ScoreResult>(plays[1]);
            break;
        // When I play Scissor
        default:
            scoreSolution2 += 3 + (int)Enum.Parse<Draw.ScoreResult>(plays[1]);
            break;

    }
}

//  Solution n°1
Console.WriteLine(scoreSolution1);

//  Solution n°2
Console.WriteLine(scoreSolution2);



class Draw
{
    public enum MyPlayPoint
    {
        X = 1,
        Y = 2,
        Z = 3
    }
    public enum ScoreResult
    {
        X = 0,
        Y = 3,
        Z = 6
    }
}
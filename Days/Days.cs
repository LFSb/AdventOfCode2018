using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

public static partial class Days
{
  private const string InputBasePath = @"Days/Input/";

  private static string OutputResult(string part1, string part2)
  {
    return $"{Environment.NewLine}- Part 1: {part1}{Environment.NewLine}- Part 2: {part2}";
  }

  #region Day1

  public static string Day1()
  {
    var current = 0;

    var changes = File.ReadAllLines(InputBasePath + @"Day1.txt").ToArray();

    var freqDict = new Dictionary<int, bool>();

    while (!freqDict.Any(x => x.Value))
    {
      for (var row = 0; row < changes.Length; row++)
      {
        var change = changes[row];

        var op = change[0];
        var number = int.Parse(change.Substring(1));

        switch (op)
        {
          case '+':
            {
              current += number;
            }
            break;
          case '-':
            {
              current -= number;
            }
            break;
        }

        if (freqDict.ContainsKey(current))
        {
          freqDict[current] = true;
          break;
        }
        else
        {
          freqDict.Add(current, false);
        }
      }
    }

    return OutputResult(current.ToString(), current.ToString());
  }

  #endregion

  #region Day 2

  public static string Day2()
  {
    var input = File.ReadAllLines(InputBasePath + @"Day2.txt").ToArray();

    var twoCount = 0;
    var threeCount = 0;

    foreach (var str in input)
    {
      var grouped = str.GroupBy(x => x);

      if (grouped.Any(x => x.Count() == 2))
        twoCount++;
      if (grouped.Any(x => x.Count() == 3))
        threeCount++;
    }

    var testInput = new[] { "abcde", "fghij", "klmno", "pqrst", "fguij", "axcye", "wvxyz" };
    var ham = new CharDifferenceComparer();

    var score = input.ToDictionary(x => x,
    x => input.Select(y => new
    {
      Value = y,
      Score = ham.Compare(x, y)
    }))
    .First(x => x.Value.Any(z => z.Score == 1));

    var val = score.Value.First(x => x.Score == 1).Value;

    var interSectingValue = string.Empty;

    for (var i = 0; i < score.Key.Length; i++)
    {
      if (score.Key[i] == val[i])
      {
        interSectingValue += score.Key[i];
      }
    }

    return OutputResult((twoCount * threeCount).ToString(), interSectingValue);
  }

  public class CharDifferenceComparer : IComparer<string>
  {
    public int Compare(string x, string y)
    {
      return CalculateDifference(Encoding.UTF8.GetBytes(x), Encoding.UTF8.GetBytes(y));
    }
  }

  public static Int32 CalculateDifference(byte[] input1, byte[] input2)
  {
    var score = 0;

    if (input1.Length != input2.Length)
    {
      throw new InvalidDataException("Input lengths are not equal. Aborting...");
    }

    for (var idx = 0; idx < input1.Length; idx++)
    {
      if (input1[idx] != input2[idx])
      {
        score++;
      }
    }

    return score;
  }

  #endregion

  #region Day 3

  public static string Day3()
  {
    var testClaims = new List<Claim>{
      new Claim("#1 @ 1,3: 4x4"),
      new Claim("#2 @ 3,1: 4x4"),
      new Claim("#3 @ 5,5: 2x2")
    };

    var claims = File.ReadAllLines(InputBasePath + "Day3.txt").Select(input => new Claim(input)).ToArray();

    var overlappingCoords = new List<Tuple<int, int>>();

    foreach (var claim in claims)
    {
      var otherClaims = claims.Where(x => x.ID != claim.ID);

      foreach (var otherClaim in otherClaims)
      {
        overlappingCoords.AddRange(claim.OverLaps(otherClaim));
      }
    }

    return OutputResult(overlappingCoords.Distinct().Count().ToString(), claims.First(x => x.OverlappedIds.Count() == 0).ID.ToString());
  }

  private class Claim
  {
    public int ID { get; set; }

    public int Left { get; set; }

    public int Top { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public Dictionary<int, List<int>> OccupiedCoordinates { get; set; } //Key is the Y axis, value is the X axis.

    public List<int> OverlappedIds { get; set; }

    public Claim(string input)
    {
      var split = input.Split(' ');

      ID = int.Parse(split[0].Substring(1));

      var pos = split[2].Split(',');

      Left = int.Parse(pos[0]);
      Top = int.Parse(pos[1].TrimEnd(':'));

      var size = split[3].Split('x');

      Width = int.Parse(size[0]);
      Height = int.Parse(size[1]);

      CalculateCoordinates();

      OverlappedIds = new List<int>();
    }

    private void CalculateCoordinates()
    {
      OccupiedCoordinates = new Dictionary<int, List<int>>();

      for (var h = 0; h < Height; h++) //Iterate through the square's height.
      {
        for (var w = 0; w < Width; w++) //But also the square's width.
        {
          if (!OccupiedCoordinates.ContainsKey(h + Top))
          {
            OccupiedCoordinates.Add(h + Top, new List<int> { w + Left });
          }
          else
          {
            OccupiedCoordinates[h + Top].Add(w + Left);
          }
        }
      }
    }

    public string VisualizeGrid(int gridSize)
    {
      var sb = new StringBuilder();

      for (var y = 0; y < gridSize; y++)
      {
        for (var x = 0; x < gridSize; x++)
        {
          if (OccupiedCoordinates.ContainsKey(y))
          {
            if (OccupiedCoordinates[y].Contains(x))
            {
              sb.Append('#');
            }
            else
            {
              sb.Append('.');
            }
          }
          else
          {
            sb.Append('.');
          }
        }

        sb.Append(Environment.NewLine);
      }

      return sb.ToString();
    }

    public List<Tuple<int, int>> OverLaps(Claim otherClaim)
    {
      var overlappingCoords = new List<Tuple<int, int>>();

      if (this.OverlappedIds.Contains(otherClaim.ID))
      {
        return overlappingCoords; //They already overlap, no need to re-check.
      }

      foreach (var othery in otherClaim.OccupiedCoordinates.Keys)
      {
        if (this.OccupiedCoordinates.ContainsKey(othery))
        {
          //Check if there's any Y coord in the other claim that collides with the current claim.
          var intersection = this.OccupiedCoordinates[othery].Intersect(otherClaim.OccupiedCoordinates[othery]);

          if (intersection.Any())
          {
            overlappingCoords.AddRange(intersection.Select(xOverLap => new Tuple<int, int>(othery, xOverLap)));
            this.OverlappedIds.Add(otherClaim.ID);
            otherClaim.OverlappedIds.Add(this.ID);
          }
        }
      }

      return overlappingCoords;
    }
  }

  #endregion

  #region Day 4

  public static string Day4()
  {
    var testLogs = new[]{
      "[1518-11-04 00:46] wakes up",
      "[1518-11-02 00:50] wakes up",
      "[1518-11-03 00:05] Guard #10 begins shift",
      "[1518-11-04 00:02] Guard #99 begins shift",
      "[1518-11-05 00:55] wakes up",
      "[1518-11-01 23:58] Guard #99 begins shift",
      "[1518-11-02 00:40] falls asleep",
      "[1518-11-03 00:29] wakes up",
      "[1518-11-01 00:30] falls asleep",
      "[1518-11-04 00:36] falls asleep",
      "[1518-11-01 00:55] wakes up",
      "[1518-11-05 00:45] falls asleep",
      "[1518-11-05 00:03] Guard #99 begins shift",
      "[1518-11-01 00:05] falls asleep",
      "[1518-11-03 00:24] falls asleep",
      "[1518-11-01 00:25] wakes up",
      "[1518-11-01 00:00] Guard #10 begins shift",
    }; //Randomized to make sure the order of the input doesn't matter.

    var logs = File.ReadAllLines(InputBasePath + "Day4.txt").Select(input => new Log(input)).OrderBy(x => x.DateTime).ToArray();

    //var logs = testLogs.Select(input => new Log(input)).OrderBy(x => x.DateTime).ToArray();

    var guards = logs.Where(log => log.Action.StartsWith("Guard")).GroupBy(x => x.Action).Select(guard => new Guard(guard.First())).ToArray(); //Get the distinct guards.

    foreach(var guard in guards)
    {
      var startLogs = logs.Where(x => x.Action.StartsWith($"Guard #{guard.ID}")).ToArray();

      var guardLogs = new List<Log>();

      foreach(var startLog in startLogs)
      {
        var index = Array.IndexOf(logs, startLog);

        while(!logs[index + 1].Action.StartsWith("Guard"))
        {
          guardLogs.Add(logs[index + 1]);
          index++;
          
          if(index + 1 == logs.Length)
          {
            break;
          }
        }
      }

      guard.CalculateSleepTime(guardLogs);
    }

    var mostAsleep = guards.OrderByDescending(guard => guard.TotalAsleepMinutes).First();

    return OutputResult((mostAsleep.ID * mostAsleep.AsleepMinutes.OrderByDescending(x => x.Value).First().Key).ToString(), string.Empty);
  }

  public class Log
  {
    public DateTime DateTime { get; set; }

    public int Day => DateTime.Day;

    public string Action { get; set; }

    public Log(string input)
    {
      var split = input.Substring(1).Split(']');
      DateTime = DateTime.ParseExact(split[0], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
      Action = split[1].Trim();
    }
  }

  public class Guard
  {
    private const int BatchSize = 2;

    public int ID { get; set; }

    public Dictionary<int, int> AsleepMinutes { get; set; }

    public int TotalAsleepMinutes => AsleepMinutes.Sum(x => x.Value);

    public Guard(Log log)
    {
      ID = int.Parse(log.Action.Split('#')[1].Split(' ')[0]); //The first log should always be the one where the guard begins the shift. This contains the ID.
    }

    public Dictionary<int, int> CalculateSleepTime(List<Log> logs)
    {
      AsleepMinutes = new Dictionary<int, int>();

      //For the next logs, parse them in batches of two and keep track of the difference in minutes between the two times. All the times between the AsleepTime and WakeupTime are the minutes asleep.
      var batches = Math.Ceiling(logs.Count() / (decimal)BatchSize);

      for(var batch = 0; batch < batches; batch++)
      {
        var sleepyTime = logs.Skip(batch * BatchSize).Take(BatchSize);
        var sleeptime = sleepyTime.First();

        var diff = Math.Abs((sleeptime.DateTime - sleepyTime.Skip(1).First().DateTime).Minutes); //The amount of minutes to start counting from the first moment.

        for(var s = 0; s < diff; s++) //-1 because the last minute counts as awake.
        {
          var currentTime = s + sleeptime.DateTime.Minute;

          if(!AsleepMinutes.ContainsKey(currentTime))
          {
            AsleepMinutes.Add(currentTime, 1);
          }
          else
          {
            AsleepMinutes[currentTime]++;
          }
        }
      }

      return AsleepMinutes;
    }
  }

  #endregion
}
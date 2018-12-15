using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

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

    foreach(var claim in claims)
    {
      var otherClaims = claims.Where(x => x.ID != claim.ID);
      
      foreach(var otherClaim in otherClaims)
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

      if(this.OverlappedIds.Contains(otherClaim.ID))
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
}
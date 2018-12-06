using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

public static partial class Days
{
  private static string OutputResult(string part1, string part2)
  {
    return $"{Environment.NewLine}- Part 1: {part1}{Environment.NewLine}- Part 2: {part2}";
  }

  public static string Day1()
  {
    var current = 0;

    var changes = File.ReadAllLines(@"Days/Input/Day1.txt").ToArray();

    var freqDict = new Dictionary<int, bool>();

    while(!freqDict.Any(x => x.Value))
    {
      for(var row = 0; row < changes.Length; row++)
      {
        var change = changes[row];

        var op = change[0];
        var number = int.Parse(change.Substring(1));

        switch(op)
        {
          case '+':
          {
            current += number;
          } break;
          case '-':
          {
            current -= number;
          } break;
        }

        if(freqDict.ContainsKey(current))
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

  public static string Day2()
  {
    var input = File.ReadAllLines(@"Days/Input/Day2.txt").ToArray();

    var twoCount = 0;
    var threeCount = 0;

    foreach(var str in input)
    {
      var grouped = str.GroupBy(x => x);
      
      if(grouped.Any(x => x.Count() == 2))
        twoCount++;
      if(grouped.Any(x => x.Count() == 3))
        threeCount++;
    }
    
    //Not quite right. Comparing wrong?
    var ordered = string.Join("", input.OrderBy(x => x, new HammingComparer()).Take(2).GroupBy(x => x).Where(z => z.Count() > 1));

    return OutputResult((twoCount * threeCount).ToString(), ordered);
  }

  public class HammingComparer : IComparer<string>
  {
    public int Compare(string x, string y)
    {
      return CalculateHammingDistance(Encoding.UTF8.GetBytes(x), Encoding.UTF8.GetBytes(y));
    }
  }

  public static Int32 CalculateHammingDistance(byte[] input1, byte[] input2)
  {
    var score = 0;

    if(input1.Length != input2.Length)
    {
      throw new InvalidDataException("Input lengths are not equal. Aborting...");
    }

    for(var idx = 0; idx < input1.Length; idx++)
    {
      var val = input1[idx] ^ input2[idx];

      while(val != 0)
      {
        score++;
        val &= val - 1;
      }
    }

    return score;
  }
}
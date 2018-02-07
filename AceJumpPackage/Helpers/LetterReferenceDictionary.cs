using System;
using System.Collections.Generic;
using System.Linq;

namespace AceJumpPackage.Helpers
{
  /// <summary>
  ///   Tracks the location of the letters
  /// </summary>
  public class LetterReferenceDictionary
  {
    private const char START_LETTER = 'A';

    private readonly Dictionary<string, int> dictionary = new Dictionary<string, int>();

    private readonly int listOffset;

    private char currentLetter = START_LETTER;

    private string prefix = string.Empty;

    public LetterReferenceDictionary(int totalLocations)
    {
      var numberOfGroups = (int) Math.Ceiling(totalLocations / (double) 26);

      // the offset for the alphabet is number of groups -1
      // if groupgs =1 then a-Z. if groups = 2 then a-y and so on.
      listOffset = numberOfGroups - 1;

      // stop the list off set at B
      if (listOffset > 24) listOffset = 24;
    }

    public int Count => dictionary.Count;

    public char? OffsetKey
    {
      get
      {
        if (listOffset == 0) return null;
        return (char) ('Z' - listOffset);
      }
    }

    public string LastKey => dictionary.Last().Key;

    public string AddSpan(int span)
    {
      var key = string.Concat(prefix, currentLetter.ToString());


      if (!string.IsNullOrEmpty(prefix) && prefix.First() < OffsetKey.Value) return string.Empty;

      dictionary.Add(key, span);
      IncrementKey();


      return key;
    }

    public int GetLetterPosition(string key)
    {
      int span;
      var success = dictionary.TryGetValue(key.ToUpper(), out span);
      if (!success)
        return -1;
      return span;
    }

    public void Reset()
    {
      currentLetter = START_LETTER;
      dictionary.Clear();
    }

    private void IncrementKey()
    {
      if (listOffset > 0)
        if (currentLetter == OffsetKey.Value - 1 && string.IsNullOrEmpty(prefix))
        {
          // then set prefix
          currentLetter = START_LETTER;
          prefix = "Z";
          return;
        }

      if (currentLetter != 'Z')
      {
        currentLetter++;
      }
      else
      {
        // reset
        currentLetter = START_LETTER;

        // decrement prefix
        if (string.IsNullOrEmpty(prefix)) prefix = "Z";
        var prefixChar = prefix.First();
        prefixChar--;
        prefix = prefixChar.ToString();
      }
    }
  }
}
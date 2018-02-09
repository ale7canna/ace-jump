using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using AceJumpPackage.Helpers;
using AceJumpPackage.Interfaces;
using AceJumpPackage.View;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace AceJumpPackage.AceJump
{
  public class AceJump : IAceJumpAdornment
  {
    private IAdornmentLayer aceLayer;


    private string letter;

    private LetterReferenceDictionary letterLocationSpans;
    private IWpfTextView textView;
    private Func<string, char, int, bool> matchingPredicate;

    public bool Active { get; private set; }

    public char? OffsetKey { get; private set; }

    public void HighlightLetter(string letterTofind)
    {
      letter = letterTofind.First().ToString();

      
      var view = textView.TextViewLines;

      var viewText = view.Aggregate("", AggregationStrategy);
      var totalLocations = viewText.Where((t, i) => matchingPredicate(viewText, t, i)).Count();

      letterLocationSpans = new LetterReferenceDictionary(totalLocations);
      OffsetKey = letterLocationSpans.OffsetKey;
      foreach (var line in view)
        CreateVisualsForLetter(line);
    }

    private string AggregationStrategy(string s, ITextViewLine line)
    {
      for (int i = line.Start; i < line.End; ++i)
        s += textView.TextSnapshot[i].ToString();

      return s;
    }

    public void UpdateLetter(string ch)
    {
      foreach (var e in aceLayer.Elements)
      {
        var letterReference = (LetterReference)e.Adornment;
        var reference = (string)letterReference.Content;
        if (!reference.StartsWith(ch.ToUpper()))
          aceLayer.RemoveAdornment(e.Adornment);
        else
        {
          letterReference.UpdateHighlight(ch);
        }
      }
    }

    public void ClearAdornments()
    {
      Active = false;
      letter = string.Empty;
      if (letterLocationSpans != null)
        letterLocationSpans.Reset();
      aceLayer.RemoveAllAdornments();
    }

    public void ShowSelector()
    {
      int cursorPoint = textView.Caret.Position.BufferPosition;
      if (textView.Caret.ContainingTextViewLine.End.Position == cursorPoint) cursorPoint = cursorPoint - 1;

      var point = new SnapshotPoint(textView.TextSnapshot, cursorPoint);
      var point2 = new SnapshotPoint(textView.TextSnapshot, cursorPoint + 1);
      var span = new SnapshotSpan(point, point2);

      // Align the image with the top of the bounds of the text geometry
      var g = textView.TextViewLines.GetMarkerGeometry(span);
      if (g != null)
      {
        var aceJumperControl = new AceJumperControl(textView);
        Canvas.SetLeft(aceJumperControl, g.Bounds.Left);
        Canvas.SetTop(aceJumperControl, g.Bounds.Top);

        //aceLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, span, null, aceJumperControl, null);
        Active = true;
      }
    }

    public void JumpTo(string key)
    {
      if (string.IsNullOrEmpty(key)) return;

      var newCaretPostion = GetLetterPosition(key.ToUpper());
      var isValidPoint = newCaretPostion >= 0 &&
                         newCaretPostion <= textView.TextSnapshot.Length;

      if (isValidPoint)
      {
        var snapshotPoint = new SnapshotPoint(textView.TextSnapshot, newCaretPostion);
        textView.Caret.MoveTo(snapshotPoint);
      }
    }

    public int GetLetterPosition(string key)
    {
      return letterLocationSpans.GetLetterPosition(key);
    }

    public void SetView(IWpfTextView wpfTextView)
    {
      textView = wpfTextView;
      aceLayer = textView.GetAdornmentLayer("AceJump");
    }

    private void CreateVisualsForLetter(ITextViewLine line)
    {
      //grab a reference to the lines in the current TextView 
      var textViewLines = textView.TextViewLines;
      int start = line.Start;
      int end = line.End;

      //Loop through each character, and place a box over item 
      for (var i = start; i < end; ++i)
      {
        var text = i > 0 ? new string(new []{ textView.TextSnapshot[i - 1], textView.TextSnapshot[i] }) : textView.TextSnapshot[i].ToString();
        var @char = textView.TextSnapshot[i];
        var index = text.Length - 1;
        if (matchingPredicate(text, @char, index))
        {
          var span = new SnapshotSpan(textView.TextSnapshot, Span.FromBounds(i, i + 1));

          var g = textViewLines.GetMarkerGeometry(span);
          if (g != null)
          {
            // save the location of this letterTofind to jump to later
            var key = letterLocationSpans.AddSpan(span.Start);


            // Align the image with the top of the bounds of the text geometry
            var letterReference = new LetterReference(key, g.Bounds, 12);
            Canvas.SetLeft(letterReference, g.Bounds.Left);
            Canvas.SetTop(letterReference, g.Bounds.Top);

            aceLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, letterReference, null);
          }
        }
      }
    }

    private double GetFontSize(ITextViewLine line, int i)
    {
      var indexOfTextLine = textView.TextViewLines.GetIndexOfTextLine(line);
      var formatLineInVisualBuffer =
        textView.FormattedLineSource.FormatLineInVisualBuffer(
          textView.VisualSnapshot.GetLineFromLineNumber(indexOfTextLine));
      var textRunProperties =
        formatLineInVisualBuffer.First().GetCharacterFormatting(new SnapshotPoint(textView.TextSnapshot, i));
      var fontRenderingEmSize = textRunProperties.FontRenderingEmSize;

      return fontRenderingEmSize;
    }

    public void SetMode(MenuCommand menuCommand)
    {
      if (menuCommand.CommandID.ID == AceJumpCommand.CommandId)
        SetLetterMode();
      else if (menuCommand.CommandID.ID == AceJumpCommand.CommandWordId)
        SetWordMode();
    }


    public void SetWordMode()
    {
      var predicate = UpperOrLowerPredicate();
      matchingPredicate = (s, c, i) => i > 0 ? predicate(c, letter) && !s[i - 1].IsAlphaNumeric() : predicate(c, letter);
    }

    private Func<char, string, bool> UpperOrLowerPredicate()
    {
      return (c, s) => s.ToLower() == s ? c.ToString().ToLower() == letter : c.ToString() == letter;
    }

    public void SetLetterMode()
    {
      var predicate = UpperOrLowerPredicate();
      matchingPredicate = (s, c, i) => predicate(c, letter);
    }

  }
}
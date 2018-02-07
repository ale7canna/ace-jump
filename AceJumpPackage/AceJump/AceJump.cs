using System;
using System.Collections.Generic;
using System.Linq;
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

    public bool Active { get; private set; }

    public char? OffsetKey { get; private set; }

    public void HighlightLetter(string letterTofind)
    {
      letter = letterTofind.First().ToString();

      Func<char, bool> predicate;
      if (letter.ToLower() == letter)
      {
        predicate = c => c.ToString().ToLower() == letter;
      }
      else
      {
        predicate = c => c.ToString() == letter;
      }

      var view = textView.TextViewLines;

      var viewText = "";
      foreach (var line in view)
      {
        int start = line.Start;
        int end = line.End;
        for (var i = start; i < end; ++i)
          viewText += textView.TextSnapshot[i].ToString();
      }
      
      //var lines = view.Select(line =>
      //{
      //  string s = textView.TextSnapshot.ToString();
      //  return s.Substring(line.Start.Position, line.End.Position);
      //}).ToList();

      //var viewText = lines.Aggregate("", (t, l) => t + l);

      //var totalLocations = text.Count(predicate);
      var totalLocations = viewText.Count(predicate);
      
      letterLocationSpans = new LetterReferenceDictionary(totalLocations);
      OffsetKey = letterLocationSpans.OffsetKey;
      foreach (var line in view) CreateVisualsForLetter(line);
    }

    public void UpdateLetter(string ch)
    {
      for (var i = 0; i < aceLayer.Elements.Count; i++)
      {
        //
        if (aceLayer.Elements[i].Adornment is LetterReference == false)
          continue;

        var letterReference = (LetterReference) aceLayer.Elements[i].Adornment;
        letterReference.UpdateHighlight(ch);
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

        aceLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, span, null, aceJumperControl, null);
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
        if (textView.TextSnapshot[i].ToString() == letter)
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
  }
}
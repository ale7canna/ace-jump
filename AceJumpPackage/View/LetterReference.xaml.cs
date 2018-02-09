using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AceJumpPackage.View
{
  /// <summary>
  ///   Interaction logic for LetterReference.xaml
  /// </summary>
  public partial class LetterReference : UserControl
  {
    public const int PADDING = 0;

    public LetterReference(string referenceLetter, Rect bounds, double fontRenderingEmSize)
    {
      InitializeComponent();

      Content = referenceLetter.ToUpper();
      Background = Brushes.DarkRed;
      Foreground = Brushes.White;

      // give letters like 'M' and 'W' some room
      Width = bounds.Width * referenceLetter.Length + PADDING * 2 + 0;
      Height = bounds.Height * 0.8;

      // make it stand out
      FontWeight = FontWeights.Regular;

      //
      FontSize = fontRenderingEmSize;
    }

    public void UpdateHighlight(string referenceLetter)
    {
      var s = (string) Content;
      if (s.StartsWith(referenceLetter.ToUpper()))
        Background = Brushes.DarkRed;
    }
  }
}
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
    public const int PADDING = 2;

    public LetterReference(string referenceLetter, Rect bounds, double fontRenderingEmSize)
    {
      InitializeComponent();

      Content = referenceLetter.ToUpper();
      Background = Brushes.GreenYellow;

      // give letters like 'M' and 'W' some room
      Width = bounds.Width * referenceLetter.Length + PADDING * 2 + 0;
      Height = bounds.Height;

      // make it stand out
      FontWeight = FontWeights.Bold;

      //
      FontSize = fontRenderingEmSize;
    }

    public void UpdateHighlight(string referenceLetter)
    {
      var s = (string) Content;
      if (s.StartsWith(referenceLetter.ToUpper()))
        Background = Brushes.Yellow;
    }
  }
}
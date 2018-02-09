using System.Text.RegularExpressions;

namespace AceJumpPackage.Helpers
{
  public static class Extensions
  {
    private static readonly Regex Regex = new Regex(@"[a-zA-Z0-9]");

    public static bool IsAlphaNumeric(this char charToCheck)
    {
      return Regex.IsMatch(charToCheck.ToString());
    }

  }
}

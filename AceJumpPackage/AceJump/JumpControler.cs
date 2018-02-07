using System;
using AceJumpPackage.Interfaces;

namespace AceJumpPackage.AceJump
{
  public class JumpControler
  {
    private readonly IAceJumpAdornment aceJump;

    private bool letterHighLightActive;

    private bool offsetKeyPressed;

    private char previouskeypress;

    public JumpControler(IAceJumpAdornment aceJump)
    {
      this.aceJump = aceJump;
    }

    /// <summary>
    ///   Controls the jump.
    /// </summary>
    /// <param name="key">The pressed key.</param>
    /// <returns><c>true</c> if we moved the cursor (i.e. we jumped) or aborted else <c>false</c></returns>
    /// <exception cref="ArgumentNullException">aceJump is not set</exception>
    public bool ControlJump(char? key)
    {
      if (aceJump == null) throw new ArgumentNullException("aceJump is not set");

      if (aceJump != null && aceJump.Active)
      {
        if (!key.HasValue) return false;

        if (key == '\0')
        {
          Close();
          return true;
        }


        if (letterHighLightActive)
        {
          if (char.ToUpper(key.Value) >= aceJump.OffsetKey &&
              !offsetKeyPressed)
          {
            offsetKeyPressed = true;
            previouskeypress = key.Value;
            aceJump.UpdateLetter(key.Value.ToString());
            return false;
          }

          if (offsetKeyPressed)
            JumpCursor(string.Format("{0}{1}", previouskeypress, key.Value));
          else
            JumpCursor(key.Value.ToString());
          return true;
        }

        aceJump.HighlightLetter(key.ToString());

        letterHighLightActive = true;
        return false;
      }

      return false;
    }

    public bool Active()
    {
      return aceJump.Active;
    }

    public void Close()
    {
      letterHighLightActive = false;
      offsetKeyPressed = false;
      aceJump.ClearAdornments();
    }

    public void ShowJumpEditor()
    {
      if (aceJump.Active)
        Close();
      else
        aceJump.ShowSelector();
    }

    private void JumpCursor(string jumpKey)
    {
      // they have highlighted all letters so they are ready to jump
      aceJump.JumpTo(jumpKey.ToUpper());
      Close();
    }
  }
}
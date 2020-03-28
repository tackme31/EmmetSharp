using EmmetSharp.Models;
using EmmetSharp.Renderer;
using System;

namespace EmmetSharp
{
    public static class Emmet
    {
        public static string Render(string abbreviation)
        {
            if (string.IsNullOrWhiteSpace(abbreviation))
            {
                throw new ArgumentException($"The argument '{nameof(abbreviation)}' should be not null.");
            }

            return AbbreviationRenderer.Render(abbreviation);
        }
    }
}

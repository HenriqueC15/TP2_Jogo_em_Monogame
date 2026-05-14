using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jogo
{
    /// <summary>
    /// Renderiza texto usando um sprite sheet de fonte
    /// </summary>
    public class SpriteSheetFont
    {
        private Texture2D fontTexture;
        private int charWidth;
        private int charHeight;
        private int charsPerRow;

        // Mapa ASCII: começa em espaço (32) até ~
        private const int FIRST_CHAR = 32;
        private const int LAST_CHAR = 126;

        public SpriteSheetFont(Texture2D texture, int charWidth, int charHeight, int charsPerRow)
        {
            this.fontTexture = texture;
            this.charWidth = charWidth;
            this.charHeight = charHeight;
            this.charsPerRow = charsPerRow;
        }

        /// <summary>
        /// Desenha texto usando o sprite sheet
        /// </summary>
        public void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale = 1.0f)
        {
            if (string.IsNullOrEmpty(text) || fontTexture == null)
                return;

            Vector2 currentPos = position;

            foreach (char c in text)
            {
                if (c == '\n')
                {
                    currentPos.X = position.X;
                    currentPos.Y += charHeight * scale;
                    continue;
                }

                if (c < FIRST_CHAR || c > LAST_CHAR)
                    continue;

                int charIndex = c - FIRST_CHAR;
                int sourceX = (charIndex % charsPerRow) * charWidth;
                int sourceY = (charIndex / charsPerRow) * charHeight;

                Rectangle sourceRect = new Rectangle(sourceX, sourceY, charWidth, charHeight);

                spriteBatch.Draw(
                    fontTexture,
                    currentPos,
                    sourceRect,
                    color,
                    0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    0f
                );

                currentPos.X += charWidth * scale;
            }
        }

        /// <summary>
        /// Mede o tamanho do texto em pixels
        /// </summary>
        public Vector2 MeasureString(string text, float scale = 1.0f)
        {
            if (string.IsNullOrEmpty(text))
                return Vector2.Zero;

            float width = 0;
            float maxWidth = 0;
            float height = charHeight * scale;

            foreach (char c in text)
            {
                if (c == '\n')
                {
                    if (width > maxWidth)
                        maxWidth = width;
                    width = 0;
                    height += charHeight * scale;
                    continue;
                }

                width += charWidth * scale;
            }

            if (width > maxWidth)
                maxWidth = width;

            return new Vector2(maxWidth, height);
        }
    }
}

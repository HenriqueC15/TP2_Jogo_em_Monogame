using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Jogo
{
    internal class Player
    {
        Texture2D textura;
        public Vector2 posicao;
        float speed; // pixels por segundo

        public Player(Texture2D textura)
        {
            this.textura = textura;
            // inicializa num ponto visível (ajuste conforme necessário)
            posicao = new Vector2(500, 250);
            speed = 300f;
        }

        // agora recebe gameTime e calcula movimento por segundo
        public void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            Vector2 dir = Vector2.Zero;

            if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left)) dir.X -= 1;
            if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right)) dir.X += 1;
            if (ks.IsKeyDown(Keys.W) || ks.IsKeyDown(Keys.Up)) dir.Y -= 1;
            if (ks.IsKeyDown(Keys.S) || ks.IsKeyDown(Keys.Down)) dir.Y += 1;

            // Se quiser bloquear diagonais (apenas 4 direções), descomente:
            // if (dir.X != 0) dir.Y = 0;

            if (dir != Vector2.Zero)
            {
                dir.Normalize(); // mantém velocidade constante em diagonais (se permitido)
                posicao += dir * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // limita dentro da janela (valores hardcoded conforme Game1)
            //posicao.X = MathHelper.Clamp(posicao.X, 0, 1280 - textura.Width);
            //posicao.Y = MathHelper.Clamp(posicao.Y, 0, 720 - textura.Height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textura, new Rectangle((int)posicao.X, (int)posicao.Y, 150, 150), Color.White);
        }
    }
}

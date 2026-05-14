using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jogo
{
    internal class DialogBox
    {
        private Texture2D texturaBarra;
        private SpriteSheetFont fonte;
        private string texto;
        private float tempoVida; // tempo restante (segundos)
        private float tempoMaximo; // tempo total do diálogo
        private Vector2 posicao;

        public bool Ativo { get; private set; }

        public DialogBox(Texture2D barraTextura, SpriteSheetFont fonte, string texto, float duracao, Vector2 posicao)
        {
            this.texturaBarra = barraTextura;
            this.fonte = fonte;
            this.texto = texto;
            this.tempoMaximo = duracao;
            this.tempoVida = duracao;
            this.posicao = posicao;
            this.Ativo = true;
        }

        public void Update(GameTime gameTime)
        {
            if (!Ativo) return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            tempoVida -= delta;

            if (tempoVida <= 0)
            {
                Ativo = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Ativo) return;

            // Desenha a barra de fundo
            if (texturaBarra != null)
            {
                spriteBatch.Draw(texturaBarra, posicao, Color.White);
            }

            // Desenha o texto com a sprite sheet font
            if (fonte != null)
            {
                fonte.DrawString(spriteBatch, texto, posicao + new Vector2(20, 20), Color.Black, scale: 1.0f);
            }
        }
    }
}

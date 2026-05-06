using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jogo
{
    internal class Player
    {
        Texture2D textura;
        Vector2 posicao;
        Vector2 velocidade;
        float speed;

        public Player(Texture2D textura)
        {
            this.textura = textura;
            speed = 2;
        }

        //movimentação do player
        public void Update()
        {
            KeyboardState keypress = Keyboard.GetState();
            if (keypress.IsKeyDown(Keys.A))
            {
                velocidade.X -= speed;
            }
            if (keypress.IsKeyDown(Keys.D))
            {
                velocidade.X += speed;
            }
            if (keypress.IsKeyDown(Keys.W))
            {
                velocidade.Y -= speed;
            }
            if (keypress.IsKeyDown(Keys.S))
            {
                velocidade.Y += speed;
            }
            posicao += velocidade * speed;
            velocidade = Vector2.Zero;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(textura, posicao, Color.White);
        }
    }
}

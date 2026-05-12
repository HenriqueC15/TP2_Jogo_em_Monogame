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
    internal class Inimigo
    {
        Texture2D textura;
        public Vector2 posicao;
        float speed; // pixels por segundo
        public int vida;
        float viewRadius; // distância em que começa a perseguir
        float stopDistance; // distância mínima para "parar" perto do jogador

        public bool IsAlive => vida > 0;

        public Inimigo(Texture2D textura, Vector2 posicaoInicial, float speed = 120f, int vida = 10, float viewRadius = 500f, float stopDistance = 40f)
        {
            this.textura = textura;
            this.posicao = posicaoInicial;
            this.speed = speed;
            this.vida = vida;
            this.viewRadius = viewRadius;
            this.stopDistance = stopDistance;
        }

        // chama por frame, passando a posição do jogador
        public void Update(GameTime gameTime, Vector2 playerPos)
        {
            if (!IsAlive) return;

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 toPlayer = playerPos - posicao;
            float dist = toPlayer.Length();

            if (dist <= viewRadius && dist > stopDistance)
            {
                toPlayer.Normalize();
                posicao += toPlayer * speed * delta;
            }

            // opcional: limites do mapa (ajuste conforme seu mundo)
            posicao.X = MathHelper.Clamp(posicao.X, 0, 3800 - 150);
            posicao.Y = MathHelper.Clamp(posicao.Y, 0, 3500 - 150);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsAlive) return;

            Color tint = Color.White;
            if (vida <= 2) tint = Color.OrangeRed; // indicador simples de dano
            spriteBatch.Draw(textura, new Rectangle((int)posicao.X, (int)posicao.Y, 150, 150), tint);
        }

        public void ReceberDano(int dano)
        {
            vida -= dano;
            if (vida < 0) vida = 0;
        }
    }
}
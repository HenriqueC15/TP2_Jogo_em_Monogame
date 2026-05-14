using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Jogo
{
    internal class Projetil
    {
        public Vector2 Posicao;
        public Vector2 Velocidade;
        public Vector2 PosicaoInicial;

        public Direction Facing { get; private set; }

        // Configurações do Tiro
        public float AlcanceMaximo;
        public bool Ativo;
        public float Dano = 5;

        // Colisão (Hitbox)
        public Rectangle Bounds => new Rectangle((int)Posicao.X + 40, (int)Posicao.Y + 40, 80, 80);

        public Projetil(Vector2 posicaoInicial, Direction facing, float velocidadeTiro, float alcance)
        {
            Posicao = posicaoInicial;
            PosicaoInicial = posicaoInicial;
            AlcanceMaximo = alcance;
            Facing = facing;
            Ativo = true;

            Vector2 direcao = Vector2.Zero;
            switch (facing)
            {
                case Direction.Up:
                    direcao = Vector2.UnitY * -1;
                    break;
                case Direction.Down:
                    direcao = Vector2.UnitY;
                    break;
                case Direction.Left:
                    direcao = Vector2.UnitX * -1;
                    break;
                case Direction.Right:
                    direcao = Vector2.UnitX;
                    break;
            }

            if (direcao != Vector2.Zero)
                direcao.Normalize();

            Velocidade = direcao * velocidadeTiro;
        }

        // Atualização sem mapa (mantido para compatibilidade)
        public void Update(GameTime gameTime)
        {
            if (!Ativo) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Posicao += Velocidade * deltaTime;

            float distanciaPercorrida = Vector2.Distance(PosicaoInicial, Posicao);
            if (distanciaPercorrida >= AlcanceMaximo)
            {
                Ativo = false;
            }
        }

        // Nova sobrecarga: atualiza e verifica colisão com o mapa.
        // Use esta assinatura se quiser que o projétil colida com a textura do mapa.
        public void Update(GameTime gameTime, Map map)
        {
            if (!Ativo) return;

            // Reuse a lógica de movimento
            Update(gameTime);

            // Se colidiu com o mapa, desativa
            if (map != null && map.VerificarColisaoObjeto(Bounds))
            {
                Ativo = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D textura)
        {
            if (Ativo)
            {
                spriteBatch.Draw(textura, Bounds, Color.White);
            }
        }
    }
}

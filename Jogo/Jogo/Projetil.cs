using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Garante que a direção esteja normalizada (tamanho 1) para a velocidade ser constante
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

        public void Update(GameTime gameTime)
        {
            if (!Ativo) return;

            // 1. Move o projétil baseado no tempo (Independente de FPS)
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Posicao.X += Velocidade.X * deltaTime;
            Posicao.Y += Velocidade.Y * deltaTime;

            // 2. Verifica se passou da distância máxima
            float distanciaPercorrida = Vector2.Distance(PosicaoInicial, Posicao);
            if (distanciaPercorrida >= AlcanceMaximo)
            {
                Ativo = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D textura)
        {
            if (Ativo)
            {
                // Desenha o tiro (aqui usamos a cor amarela para destacar)
                spriteBatch.Draw(textura, Bounds, Color.Purple);
            }
        }

    }
}

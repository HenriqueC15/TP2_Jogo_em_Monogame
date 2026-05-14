using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Jogo
{
    // Boss estático: vida = 100, não se move, atira projéteis contra o Player em direções cardeais.
    internal class Boss
    {
        private Texture2D textura;
        public Vector2 posicao;
        public int vida = 100;

        private float shootTimer = 0f;
        private readonly float shootInterval; // segundos entre tiros
        private readonly float detectRange; // distância máxima para começar a atirar
        private readonly float projectileSpeed;
        private readonly float projectileRange;

        public bool IsAlive => vida > 0;

        public Boss(Texture2D textura, Vector2 posicaoInicial, float shootIntervalSeconds = 2.0f, float detectRange = 1200f, float projectileSpeed = 400f, float projectileRange = 1200f)
        {
            this.textura = textura;
            this.posicao = posicaoInicial;
            this.shootInterval = Math.Max(0.1f, shootIntervalSeconds);
            this.detectRange = detectRange;
            this.projectileSpeed = projectileSpeed;
            this.projectileRange = projectileRange;
            this.shootTimer = this.shootInterval * 0.5f; // pequeno atraso inicial
        }

        public Rectangle Hitbox
        {
            get
            {
                // ajusta conforme a sua textura; usa 200x200 por padrão
                return new Rectangle((int)posicao.X, (int)posicao.Y, 400, 400);
            }
        }

        // Atualiza estado e atira em direções cardeais, adicionando os projéteis à lista fornecida.
        public void Update(GameTime gameTime, Player player, List<Projetil> projetis)
        {
            if (!IsAlive) return;
            if (player == null || projetis == null) return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            shootTimer -= delta;
            if (shootTimer > 0f) return;

            // Só atira se o jogador estiver dentro do alcance de detecção
            Vector2 toPlayer = player.posicao - posicao;
            float dist = toPlayer.Length();
            if (dist > detectRange)
            {
                shootTimer = 0.5f;
                return;
            }

            // Escolhe a direção cardinal mais próxima do vetor para o jogador
            Direction dir;
            if (Math.Abs(toPlayer.X) >= Math.Abs(toPlayer.Y))
            {
                dir = toPlayer.X >= 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                dir = toPlayer.Y >= 0 ? Direction.Down : Direction.Up;
            }

            // Ponto de spawn do projétil: centro aproximado do boss
            var spawn = new Vector2(posicao.X + Hitbox.Width / 2f - 40f, posicao.Y + Hitbox.Height / 2f - 40f);

            // Cria e adiciona o projétil marcado como vindo do boss (IsFromBoss = true)
            projetis.Add(new Projetil(spawn, dir, projectileSpeed, projectileRange, isFromBoss: true));

            // reseta o timer
            shootTimer = shootInterval;
        }

        public void ReceberDano(int dano)
        {
            vida -= dano;
            if (vida < 0) vida = 0;
        }

        // Método específico do boss para verificar se o ataque do jogador acertou.
        // Centraliza a lógica de aplicar dano quando o jogador realiza um ataque corpo-a-corpo.
        public void CheckAndApplyPlayerAttack(Player player)
        {
            if (!IsAlive || player == null) return;

            // ConsumeJustAttacked garante que o dano do ataque do jogador é aplicado apenas uma vez
            if (player.IsAttacking && player.ConsumeJustAttacked())
            {
                if (player.AttackHitbox.Intersects(this.Hitbox))
                {
                    ReceberDano(player.dano);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (!IsAlive) return;

            if (pixel != null)
                spriteBatch.Draw(pixel, Hitbox, Color.Red * 0.5f);

            if (textura != null)
                spriteBatch.Draw(textura, new Rectangle((int)posicao.X, (int)posicao.Y, Hitbox.Width, Hitbox.Height), Color.White);
        }
    }
}

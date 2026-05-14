using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Jogo
{
    internal class Inimigo
    {
        Texture2D textura;
        public Vector2 posicao;
        float speed; // pixels por segundo
        public int vida;
        public int dano = 1;
        float viewRadius; // distância em que começa a perseguir
        float stopDistance; // distância mínima para "parar" perto do jogador

        // controle de invulnerabilidade entre acertos
        private float damageCooldownTimer = 0f;
        private const float damageCooldownDuration = 0.6f; // tempo que o inimigo fica imune a repetir o mesmo tipo de dano
        private int lastDamageType = -1;

        // ataque do inimigo
        private float attackCooldownTimer = 0f;
        private const float attackCooldownDuration = 1.0f; // tempo entre ataques do inimigo
        private float attackRange = 60f; // alcance do ataque (ajuste conforme necessário)

        public bool IsAlive => vida > 0;
        public bool takedamage = false;
        public bool isTakedamage = false;
        public Inimigo(Texture2D textura, Vector2 posicaoInicial, int vida, float speed = 250f, float viewRadius = 400f, float stopDistance = 40f)
        {
            this.textura = textura;
            this.posicao = posicaoInicial;
            this.speed = speed;
            this.vida = vida;
            this.viewRadius = viewRadius;
            this.stopDistance = stopDistance;
        }
        public Rectangle Hitbox
        {
            get
            {
                return new Rectangle((int)posicao.X, (int)posicao.Y, 150, 150);
            }
        }

        // agora recebe o Player para poder aplicar dano
        public void Update(GameTime gameTime, Player player)
        {
            if (!IsAlive) return;

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 toPlayer = player.posicao - posicao;
            float dist = toPlayer.Length();

            // movimentação simples: se estiver dentro do raio de visão, mas fora da distância de parada, move em direção ao jogador
            if (dist <= viewRadius && dist > stopDistance)
            {
                toPlayer.Normalize();
                posicao += toPlayer * speed * delta;
            }

            // atualiza cooldown de dano recebido
            if (damageCooldownTimer > 0f)
            {
                damageCooldownTimer -= delta;
                if (damageCooldownTimer < 0f) damageCooldownTimer = 0f;
            }

            // atualiza cooldown de ataque do inimigo
            if (attackCooldownTimer > 0f)
            {
                attackCooldownTimer -= delta;
                if (attackCooldownTimer < 0f) attackCooldownTimer = 0f;
            }

            // lógica de ataque: se estiver perto o bastante e cooldown zerado, aplica dano ao jogador
            if (dist <= attackRange + 70f) // +70 porque a hitbox do inimigo tem 150x150, ajuste se necessário
            {
                if (attackCooldownTimer == 0f)
                {
                    player.receberDano(dano);
                    attackCooldownTimer = attackCooldownDuration;
                }
            }

            // limites do mapa (ajuste conforme seu mundo)
            posicao.X = MathHelper.Clamp(posicao.X, 0, 3800 - 150);
            posicao.Y = MathHelper.Clamp(posicao.Y, 0, 3500 - 150);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (!IsAlive) return;

            Color tint = Color.White;
            if (vida <= 2) tint = Color.OrangeRed; // indicador simples de dano
            if (takedamage) tint = Color.Red; // indicador de dano recente
            spriteBatch.Draw(pixel, Hitbox, Color.Red * 0.5f);
            spriteBatch.Draw(textura, new Rectangle((int)posicao.X, (int)posicao.Y, 150, 150), tint);
        }

        // ReceberDano agora aceita um tipo; se o mesmo tipo chegar durante o cooldown, ignora
        public void ReceberDano(int dano, int tipo = 0)
        {
            if (damageCooldownTimer > 0f && tipo == lastDamageType)
                return;

            vida -= dano;
            if (vida < 0) vida = 0;
            takedamage = true;

            lastDamageType = tipo;
            damageCooldownTimer = damageCooldownDuration;
        }
    }
}
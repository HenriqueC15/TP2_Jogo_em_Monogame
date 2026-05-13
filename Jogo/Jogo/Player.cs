using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Jogo
{
    internal enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    internal class Player
    {
        Texture2D textura;
        public Vector2 posicao;
        float speed; // pixels por segundo
        public int vida;
        public int dano = 2;
        public int tiro = 5;
        public bool IsAttacking = false;
        public bool attackfinish = true;
        public bool takedamage = false;

        List<Projetil> projetis = new List<Projetil>();
        Texture2D projetil;

        public Direction Facing{ get; private set; }

        // estados para ataque e input
        private KeyboardState previousKeyboardState;
        private float attackTimer = 0f;
        private float couldownattack = 0f;
        private float couldowntiro = 0f;
        private const float attackDuration = 0.2f;
        private bool justAttacked = false;

        public Player(Texture2D textura, Vector2 posicaoinicial, int vida)
        {
            this.textura = textura;
            this.vida = vida;
            this.posicao = posicaoinicial;
            speed = 300f;
            Facing = Direction.Down;
            previousKeyboardState = Keyboard.GetState();
        }

        public Rectangle Hitbox
        {
            get
            {
                return new Rectangle((int)posicao.X, (int)posicao.Y, 150, 150);
            }
        }

        // hitbox de ataque baseada na direção do jogador
        public Rectangle AttackHitbox
        {
            get
            {
                switch (Facing)
                {
                    case Direction.Up:
                        return new Rectangle((int)posicao.X, (int)posicao.Y -40, 150, 60);

                    case Direction.Down:
                        return new Rectangle((int)posicao.X, (int)posicao.Y +130, 150, 60);

                    case Direction.Left:
                        return new Rectangle((int)posicao.X - 40, (int)posicao.Y, 60, 150);

                    case Direction.Right:
                        return new Rectangle((int)posicao.X + 130, (int)posicao.Y, 60, 150);
                }

                return Rectangle.Empty;
            }
        }

        // retorna true apenas uma vez quando o ataque começar; consome o flag
        public bool ConsumeJustAttacked()
        {
            if (justAttacked)
            {
                justAttacked = false;
                return true;
            }
            return false;
        }

        public void Update(GameTime gameTime, List<Projetil> projetis)
        {
            KeyboardState currentState = Keyboard.GetState();
            Vector2 dir = Vector2.Zero;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (currentState.IsKeyDown(Keys.A)) dir.X -= 1;
            if (currentState.IsKeyDown(Keys.D)) dir.X += 1;
            if (currentState.IsKeyDown(Keys.W)) dir.Y -= 1;
            if (currentState.IsKeyDown(Keys.S)) dir.Y += 1;

            if (dir != Vector2.Zero)
            {
                if(dir.X > 0) Facing = Direction.Right;
                else if (dir.X < 0) Facing = Direction.Left;
                else if (dir.Y > 0) Facing = Direction.Down;
                else if (dir.Y < 0) Facing = Direction.Up;
                
                dir.Normalize();
                posicao += dir * speed * delta;
            }

            // detectar início do ataque (apenas quando tecla for pressionada neste frame)
            // só inicia ataque se o cooldown estiver zerado
            if (currentState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space) && couldownattack <= 0f)
            {
                IsAttacking = true;
                attackTimer = attackDuration;
                couldownattack = 0.5f;
                attackfinish = false;
                justAttacked = true; // sinaliza que devemos aplicar dano uma vez
            }

            if (currentState.IsKeyDown(Keys.Q) && previousKeyboardState.IsKeyUp(Keys.Q))
            {
                if (couldowntiro <= 0f)
                {
                    projetis.Add(new Projetil(this.posicao, Facing, 400f, 700));
                    couldowntiro = 3f;
                }
            }

            if(couldowntiro > 0f)
            {
                couldowntiro -= delta;
                if (couldowntiro < 0f) couldowntiro = 0f;
            }
            if ( couldownattack > 0f)//somente atualiza timers se estivermos no meio do ataque ou cooldown
            {
                couldownattack -= delta;
                if (couldownattack < 0f) couldownattack = 0f;
                if (couldownattack == 0f) attackfinish = true;
            }
            if (IsAttacking)
            {
                attackTimer -= delta;

                if (attackTimer <= 0f)
                {
                    IsAttacking = false;
                    attackTimer = 0f;
                }
            }

            // atualiza previous para o próximo frame
            previousKeyboardState = currentState;

            // limita dentro do mapa
            posicao.X = MathHelper.Clamp(posicao.X, 16, 3800 - textura.Width);
            posicao.Y = MathHelper.Clamp(posicao.Y, 0, 3500 - textura.Height);
        }

        public void receberDano(int dano)// método público para receber dano
        {
            vida -= dano;
            if (vida < 0) vida = 0;
            takedamage = true; // sinaliza que o jogador recebeu dano
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Color tint = Color.White;
            // desenha hitbox de ataque enquanto o ataque estiver ativo
            if (IsAttacking)
                spriteBatch.Draw(pixel, AttackHitbox, Color.Red * 0.5f);
            if (takedamage) tint = Color.Red; // indicador de dano recente

            spriteBatch.Draw(pixel, Hitbox, Color.Red * 0.5f);
            spriteBatch.Draw(textura, Hitbox, tint);
        }
    }
}

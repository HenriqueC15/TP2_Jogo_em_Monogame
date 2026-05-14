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
        Texture2D texturaWalkRight;
        Texture2D texturaWalkLeft;
        Texture2D texturaWalkUp;
        Texture2D texturaWalkDown;
        Texture2D texturaAttackEspecialUp;
        Texture2D texturaAttackEspecialDown;
        Texture2D texturaAttackEspecialLeft;
        Texture2D texturaAttackEspecialRight;
        Texture2D texturaAttackUp;
        Texture2D texturaAttackDown;
        Texture2D texturaAttackLeft;
        Texture2D texturaAttackRight;

        public Vector2 posicao;
        public int vida;
        public float speed = 300f; // pixels por segundo
        public int dano = 2;
        public int tiro = 5;
        public bool IsAttacking = false;
        public bool attackfinish = true;
        public bool takedamage = false;

        List<Projetil> projetis = new List<Projetil>();
        Texture2D projetil;

        public Direction Facing { get; private set; }

        // estados para ataque e input
        private KeyboardState previousKeyboardState;
        private float attackTimer = 0f;
        private float couldownattack = 0f;
        private float couldowntiro = 0f;
        private const float attackDuration = 0.2f;
        private bool justAttacked = false;

        // animação de andar
        private int frameIndex = 0;
        private float frameTimer = 0f;
        private float frameLength = 0.08f; // tempo entre frames (80ms por padrão)
        private int frameWidth = 150;
        private int frameHeight = 150;
        private int framesPerRow = 8; // número de frames na animação (8 frames)
        private bool isMoving = false;
        private Direction lastDirection = Direction.Down;

        // duração de frames por direção (permite diferentes velocidades)
        private Dictionary<Direction, float> frameDurations = new Dictionary<Direction, float>
        {
            { Direction.Up, 0.08f },
            { Direction.Down, 0.08f },
            { Direction.Left, 0.08f },
            { Direction.Right, 0.08f }
        };

        public Player(Texture2D textura, Vector2 posicaoinicial, int vida, Texture2D walkRight, Texture2D walkLeft, Texture2D walkUp, Texture2D walkDown, Texture2D texturaAttackEspecialDown, Texture2D texturaAttackEspecialLeft, Texture2D texturaAttackEspecialRight, Texture2D texturaAttackDown, Texture2D texturaAttackLeft, Texture2D texturaAttackRight)
        {
            this.textura = textura;
            this.texturaWalkRight = walkRight;
            this.texturaWalkLeft = walkLeft;
            this.texturaWalkUp = walkUp;
            this.texturaWalkDown = walkDown;
            this.vida = vida;
            this.posicao = posicaoinicial;
            Facing = Direction.Down;
            previousKeyboardState = Keyboard.GetState();
            this.texturaAttackEspecialUp = texturaAttackEspecialUp;
            this.texturaAttackEspecialDown = texturaAttackEspecialDown;
            this.texturaAttackEspecialLeft = texturaAttackEspecialLeft;
            this.texturaAttackEspecialRight = texturaAttackEspecialRight;
            this.texturaAttackUp = texturaAttackUp;
            this.texturaAttackDown = texturaAttackDown;
            this.texturaAttackLeft = texturaAttackLeft;
            this.texturaAttackRight = texturaAttackRight;
        }

        public Rectangle Hitbox
        {
            get
            {
                return new Rectangle((int)posicao.X, (int)posicao.Y, 150, 150);
            }
        }

        public Rectangle HitboxColissao
        {
            get
            {
                return new Rectangle((int)posicao.X + 25, (int)posicao.Y + 90, 100, 60);
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
                        return new Rectangle((int)posicao.X, (int)posicao.Y - 40, 150, 60);

                    case Direction.Down:
                        return new Rectangle((int)posicao.X, (int)posicao.Y + 130, 150, 60);

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

        // Atualizado: recebe Map para checar colisões
        public void Update(GameTime gameTime, List<Projetil> projetis, Map map)
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
                if (dir.X > 0) Facing = Direction.Right;
                else if (dir.X < 0) Facing = Direction.Left;
                else if (dir.Y > 0) Facing = Direction.Down;
                else if (dir.Y < 0) Facing = Direction.Up;

                dir.Normalize();
            }

            // atualiza estado de movimento
            isMoving = (dir != Vector2.Zero);

            // atualiza direção quando em movimento
            if (isMoving)
            {
                lastDirection = Facing;
            }

            // atualiza animação: progride frames quando em movimento, mantém frame 0 quando parado
            if (isMoving)
            {
                // obtém a duração de frame para a direção atual
                frameLength = frameDurations[lastDirection];

                frameTimer += delta;
                if (frameTimer >= frameLength)
                {
                    frameTimer = 0f;
                    frameIndex = (frameIndex + 1) % framesPerRow;
                }
            }
            else
            {
                // quando parado, mantém o frame 0 (pose parada) da última direção
                frameIndex = 0;
                frameTimer = 0f;
            }

            // Calcula deslocamento desejado
            Vector2 desloc = dir * speed * delta;

            // Move por eixo e desfaz se houver colisão com o mapa (evita "deslizamento")
            if (desloc.X != 0f)
            {
                posicao.X += desloc.X;
                if (map != null && map.VerificarColisaoObjeto(HitboxColissao))
                {
                    posicao.X -= desloc.X; // revertendo movimento X
                }
            }

            if (desloc.Y != 0f)
            {
                posicao.Y += desloc.Y;
                if (map != null && map.VerificarColisaoObjeto(HitboxColissao))
                {
                    posicao.Y -= desloc.Y; // revertendo movimento Y
                }
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
                    // projétil do jogador (IsFromBoss = false)
                    projetis.Add(new Projetil(this.posicao, Facing, 400f, 700, isFromBoss: false));
                    couldowntiro = 3f;
                }
            }

            if (couldowntiro > 0f)
            {
                couldowntiro -= delta;
                if (couldowntiro < 0f) couldowntiro = 0f;
            }
            if (couldownattack > 0f)//somente atualiza timers se estivermos no meio do ataque ou cooldown
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

            // limita dentro do mapa (ajusta estes valores conforme teu mundo)
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
            spriteBatch.Draw(pixel, HitboxColissao, Color.Blue * 0.5f);

            spriteBatch.Draw(pixel, Hitbox, Color.Red * 0.5f);

            // seleciona spritesheet baseado na última direção
            Texture2D animTextura = lastDirection switch
            {
                Direction.Up => texturaWalkUp,
                Direction.Down => texturaWalkDown,
                Direction.Left => texturaWalkLeft,
                Direction.Right => texturaWalkRight,
                _ => texturaWalkDown
            };

            // calcula o source rectangle para o frame atual (inclui frame 0 quando parado)
            Rectangle source = new Rectangle(frameIndex * frameWidth, 0, frameWidth, frameHeight);
            spriteBatch.Draw(animTextura, Hitbox, source, tint);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Jogo
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Texture2D textura;
        public Texture2D pixel;
        public Texture2D playerlife;

        Player player;
        Camara camera;
        Inimigo inimigo;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            textura = Content.Load<Texture2D>("Crianca");
            pixel = new Texture2D(GraphicsDevice, 1, 1);

            playerlife = new Texture2D(GraphicsDevice, 1, 1);
            playerlife.SetData(new[] { Color.White });

            pixel.SetData(new[] { Color.White });

            player = new Player(textura);
            camera = new Camara(_graphics.GraphicsDevice.Viewport);
            inimigo = new Inimigo(Content.Load<Texture2D>("inimigo_3"), new Vector2(200, 200));
        }

        protected override void Update(GameTime gameTime)
        {
            player.Update(gameTime); // passar gameTime
            camera.Follow(player.posicao); // fazer a câmera seguir o jogador
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            // atualizar o inimigo (se existir)
            if (inimigo != null && inimigo.IsAlive)
            {
                inimigo.Update(gameTime, player); // antes passava apenas player.posicao

                // verificar ataque do jogador contra o inimigo (aplica dano apenas uma vez por ataque)
                // ConsumeJustAttacked garante que aplicamos dano só uma vez no início do ataque
                if (player.IsAttacking && player.ConsumeJustAttacked())
                {
                    var attackCenter = player.AttackHitbox.Center.ToVector2();
                    bool hitByHitbox = player.AttackHitbox.Intersects(inimigo.Hitbox);
                    bool hitByDistance = Vector2.Distance(attackCenter, inimigo.posicao) < 50f;

                    if (hitByHitbox || hitByDistance)
                    {
                        // tipo 1 = ataque corpo-a-corpo do jogador
                        inimigo.ReceberDano(player.dano, tipo: 1);
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.Transform);
            _spriteBatch.Draw(Content.Load<Texture2D>("TesteCasa"), new Rectangle(0, 0, 3800, 3800), Color.White);
            player.Draw(_spriteBatch, pixel);
            if (inimigo != null) inimigo.Draw(_spriteBatch);
            _spriteBatch.End();

            _spriteBatch.Begin();

            _spriteBatch.Draw(pixel, new Rectangle(20, 50, 200, 20), Color.White);
            _spriteBatch.Draw(pixel, new Rectangle(20, 50, player.vida * 10, 20), Color.Red);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

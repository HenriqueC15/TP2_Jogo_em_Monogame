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

            player = new Player(textura, 10);
            camera = new Camara(_graphics.GraphicsDevice.Viewport);
            inimigo = new Inimigo(Content.Load<Texture2D>("Kirbcook"), new Vector2(200, 200), speed: 120f, vida: 10, viewRadius: 600f, stopDistance: 50f);
        }

        protected override void Update(GameTime gameTime)
        {
            player.Update(gameTime); // passar gameTime
            camera.Follow(player.posicao); // fazer a câmera seguir o jogador
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            inimigo.Update(gameTime, player.posicao); // atualizar o inimigo
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.Transform);
            _spriteBatch.Draw(Content.Load<Texture2D>("Casa"), new Rectangle(0, 0, 3800, 3800), Color.White);
            player.Draw(_spriteBatch);
            inimigo.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Jogo
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont font;
        Texture2D textura;
        Texture2D mobilia;
        Texture2D telafim;
        public Texture2D pixel;
        public Texture2D playerlife;

        Player player;
        Boss boss;
        Camara camera;
        Projetil projetil;
        Map mapa;
        List<Projetil> projetis = new List<Projetil>();
        DialogManager dialogManager;
        SpriteSheetFont fonte;

        // Lista de inimigos (pode conter mortos; controlamos spawn/remoção)
        private List<Inimigo> inimigos = new List<Inimigo>();

        // Controle de spawn: requested = condição satisfeita; spawned = já apareceu de fato
        private bool requestedHorda1 = false;
        private bool requestedHorda2 = false;
        private bool requestedHorda3 = false;
        private bool spawnedHorda1 = false;
        private bool spawnedHorda2 = false;
        private bool spawnedHorda3 = false;

        // Posições fixas para os 3 inimigos (ajuste conforme necessário)
        private Vector2 spawnHorda1 = new Vector2(920, 1650);
        private Vector2 spawnHorda2 = new Vector2(770, 3500);
        private Vector2 spawnHorda3 = new Vector2(930, 1180);

        // Estado dos itens/condições (estes devem ser atualizados quando o jogador pegar os itens)
        private bool temVela = false;
        private bool temLanterna = false;
        private bool temFosforos = false;
        private int pilha = 0;
        private bool chave = false;
        private bool fim = false;

        // --- Apenas o código relacionado à coleta de itens ---
        private List<Item> items = new List<Item>();
        private Dictionary<ItemType, Rectangle> collectionZones = new Dictionary<ItemType, Rectangle>();

        // posições fixas dos itens (ajuste conforme o mapa)
        private readonly Vector2 posVela = new Vector2(350, 2450);
        private readonly Vector2 posLanterna = new Vector2(210, 1700);
        private readonly Vector2 posFosforos = new Vector2(210, 1700);
        private readonly Vector2 posPilha1 = new Vector2(970, 2700);
        private readonly Vector2 posChave = new Vector2(100, 1360);
        private readonly Vector2 posPilha2 = new Vector2(1200, 850);
        private readonly Vector2 posBoss = new Vector2(500, 950);

        public bool Bossapareceu = false;

        private bool Gamestarted = false;

        // estágio de progressão de spawn:
        // 0 = vela+lanterna+fosforos (iniciais)
        // 1 = pilha (primeira)
        // 2 = chave
        // 3 = pilha (segunda)
        private int collectionStage = 0;

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
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("File");
            // Carrega as texturas
            mobilia = Content.Load<Texture2D>("Colissoes_mobilia_2");
            mapa = new Map(mobilia, mobilia, Content.Load<Texture2D>("Parede_Colissao"), 3800, 3800, GraphicsDevice);

            textura = Content.Load<Texture2D>("Crianca");
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            boss = new Boss(Content.Load<Texture2D>("Boss"), new Vector2(2970, 300), shootIntervalSeconds: 2.0f, detectRange: 1500f, projectileSpeed: 450f, projectileRange: 1500f);
            mapa.ResetPlayerCheckpoint();

            playerlife = new Texture2D(GraphicsDevice, 1, 1);
            playerlife.SetData(new[] { Color.White });

            pixel.SetData(new[] { Color.White });

            // carrega texturas de animação
            Texture2D walkRight = Content.Load<Texture2D>("crianca_walk_right_sps");
            Texture2D walkLeft = Content.Load<Texture2D>("crianca_walk_left_sps");
            Texture2D walkUp = Content.Load<Texture2D>("crianca_walk_up_sps");
            Texture2D walkDown = Content.Load<Texture2D>("crianca_walk_down_sps");

            // carrega texturas de ataque especial
            Texture2D texturaAttackRight = Content.Load<Texture2D>("ataque_direita_sps");
            Texture2D texturaAttackDown = Content.Load<Texture2D>("ataque_frente_sps");
            Texture2D texturaAttackLeft = Content.Load<Texture2D>("ataque_esquerda_sps");

            Texture2D atackespecialRght = Content.Load<Texture2D>("ataque_especial_direita_sps");
            Texture2D atackespecialLeft = Content.Load<Texture2D>("ataque_especial_esquerda_sps");
            //Texture2D atackespecialUp = Content.Load<Texture2D>("ataque_especial_up_sps");
            Texture2D atackespecialDown = Content.Load<Texture2D>("ataque_especial_frente_sps");

            player = new Player(textura, mapa.PlayerCheckpoint, 25, walkRight, walkLeft, walkUp, walkDown, atackespecialDown, atackespecialLeft, atackespecialRght
                , texturaAttackDown, texturaAttackRight, texturaAttackLeft);
            //boss = new Boss(Content.Load<Texture2D>("Boss"), new Vector2(2970, 300));
            camera = new Camara(_graphics.GraphicsDevice.Viewport);

            // Carrega a fonte do sprite sheet (9x9 = 9 pixels width, 9 pixels height, 16 caracteres por linha)
            Texture2D fontTexture = Content.Load<Texture2D>("9x9 Font Spritesheet");
            fonte = new SpriteSheetFont(fontTexture, charWidth: 9, charHeight: 9, charsPerRow: 16);

            // Inicializa o gestor de diálogos
            dialogManager = new DialogManager();

            // --- Inicialização das zonas e spawn inicial ---
            collectionZones[ItemType.Vela] = new Rectangle(350, 2450, 120, 120);
            collectionZones[ItemType.Lanterna] = new Rectangle(210, 1700, 140, 140);
            collectionZones[ItemType.Fosforos] = new Rectangle(210, 1700, 160, 160);
            collectionZones[ItemType.Pilha] = new Rectangle(970, 2700, 120, 120);
            collectionZones[ItemType.Chave] = new Rectangle(100, 1360, 160, 160);
            collectionZones[ItemType.FIM] = new Rectangle(500, 950, 120, 120);

            // spawna os itens iniciais (vela, lanterna, fósforos)
            SpawnStageItems();

            // Cria o diálogo inicial
            //Texture2D barraDialogo = Content.Load<Texture2D>("barra_de_texto");
            //dialogManager.AddDialog(barraDialogo, fonte, "tenho que encontrar uma fonte de luz", 5f, new Vector2(240, 550));
        }

        protected override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboardState = Keyboard.GetState();
            if (!Gamestarted)
            {
                if (keyboardState.IsKeyDown(Keys.Enter))
                {
                    Gamestarted = true;
                }
                else if (keyboardState.IsKeyDown(Keys.F))
                {
                    Exit();
                }
                else
                {
                    base.Update(gameTime);
                    return; // não atualiza o jogo se ainda não começou
                }
            }

            camera.Follow(player.posicao);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Atualiza o gestor de diálogos
            dialogManager.Update(gameTime);

            // --- Atualiza condições de spawn: marca requested quando condição aparece ---
            if (!spawnedHorda1 && !requestedHorda1 && temVela && temLanterna && temFosforos)
                requestedHorda1 = true;

            if (!spawnedHorda2 && !requestedHorda2 && pilha == 1)
                requestedHorda2 = true;

            if (!spawnedHorda3 && !requestedHorda3 && chave)
                requestedHorda3 = true;

            // Limpa inimigos mortos da lista (opcional)
            for (int i = inimigos.Count - 1; i >= 0; i--)
            {
                if (!inimigos[i].IsAlive)
                    inimigos.RemoveAt(i);
            }

            // Só permite spawn de um novo inimigo quando NÃO houver inimigos vivos
            bool anyAlive = false;
            foreach (var inim in inimigos)
            {
                if (inim.IsAlive) { anyAlive = true; break; }
            }

            if (!anyAlive)
            {
                // spawn na ordem: horda1, depois horda2, depois horda3 — a primeira requested que existir é spawnada
                if (requestedHorda1 && !spawnedHorda1)
                {
                    inimigos.Add(new Inimigo(Content.Load<Texture2D>("inimigo_3"), spawnHorda1, 10));
                    spawnedHorda1 = true;
                    requestedHorda1 = false;
                }
                else if (requestedHorda2 && !spawnedHorda2)
                {
                    inimigos.Add(new Inimigo(Content.Load<Texture2D>("inimigo_1"), spawnHorda2, 10));
                    spawnedHorda2 = true;
                    requestedHorda2 = false;
                }
                else if (requestedHorda3 && !spawnedHorda3)
                {
                    inimigos.Add(new Inimigo(Content.Load<Texture2D>("inimigo_3"), spawnHorda3, 10));
                    spawnedHorda3 = true;
                    requestedHorda3 = false;
                }
            }
            // Atualiza jogador (colisões tratadas internamente)
            player.Update(gameTime, projetis, mapa, inimigos);

            // Atualiza projéteis e checa colisões com todos os inimigos
            for (int i = projetis.Count - 1; i >= 0; i--)
            {
                projetis[i].Update(gameTime);

                // Checar colisão com cada inimigo
                for (int j = 0; j < inimigos.Count; j++)
                {
                    var inim = inimigos[j];
                    if (!inim.IsAlive) continue;

                    if (projetis[i].Bounds.Intersects(inim.Hitbox))
                    {
                        inim.ReceberDano(player.tiro);
                        projetis[i].Ativo = false;
                        break; // projétil já colidiu
                    }
                }

                if (!projetis[i].Ativo)
                    projetis.RemoveAt(i);
            }

            // Atualiza cada inimigo e aplica dano do jogador (ataque corpo a corpo)
            for (int i = inimigos.Count - 1; i >= 0; i--)
            {
                var inim = inimigos[i];
                if (inim.IsAlive)
                {
                    inim.Update(gameTime, player);

                    if (player.IsAttacking && player.ConsumeJustAttacked())
                    {
                        var attackCenter = player.AttackHitbox.Center.ToVector2();
                        bool hitByHitbox = player.AttackHitbox.Intersects(inim.Hitbox);
                        bool hitByDistance = Vector2.Distance(attackCenter, inim.posicao) < 50f;

                        if (hitByHitbox || hitByDistance)
                        {
                            // tipo 1 = ataque corpo-a-corpo do jogador
                            inim.ReceberDano(player.dano, tipo: 1);
                        }
                    }
                }
            }

            // --- Atualização da lógica de coleta de itens ---
            // atualiza animação/estado dos itens
            foreach (var it in items) it.Update(gameTime);

            // coleta ao tocar na hitbox do item (apenas interseção das hitboxes)
            foreach (var it in items)
            {
                if (it.Collected) continue;

                if (!player.Hitbox.Intersects(it.Hitbox)) continue;

                // efetiva coleta
                it.Collect();

                // atualiza flags/estado do jogo conforme o tipo coletado
                switch (it.Type)
                {
                    case ItemType.Vela: SetVela(true); break;
                    case ItemType.Lanterna: SetLanterna(true); break;
                    case ItemType.Fosforos: SetFosforos(true); break;
                    case ItemType.Pilha:
                        if (collectionStage == 1) SetPilha(1);
                        else SetPilha(2);
                        break;
                    case ItemType.Chave: SetChave(true); break;
                    case ItemType.FIM: SetFim(true); break;
                }
            }

            // verifica avanço de estágio:
            bool Collected(ItemType t) => items.Any(i => i.Type == t && i.Collected);

            if (collectionStage == 0)
            {
                if (Collected(ItemType.Vela) && Collected(ItemType.Lanterna) && Collected(ItemType.Fosforos))
                {
                    collectionStage = 1;
                    SpawnStageItems();
                }
            }
            else if (collectionStage == 1)
            {
                if (Collected(ItemType.Pilha))
                {
                    collectionStage = 2;
                    SpawnStageItems();
                }
            }
            else if (collectionStage == 2)
            {
                if (Collected(ItemType.Chave))
                {
                    collectionStage = 3;
                    SpawnStageItems();
                }
            } else if (collectionStage == 3)
            {
                if (boss.IsAlive == false && Collected(ItemType.Chave)) 
                {
                    collectionStage = 4;
                    SpawnStageItems();
                }
            }
            // --- spawn do boss apenas quando a segunda pilha for coletada (pilha == 2)
            if (pilha == 2)
            {
                Bossapareceu = true;
            }
            if (boss != null && boss.IsAlive && Bossapareceu)
            {
                // Atualiza o boss para disparar projéteis contra o player
                boss.Update(gameTime, player, projetis);

                // Delegamos ao boss a verificação e aplicação de dano pelo ataque do jogador
                boss.CheckAndApplyPlayerAttack(player);
                for (int i = projetis.Count - 1; i >= 0; i--)
                {
                    projetis[i].Update(gameTime);

                    if (!boss.IsAlive) continue;

                    if (projetis[i].Bounds.Intersects(boss.Hitbox) && !projetis[i].IsFromBoss)
                    {
                        boss.ReceberDano(player.tiro);
                        projetis[i].Ativo = false;
                        break; // projétil já colidiu
                    }

                    if (projetis[i].Bounds.Intersects(player.Hitbox) && projetis[i].IsFromBoss)
                    {
                        player.receberDano(boss.dano);
                        projetis[i].Ativo = false;
                        break; // projétil já colidiu
                    }

                    if (!projetis[i].Ativo)
                        projetis.RemoveAt(i);
                }
            }

            boss.Update(gameTime, player, projetis);

            if (player != null && boss.IsAlive && Bossapareceu)
            {

                // Delegamos ao boss a verificação e aplicação de dano pelo ataque do jogador
                boss.CheckAndApplyPlayerAttack(player);
                for (int i = projetis.Count - 1; i >= 0; i--)
                {
                    projetis[i].Update(gameTime);

                    //if (!boss.IsAlive) continue;

                    if (projetis[i].Bounds.Intersects(player.Hitbox) && projetis[i].IsFromBoss)
                    {
                        player.receberDano(boss.dano);
                        projetis[i].Ativo = false;
                        break; // projétil já colidiu
                    }

                    if (!projetis[i].Ativo)
                        projetis.RemoveAt(i);
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.Transform);
            _spriteBatch.Draw(Content.Load<Texture2D>("Casa_background"), new Rectangle(0, 0, 3800, 3800), Color.White);
            _spriteBatch.Draw(Content.Load<Texture2D>("Casa_mobilia_1"), new Rectangle(0, 0, 3800, 3800), Color.White);
            _spriteBatch.Draw(Content.Load<Texture2D>("Casa_mobilia_2"), new Rectangle(0, 0, 3800, 3800), Color.White);
            //mapa.Draw(_spriteBatch);

            foreach (var item in projetis)
            {
                if (item.IsFromBoss)
                    item.Draw(_spriteBatch, Content.Load<Texture2D>("ataque_boss"));
                else
                    item.Draw(_spriteBatch, Content.Load<Texture2D>("almofada"));
            }

            player.Draw(_spriteBatch, pixel,Content.Load<Texture2D>("Tela_Morte"), 0);

            // desenha todos os inimigos
            foreach (var inim in inimigos)
            {
                inim.Draw(_spriteBatch, pixel);
            }

            if (Bossapareceu)
            {
                boss.Draw(_spriteBatch, Content.Load<Texture2D>("Boss"));
            }

            _spriteBatch.Draw(Content.Load<Texture2D>("sombra"), new Rectangle(0, 0, 3800, 3800), Color.White);
            // desenha os itens
            foreach (var it in items)
            {
                it.Draw(_spriteBatch, pixel);
            }

            _spriteBatch.End();

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(Content.Load<Texture2D>("bara_vida_1"), new Rectangle(10, 40, 231, 60), Color.White);
            _spriteBatch.Draw(pixel, new Rectangle(25, 60, player.vida * 8, 20), Color.FromNonPremultiplied(24, 0, 24, 255));
            // barra de vida do boss (aparece apenas se o boss tiver sido ativado)
            if (Bossapareceu)
            {
                _spriteBatch.Draw(pixel, new Rectangle(600, 20, 200, 20), Color.White);
                _spriteBatch.Draw(pixel, new Rectangle(600, 20, boss.vida * 2, 20), Color.FromNonPremultiplied(24, 0, 24, 255));
            }

            // Desenha o gestor de diálogos
            dialogManager.Draw(_spriteBatch);

            player.Draw(_spriteBatch, pixel, Content.Load<Texture2D>("Tela_Morte"), 1);
            if(Gamestarted == false)
            {   
                _spriteBatch.Draw(Content.Load<Texture2D>("Tela_menu"), new Rectangle(0, 0, 1280, 720), Color.White);
                _spriteBatch.DrawString(font, "Jogar (Enter)", new Vector2(900, 300), Color.White);
                _spriteBatch.DrawString(font, "Sair Tristemente (F)", new Vector2(900, 350), Color.White);
            }
            if(fim == true)
            {
                _spriteBatch.Draw(Content.Load<Texture2D>("Tela_fim"), new Rectangle(-150, 0, 1500, 720), Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        // --- Métodos públicos para atualizar o estado dos itens (chame quando o jogador recolher) ---
        public void SetVela(bool v) => temVela = v;
        public void SetLanterna(bool v) => temLanterna = v;
        public void SetFosforos(bool v) => temFosforos = v;
        public void SetPilha(int p) => pilha = p;
        public void SetChave(bool v) => chave = v;
        public void SetFim(bool v) => fim = v;

        // Spawna os itens correspondentes ao estágio atual (não duplica itens já existentes)
        private void SpawnStageItems()
        {
            bool ExistsNotCollected(ItemType t) => items.Any(i => i.Type == t && !i.Collected);

            if (collectionStage == 0)
            {
                if (!ExistsNotCollected(ItemType.Vela)) items.Add(new Item(ItemType.Vela, posVela));
                if (!ExistsNotCollected(ItemType.Lanterna)) items.Add(new Item(ItemType.Lanterna, posLanterna));
                if (!ExistsNotCollected(ItemType.Fosforos)) items.Add(new Item(ItemType.Fosforos, posFosforos));
            }
            else if (collectionStage == 1)
            {
                if (!ExistsNotCollected(ItemType.Pilha)) items.Add(new Item(ItemType.Pilha, posPilha1));
            }
            else if (collectionStage == 2)
            {
                if (!ExistsNotCollected(ItemType.Chave)) items.Add(new Item(ItemType.Chave, posChave));
            }
            else if (collectionStage == 3)
            {
                if (!ExistsNotCollected(ItemType.Pilha)) items.Add(new Item(ItemType.Pilha, posPilha2));
            }
            else if (collectionStage == 4)
            {
                if (!ExistsNotCollected(ItemType.FIM)) items.Add(new Item(ItemType.FIM, posBoss));
            }
        }
    }
}

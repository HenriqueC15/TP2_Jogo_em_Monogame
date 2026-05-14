using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Jogo
{
    internal class Map
    {
        // origem do mapa no mundo
        public Vector2 localizacao;

        // textura visual do mapa
        public Texture2D textura;

        // máscaras de colisão (p.ex. mobilia e parede)
        private readonly List<Texture2D> collisionTextures = new List<Texture2D>();
        private readonly List<Color[]> collisionColors = new List<Color[]>();

        private float larguraDesenho;
        private float alturaDesenho;

        private Texture2D texturaDebug;
        public bool MostrarDebug = false;

        // retângulos de colisão em coordenadas do MUNDO (combinação de todas as máscaras)
        public List<Rectangle> CollisionRects { get; private set; }

        // checkpoint do jogador
        public Vector2 PlayerCheckpoint { get; private set; }

        // checkpoints para inimigos
        public List<Vector2> EnemyCheckpoints { get; private set; }

        // Construtor que aceita duas máscaras: mobilia e paredes (ambas 3800x3800 no seu caso)
        public Map(Texture2D texturaVisual, Texture2D colisaoMobilia, Texture2D colisaoParede, float larguraDesejada, float alturaDesejada, GraphicsDevice gd)
        {
            localizacao = Vector2.Zero;
            this.textura = texturaVisual;
            this.larguraDesenho = larguraDesejada;
            this.alturaDesenho = alturaDesejada;

            CollisionRects = new List<Rectangle>();
            EnemyCheckpoints = new List<Vector2>();
            PlayerCheckpoint = new Vector2(450, 2800);

            // adiciona máscaras (pode passar null se não existir)
            if (colisaoMobilia != null) AddCollisionTexture(colisaoMobilia);
            if (colisaoParede != null) AddCollisionTexture(colisaoParede);

            // constrói rects a partir das máscaras
            BuildCollisionRectangles();

            CriarTexturaDebug(gd);
        }

        private void AddCollisionTexture(Texture2D tex)
        {
            collisionTextures.Add(tex);
            var colors = new Color[tex.Width * tex.Height];
            tex.GetData(colors);
            collisionColors.Add(colors);
        }

        private void CriarTexturaDebug(GraphicsDevice gd)
        {
            if (collisionTextures.Count == 0) return;

            int w = collisionTextures[0].Width;
            int h = collisionTextures[0].Height;
            texturaDebug = new Texture2D(gd, w, h);
            Color[] pixels = new Color[w * h];

            // combina as máscaras com cores diferentes:
            // primeiro mask -> vermelho, segundo -> azul. sobreposição -> magenta.
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.Transparent;

            for (int t = 0; t < collisionTextures.Count; t++)
            {
                var colors = collisionColors[t];
                for (int i = 0; i < colors.Length; i++)
                {
                    if (colors[i].A > 0)
                    {
                        if (pixels[i] == Color.Transparent)
                            pixels[i] = (t == 0) ? Color.Red * 0.6f : Color.Blue * 0.6f;
                        else
                            pixels[i] = Color.Magenta * 0.6f; // sobreposição
                    }
                }
            }

            texturaDebug.SetData(pixels);
        }

        // --- Checkpoints ---
        public void SetPlayerCheckpoint(Vector2 pos) => PlayerCheckpoint = pos;
        public void ResetPlayerCheckpoint() => PlayerCheckpoint = new Vector2(320, 2850);

        public void AddEnemyCheckpoint(Vector2 pos) => EnemyCheckpoints.Add(pos);

        /*
        public void AparicaoInimigos(Inimigo inimigo, Player player)
        {
            if(player.Vela && player.Lanterna && player.fosforos)
            {

            }
            else if(player.chave)
            {
                inimigo.posicao = new Vector2(450, 2800); inimigo.posicao.Normalize();
            }

        }*/
        public void ClearEnemyCheckpoints() => EnemyCheckpoints.Clear();

        // --- Construção de CollisionRects a partir de todas as máscaras ---
        private void BuildCollisionRectangles()
        {
            CollisionRects.Clear();
            if (collisionTextures.Count == 0) return;

            // processa cada máscara separadamente e adiciona seus rects convertidos para coordenadas do mundo
            for (int t = 0; t < collisionTextures.Count; t++)
            {
                var tex = collisionTextures[t];
                var colors = collisionColors[t];
                int w = tex.Width;
                int h = tex.Height;

                // runs por linha
                var runsByRow = new List<List<(int x, int width)>>(h);
                for (int y = 0; y < h; y++)
                {
                    var runs = new List<(int x, int width)>();
                    int x = 0;
                    while (x < w)
                    {
                        while (x < w && !IsSolid(colors, w, x, y)) x++;
                        if (x >= w) break;
                        int start = x;
                        while (x < w && IsSolid(colors, w, x, y)) x++;
                        runs.Add((start, x - start));
                    }
                    runsByRow.Add(runs);
                }

                // mescla runs verticalmente
                var activeRects = new List<Rectangle>();
                for (int y = 0; y < h; y++)
                {
                    var newActive = new List<Rectangle>();
                    var runs = runsByRow[y];

                    foreach (var run in runs)
                    {
                        bool extended = false;
                        for (int i = 0; i < activeRects.Count; i++)
                        {
                            var r = activeRects[i];
                            if (r.X == run.x && r.Width == run.width && r.Y + r.Height == y)
                            {
                                r.Height += 1;
                                newActive.Add(r);
                                activeRects[i] = Rectangle.Empty;
                                extended = true;
                                break;
                            }
                        }
                        if (!extended)
                        {
                            newActive.Add(new Rectangle(run.x, y, run.width, 1));
                        }
                    }

                    // finaliza rects não estendidas
                    foreach (var r in activeRects)
                    {
                        if (r != Rectangle.Empty && r.Height > 0) CollisionRects.Add(r);
                    }

                    activeRects = newActive;
                }

                foreach (var r in activeRects)
                    if (r != Rectangle.Empty && r.Height > 0) CollisionRects.Add(r);

                // converte rects de textura para mundo aplicando escala e offset
                float scaleX = larguraDesenho > 0 ? (float)larguraDesenho / tex.Width : 1f;
                float scaleY = alturaDesenho > 0 ? (float)alturaDesenho / tex.Height : 1f;

                var converted = new List<Rectangle>(CollisionRects.Count);
                foreach (var tr in CollisionRects)
                {
                    int wx = (int)Math.Round(localizacao.X + tr.X * scaleX);
                    int wy = (int)Math.Round(localizacao.Y + tr.Y * scaleY);
                    int ww = Math.Max(1, (int)Math.Round(tr.Width * scaleX));
                    int wh = Math.Max(1, (int)Math.Round(tr.Height * scaleY));
                    converted.Add(new Rectangle(wx, wy, ww, wh));
                }

                // substitui a lista por esses rects e segue para próxima máscara
                CollisionRects = converted;
            }
        }

        private bool IsSolid(Color[] colors, int width, int x, int y)
        {
            int idx = x + y * width;
            return colors[idx].A > 0;
        }

        // --- Colisões em runtime: checagem por interseção com os retângulos pré-computados ---
        public bool VerificarColisaoObjeto(Rectangle rect)
        {
            if (rect.IsEmpty) return false;
            for (int i = 0; i < CollisionRects.Count; i++)
            {
                if (rect.Intersects(CollisionRects[i])) return true;
            }
            return false;
        }

        public bool EstaColidindo(Vector2 ponto)
        {
            var p = new Point((int)ponto.X, (int)ponto.Y);
            foreach (var r in CollisionRects)
                if (r.Contains(p)) return true;
            return false;
        }

        // Desenha retângulos de colisão para debug usando um pixel branco (1x1) passado pelo chamador
        public void DrawCollisions(SpriteBatch spriteBatch, Texture2D pixel, Color color)
        {
            if (spriteBatch == null) throw new ArgumentNullException(nameof(spriteBatch));
            if (pixel == null) return;

            foreach (var r in CollisionRects)
                spriteBatch.Draw(pixel, r, color);
        }

        // Desenha o mapa e (opcional) a máscara de colisão em overlay para debug
        public void Draw(SpriteBatch spriteBatch)
        {
            if (spriteBatch == null) throw new ArgumentNullException(nameof(spriteBatch));

            if (textura != null)
                spriteBatch.Draw(textura, localizacao, Color.White);

            var destino = new Rectangle((int)localizacao.X, (int)localizacao.Y, (int)larguraDesenho, (int)alturaDesenho);
            if (MostrarDebug && texturaDebug != null)
                spriteBatch.Draw(texturaDebug, destino, Color.White * 0.6f);
        }
    }
}

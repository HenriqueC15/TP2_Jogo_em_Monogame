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

        // textura visual do mapa (tile/visual)
        public Texture2D textura;
        // textura usada apenas como máscara de colisões (alpha > 0 = sólido)
        private Texture2D texturaColisao;
        private Color[] dadosCores;

        private float larguraDesenho;
        private float alturaDesenho;

        private Texture2D texturaDebug;
        public bool MostrarDebug = false;

        // retângulos de colisão em coordenadas do MUNDO
        public List<Rectangle> CollisionRects { get; private set; }

        // checkpoint do jogador
        public Vector2 PlayerCheckpoint { get; private set; }

        // checkpoints para inimigos
        public List<Vector2> EnemyCheckpoints { get; private set; }

        public Map(Texture2D texturaVisual, Texture2D texturaColisao, float larguraDesejada, float alturaDesejada, GraphicsDevice gd)
        {
            localizacao = Vector2.Zero;
            this.textura = texturaVisual;
            this.texturaColisao = texturaColisao;
            this.larguraDesenho = larguraDesejada;
            this.alturaDesenho = alturaDesejada;

            CollisionRects = new List<Rectangle>();
            EnemyCheckpoints = new List<Vector2>();
            PlayerCheckpoint = new Vector2(450, 2800);

            // extrai pixels da máscara uma vez
            dadosCores = new Color[this.texturaColisao.Width * this.texturaColisao.Height];
            this.texturaColisao.GetData(dadosCores);

            // constrói retângulos de colisão a partir da máscara (pré-processamento)
            BuildCollisionRectangles();

            CriarTexturaDebug(gd);
        }

        private void CriarTexturaDebug(GraphicsDevice gd)
        {
            texturaDebug = new Texture2D(gd, texturaColisao.Width, texturaColisao.Height);
            Color[] pixelsDebug = new Color[dadosCores.Length];

            for (int i = 0; i < dadosCores.Length; i++)
            {
                // alpha > 0 -> sólido
                if (dadosCores[i].A > 0)
                    pixelsDebug[i] = Color.Red * 0.5f;
                else
                    pixelsDebug[i] = Color.Transparent;
            }
            texturaDebug.SetData(pixelsDebug);
        }

        // --- Checkpoints ---
        public void SetPlayerCheckpoint(Vector2 pos) => PlayerCheckpoint = pos;
        public void ResetPlayerCheckpoint() => PlayerCheckpoint = new Vector2(320, 2850);

        public void AddEnemyCheckpoint(Vector2 pos) => EnemyCheckpoints.Add(pos);
        public void ClearEnemyCheckpoints() => EnemyCheckpoints.Clear();

        // --- Construção de CollisionRects ---
        // Converte máscara (pixels) em retângulos mesclados para uso em colisões rápidas.
        private void BuildCollisionRectangles()
        {
            CollisionRects.Clear();

            int w = texturaColisao.Width;
            int h = texturaColisao.Height;

            // **Etapa 1**: gera runs (segmentos) por linha
            var runsByRow = new List<List<(int x, int width)>>(h);
            for (int y = 0; y < h; y++)
            {
                var runs = new List<(int x, int width)>();
                int x = 0;
                while (x < w)
                {
                    // encontra início do run sólido
                    while (x < w && !IsSolidPixel(x, y)) x++;
                    if (x >= w) break;
                    int start = x;
                    while (x < w && IsSolidPixel(x, y)) x++;
                    int runWidth = x - start;
                    runs.Add((start, runWidth));
                }
                runsByRow.Add(runs);
            }

            // **Etapa 2**: mescla runs verticalmente em retângulos
            var activeRects = new List<Rectangle>(); // retângulos em progresso (em coordenadas de textura)
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
                        // se o run encaixa exatamente em cima do rect ativo (mesmo X e Width)
                        if (r.X == run.x && r.Width == run.width && r.Y + r.Height == y)
                        {
                            // extende a altura do rect
                            r.Height += 1;
                            newActive.Add(r);
                            activeRects[i] = Rectangle.Empty; // marca como usado
                            extended = true;
                            break;
                        }
                    }
                    if (!extended)
                    {
                        // cria novo rect com altura 1
                        newActive.Add(new Rectangle(run.x, y, run.width, 1));
                    }
                }

                // finaliza quaisquer rects ativas que não foram estendidas nesta linha
                foreach (var r in activeRects)
                {
                    if (r != Rectangle.Empty && r.Height > 0)
                    {
                        CollisionRects.Add(r);
                    }
                }

                activeRects = newActive;
            }

            // adiciona restos
            foreach (var r in activeRects)
                if (r != Rectangle.Empty && r.Height > 0)
                    CollisionRects.Add(r);

            // **Etapa 3**: converte rects de coordenadas de textura para coordenadas do MUNDO (aplica escala e offset)
            float scaleX = larguraDesenho > 0 ? (float)larguraDesenho / w : 1f;
            float scaleY = alturaDesenho > 0 ? (float)alturaDesenho / h : 1f;

            var worldRects = new List<Rectangle>(CollisionRects.Count);
            foreach (var tr in CollisionRects)
            {
                int wx = (int)Math.Round(localizacao.X + tr.X * scaleX);
                int wy = (int)Math.Round(localizacao.Y + tr.Y * scaleY);
                int ww = Math.Max(1, (int)Math.Round(tr.Width * scaleX));
                int wh = Math.Max(1, (int)Math.Round(tr.Height * scaleY));
                worldRects.Add(new Rectangle(wx, wy, ww, wh));
            }

            CollisionRects = worldRects;
        }

        private bool IsSolidPixel(int tx, int ty)
        {
            if (tx < 0 || tx >= texturaColisao.Width || ty < 0 || ty >= texturaColisao.Height)
                return false;
            int idx = tx + ty * texturaColisao.Width;
            // alpha > 0 => sólido
            return dadosCores[idx].A > 0;
        }

        // --- Colisões em runtime: checagem por interseção com os retângulos pré-computados ---
        public bool VerificarColisaoObjeto(Rectangle rect)
        {
            if (rect.IsEmpty) return false;
            // testamos interseção com cada rect (pode ser otimizado com spatial partition se necessário)
            for (int i = 0; i < CollisionRects.Count; i++)
            {
                if (rect.Intersects(CollisionRects[i]))
                    return true;
            }
            return false;
        }

        public bool EstaColidindo(Vector2 ponto)
        {
            var p = new Point((int)ponto.X, (int)ponto.Y);
            foreach (var r in CollisionRects)
            {
                if (r.Contains(p)) return true;
            }
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
            if (texturaColisao != null)
                spriteBatch.Draw(texturaColisao, destino, Color.White * 0.0f); // não visível por padrão

            if (MostrarDebug && texturaDebug != null)
                spriteBatch.Draw(texturaDebug, destino, Color.White * 0.5f);
        }
    }
}

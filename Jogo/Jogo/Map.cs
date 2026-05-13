using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jogo
{
    internal class Map
    {
        // posição de origem do mapa
        public Vector2 localizacao;
        public Texture2D textura;

        // lista de retângulos usados para colisão no mapa
        public List<Rectangle> CollisionRects { get; private set; }

        // checkpoint do jogador
        public Vector2 PlayerCheckpoint { get; private set; }

        // checkpoints usados para respawn / spawn dos inimigos
        public List<Vector2> EnemyCheckpoints { get; private set; }

        public Map()
        {
            localizacao = new Vector2(0, 0);
            CollisionRects = new List<Rectangle>();
            EnemyCheckpoints = new List<Vector2>();
            // valor padrão do checkpoint do jogador
            PlayerCheckpoint = new Vector2(450, 2800);
        }

        // Desenha a textura do mapa (se houver)
        public void Draw(SpriteBatch spriteBatch)
        {
            if (spriteBatch == null) throw new ArgumentNullException(nameof(spriteBatch));
            if (textura != null)
            {
                spriteBatch.Draw(textura, localizacao, Color.White);
            }
        }

        // Desenha retângulos de colisão para debug usando um pixel branco (1x1) passado pelo chamador
        public void DrawCollisions(SpriteBatch spriteBatch, Texture2D pixel, Color color)
        {
            if (spriteBatch == null) throw new ArgumentNullException(nameof(spriteBatch));
            if (pixel == null) return;

            foreach (var r in CollisionRects)
            {
                spriteBatch.Draw(pixel, r, color);
            }
        }

        // --- Colisões ---
        public void AddCollision(Rectangle rect) => CollisionRects.Add(rect);
        public void RemoveCollision(Rectangle rect) => CollisionRects.Remove(rect);
        public void ClearCollisions() => CollisionRects.Clear();

        // retorna true se o retângulo informado colide com alguma área de colisão do mapa
        public bool IsColliding(Rectangle rect)
        {
            for (int i = 0; i < CollisionRects.Count; i++)
            {
                if (CollisionRects[i].Intersects(rect)) return true;
            }
            return false;
        }

        // retorna todos os retângulos de colisão que intersectam o retângulo informado
        public List<Rectangle> GetCollidingRects(Rectangle rect)
        {
            return CollisionRects.Where(r => r.Intersects(rect)).ToList();
        }

        // --- Checkpoints ---
        public void SetPlayerCheckpoint(Vector2 pos) => PlayerCheckpoint = pos;
        public void ResetPlayerCheckpoint() => PlayerCheckpoint = new Vector2(320, 2850);

        public void AddEnemyCheckpoint(Vector2 pos) => EnemyCheckpoints.Add(pos);
        public void ClearEnemyCheckpoints() => EnemyCheckpoints.Clear();
        public Vector2 GetEnemyCheckpoint(int index) => (index >= 0 && index < EnemyCheckpoints.Count) ? EnemyCheckpoints[index] : Vector2.Zero;

        public bool TryGetEnemyCheckpoint(int index, out Vector2 checkpoint)
        {
            if (index >= 0 && index < EnemyCheckpoints.Count)
            {
                checkpoint = EnemyCheckpoints[index];
                return true;
            }
            checkpoint = Vector2.Zero;
            return false;
        }

        // retorna o checkpoint de inimigo mais próximo de uma posição
        public Vector2 GetNearestEnemyCheckpoint(Vector2 position)
        {
            if (EnemyCheckpoints == null || EnemyCheckpoints.Count == 0) return Vector2.Zero;
            return EnemyCheckpoints.OrderBy(p => Vector2.DistanceSquared(p, position)).First();
        }

        // retorna um checkpoint aleatório de inimigo
        public Vector2 GetRandomEnemyCheckpoint(Random rng = null)
        {
            if (EnemyCheckpoints == null || EnemyCheckpoints.Count == 0) return Vector2.Zero;
            rng ??= new Random();
            return EnemyCheckpoints[rng.Next(EnemyCheckpoints.Count)];
        }

        // Translada a posição do mapa e, opcionalmente, ajusta retângulos de colisão e checkpoints
        public void Translate(Vector2 delta, bool moveCollisionRects = true, bool moveCheckpoints = true)
        {
            localizacao += delta;

            if (moveCollisionRects && CollisionRects != null && CollisionRects.Count > 0)
            {
                for (int i = 0; i < CollisionRects.Count; i++)
                {
                    var r = CollisionRects[i];
                    CollisionRects[i] = new Rectangle(r.X + (int)delta.X, r.Y + (int)delta.Y, r.Width, r.Height);
                }
            }

            if (moveCheckpoints)
            {
                PlayerCheckpoint += delta;
                for (int i = 0; i < EnemyCheckpoints.Count; i++)
                {
                    EnemyCheckpoints[i] += delta;
                }
            }
        }
    }
}

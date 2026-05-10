using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jogo
{
    public class Camara
    {
        public Matrix Transform { get; private set; }
        public Vector2 Position { get; private set; }
        public float Zoom = 1f;
        public float Rotation = 0f;

        private Viewport viewport;

        // Ajustáveis
        private float smoothFactor = 0.12f; // 0: travada, 1: segue instantâneo
        private Rectangle deadzone;

        // Opcional: limites do mundo para prender a câmera
        public Rectangle? WorldBounds { get; set; }

        public Camara(Viewport viewport)
        {
            this.viewport = viewport;
            // inicializa a posição no centro da viewport
            Position = new Vector2(viewport.Width / 2f, viewport.Height / 2f);

            // deadzone padrão central (pequena região onde o jogador pode se mover sem mover a câmera)
            int dzW = viewport.Width / 6;
            int dzH = viewport.Height / 6;
            deadzone = new Rectangle((viewport.Width - dzW) / 2, (viewport.Height - dzH) / 2, dzW, dzH);

            UpdateTransform();
        }

        public void Follow(Vector2 target)
        {
            // Calcula deadzone em coordenadas do mundo
            Vector2 topLeft = Position - new Vector2(viewport.Width / 2f, viewport.Height / 2f);
            Rectangle deadWorld = new Rectangle(
                (int)(topLeft.X + deadzone.X),
                (int)(topLeft.Y + deadzone.Y),
                deadzone.Width,
                deadzone.Height);

            Vector2 desired = Position;

            if (!deadWorld.Contains(target))
            {
                float dx = 0f, dy = 0f;
                if (target.X < deadWorld.Left) dx = target.X - deadWorld.Left;
                else if (target.X > deadWorld.Right) dx = target.X - deadWorld.Right;
                if (target.Y < deadWorld.Top) dy = target.Y - deadWorld.Top;
                else if (target.Y > deadWorld.Bottom) dy = target.Y - deadWorld.Bottom;

                desired += new Vector2(dx, dy);
            }

            // Suaviza a transição
            Position = Vector2.Lerp(Position, desired, smoothFactor);

            // Se houver limites do mundo, prende a câmera para não mostrar além do mapa
            if (WorldBounds.HasValue)
            {
                var bounds = WorldBounds.Value;
                float halfW = viewport.Width / 2f;
                float halfH = viewport.Height / 2f;

                float minX = bounds.Left + halfW;
                float maxX = bounds.Right - halfW;
                float minY = bounds.Top + halfH;
                float maxY = bounds.Bottom - halfH;

                Position = new Vector2(
                    MathHelper.Clamp(Position.X, minX, maxX),
                    MathHelper.Clamp(Position.Y, minY, maxY));
            }

            UpdateTransform();
        }

        private void UpdateTransform()
        {
            Transform =
                Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom, Zoom, 1f) *
                Matrix.CreateTranslation(viewport.Width / 2f, viewport.Height / 2f, 0f);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Jogo
{
    internal enum ItemType
    {
        Vela,
        Lanterna,
        Fosforos,
        Pilha,
        Chave,
        Outro
    }

    internal class Item
    {
        public ItemType Type { get; private set; }
        public Vector2 Position;
        public bool Collected { get; private set; }
        public bool IsRequired { get; set; } = false;

        private Texture2D texture; // pode ser null -> desenharemos um pixel tintado
        private const int Size = 32;
        private float bobbingPhase = 0f;

        public Item(ItemType type, Vector2 position, Texture2D texture = null)
        {
            Type = type;
            Position = position;
            this.texture = texture;
        }

        public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, Size, Size);

        public void Update(GameTime gt)
        {
            // animação simples de "bobbing"
            bobbingPhase += (float)gt.ElapsedGameTime.TotalSeconds * 3f;
            if (bobbingPhase > MathF.PI * 2f) bobbingPhase -= MathF.PI * 2f;
        }

        public void Collect()
        {
            Collected = true;
        }

        public void Draw(SpriteBatch sb, Texture2D pixel)
        {
            if (Collected) return;

            Color tint = Color.White;
            switch (Type)
            {
                case ItemType.Vela: tint = Color.Yellow*0.8f; break;
                case ItemType.Lanterna: tint = Color.Yellow*0.8f; break;
                case ItemType.Fosforos: tint = Color.Yellow*0.8f; break;
                case ItemType.Pilha: tint = Color.Yellow*0.8f; break;
                case ItemType.Chave: tint = Color.Yellow*0.8f; break;
                default: tint = Color.Gray*0.8f; break;
            }

            // pequena oscilação vertical
            int yOffset = (int)(MathF.Sin(bobbingPhase) * 4f);
            var dest = new Rectangle((int)Position.X, (int)Position.Y + yOffset, Size, Size);

            if (texture != null)
                sb.Draw(texture, dest, tint);
            else
                sb.Draw(pixel, dest, tint);
        }
    }
}

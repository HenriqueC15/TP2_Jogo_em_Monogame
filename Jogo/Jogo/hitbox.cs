using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jogo
{
    internal class hitbox
    {
        public hitbox() { }
        new Vector2 posicao;
        public Vector2 posicao2;
        new Rectangle Hitbox
        {
            get
            {
                return new Rectangle((int)posicao.X, (int)posicao.Y, 150, 150);
            }
        }

    }
}

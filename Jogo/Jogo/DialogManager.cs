using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Jogo
{
    /// <summary>
    /// Gestor de diálogos - permite fila de diálogos
    /// </summary>
    public class DialogManager
    {
        private Queue<DialogBox> dialogQueue = new Queue<DialogBox>();
        private DialogBox currentDialog = null;

        public bool HasActiveDialog => currentDialog != null && currentDialog.Ativo;

        public DialogManager()
        {
        }

        /// <summary>
        /// Adiciona um diálogo à fila
        /// </summary>
        
        

        /// <summary>
        /// Cria e adiciona um novo diálogo
        /// </summary>
        public void AddDialog(Texture2D barraTextura, SpriteSheetFont fonte, string texto, float duracao, Vector2 posicao)
        {
            var dialog = new DialogBox(barraTextura, fonte, texto, duracao, posicao);
            AddDialog_(dialog);
        }
        private void AddDialog_(DialogBox dialog)
        {
            dialogQueue.Enqueue(dialog);
        }
        public void Update(GameTime gameTime)
        {
            // Se não há diálogo ativo, tenta pegar o próximo da fila
            if (currentDialog == null || !currentDialog.Ativo)
            {
                if (dialogQueue.Count > 0)
                {
                    currentDialog = dialogQueue.Dequeue();
                }
            }

            // Atualiza o diálogo ativo
            if (currentDialog != null)
            {
                currentDialog.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (currentDialog != null && currentDialog.Ativo)
            {
                currentDialog.Draw(spriteBatch);
            }
        }

        public void Clear()
        {
            dialogQueue.Clear();
            currentDialog = null;
        }
    }
}

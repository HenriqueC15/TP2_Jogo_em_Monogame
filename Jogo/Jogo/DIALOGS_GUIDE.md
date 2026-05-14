# Sistema de Diálogos - Guia de Utilização

## Visão Geral
O novo sistema permite:
- ✅ Renderizar texto usando sprite sheets de fontes
- ✅ Gerir fila de diálogos
- ✅ Exibir diálogos em qualquer altura do jogo
- ✅ Suportar múltiplas fontes (4x4, 5x5, 8x8, 9x9)

## Classes Principais

### 1. **SpriteSheetFont**
Renderiza texto usando um sprite sheet

```csharp
// Carregar uma fonte
Texture2D fontTexture = Content.Load<Texture2D>("9x9 Font Spritesheet");
SpriteSheetFont fonte = new SpriteSheetFont(fontTexture, charWidth: 9, charHeight: 9, charsPerRow: 16);

// Desenhar texto
fonte.DrawString(spriteBatch, "Olá Mundo!", new Vector2(100, 100), Color.Black, scale: 1.0f);

// Medir tamanho do texto (útil para centralizar)
Vector2 size = fonte.MeasureString("Olá Mundo!", scale: 1.0f);
```

### 2. **DialogManager**
Gestor de diálogos com fila

```csharp
// Já está inicializado em Game1
// dialogManager é membro privado de Game1

// Adicionar um diálogo
dialogManager.AddDialog(barraTextura, fonte, "Meu primeiro diálogo!", 3f, new Vector2(100, 200));

// Verificar se há diálogo ativo
if (dialogManager.HasActiveDialog)
{
	// Ainda há diálogos a exibir
}
```

### 3. **DialogBox**
Caixa de diálogo individual

## Exemplos de Uso em Game1.cs

### Exemplo 1: Diálogo ao recolher um item
```csharp
// No método Update(), na lógica de coleta de itens:
case ItemType.Vela:
	SetVela(true);
	Texture2D barra = Content.Load<Texture2D>("barra_de_texto");
	dialogManager.AddDialog(barra, fonte, "Consegui a vela!", 3f, new Vector2(240, 550));
	break;
```

### Exemplo 2: Diálogo ao iniciar combate
```csharp
// Quando o boss aparece:
if (pilha == 2 && !Bossapareceu)
{
	Bossapareceu = true;
	Texture2D barra = Content.Load<Texture2D>("barra_de_texto");
	dialogManager.AddDialog(barra, fonte, "Um inimigo poderoso aparece!", 4f, new Vector2(240, 550));
}
```

### Exemplo 3: Sequência de diálogos
```csharp
// Adicionar múltiplos diálogos (aparecem em sequência)
Texture2D barra = Content.Load<Texture2D>("barra_de_texto");
dialogManager.AddDialog(barra, fonte, "Primeiro diálogo", 2f, new Vector2(240, 550));
dialogManager.AddDialog(barra, fonte, "Segundo diálogo", 2f, new Vector2(240, 550));
dialogManager.AddDialog(barra, fonte, "Terceiro diálogo", 2f, new Vector2(240, 550));
```

## Dimensões das Fontes

- **4x4 Font**: charWidth=4, charHeight=4, charsPerRow=16
- **5x5 Font**: charWidth=5, charHeight=5, charsPerRow=16
- **8x8 Wide Font**: charWidth=8, charHeight=8, charsPerRow=16
- **9x9 Font**: charWidth=9, charHeight=9, charsPerRow=16

## Usando Diferentes Fontes

```csharp
// Carregar múltiplas fontes em LoadContent:
Texture2D font9x9 = Content.Load<Texture2D>("9x9 Font Spritesheet");
Texture2D font8x8 = Content.Load<Texture2D>("8x8 Wide Spritesheet");

SpriteSheetFont fonte9x9 = new SpriteSheetFont(font9x9, 9, 9, 16);
SpriteSheetFont fonte8x8 = new SpriteSheetFont(font8x8, 8, 8, 16);

// Usar em diálogos
dialogManager.AddDialog(barra, fonte9x9, "Texto grande", 3f, pos);
dialogManager.AddDialog(barra, fonte8x8, "Texto pequeno", 3f, pos);
```

## Posições Comuns de Diálogo

```csharp
// Canto inferior esquerdo (atual)
new Vector2(240, 550)

// Centro inferior
new Vector2(400, 550)

// Canto superior esquerdo
new Vector2(100, 50)

// Centro da tela
new Vector2(400, 300)

// Seguindo o jogador (relativo à câmera)
player.posicao + new Vector2(-100, -150)
```

## Recursos da Câmera
Se quiser diálogos que se movem com a câmera, use:
```csharp
spriteBatch.Begin(transformMatrix: camera.Transform);
// desenha aqui
spriteBatch.End();
```

Se quiser diálogos fixos na tela, desenhe na batch final (sem transformação de câmera).

## Suporte para Quebras de Linha
```csharp
// Texto com quebra de linha
string texto = "Primeira linha\nSegunda linha";
fonte.DrawString(spriteBatch, texto, new Vector2(100, 100), Color.Black);
```

## Dicas

1. **Cache de fontes**: Carregue as fontes uma única vez em LoadContent e reutilize
2. **Durações variáveis**: Diálogos mais longos merecem mais tempo na tela
3. **Posições estratégicas**: Coloque diálogos onde não tapem pontos importantes do jogo
4. **Escalas diferentes**: Use o parâmetro scale para ajustar tamanho do texto
		
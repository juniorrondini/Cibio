# Tela inicial e Palma da Centelha — versão 13

Quatro imagens originais criadas com **image_gen integrado**, modo padrão. Os prompts finais completos estão em [assets13.json](assets13.json); os originais acompanham esta pasta.

- `../../Resources/RpgArt/Title_Vila13.png`: key art da vila atual, usando capturas da praça/Largo e Hero_idle como referências de cenário e personagem. Mesmo brasileiro de camiseta amarela, capela pequena, feira, calçamento e ipês; área esquerda reservada ao menu. O movimento suave do fundo continua no RpgInterface.
- `../../Resources/RpgArt/UI_Palma13.png`: ícone exclusivo da tecla3, mão aberta com energia,256px com margem transparente; não substitui os outros ícones de UI_Skills09.
- `../../Resources/RpgArt/Vfx_PalmaFlight13.png`:16quadros em4×4, células256px. Cabeça luminosa voltada ao norte, cauda para baixo; o jogo gira o sprite segundo a direção do disparo.
- `../../Resources/RpgArt/Vfx_PalmaImpact13.png`:16quadros em4×4 de ignição, expansão orgânica e dissipação; duração0.5s com saída suave. Sem anéis/polígonos de contorno.

Importação: execute `python import13.py` nesta pasta, com Pillow e numpy. Apenas redimensionamento, recorte e margem/registro dos originais gerados; nenhum desenho é criado por código. Os atlas finais têm1024×1024 e16px ou mais de margem transparente por célula. O ícone tem256×256. Transparência real preservada; RGB colorido sob alpha0 não é fundo opaco.

Arte derivada do projeto Cibio: CC BY-SA4.0, mantendo as atribuições herdadas nos arquivos de créditos existentes. Referências pertencem ao próprio projeto; nenhuma imagem ou som foi extraído de franquias. Fontes-Visual-13.zip acompanha a distribuição.

Gameplay preservado: Palma22energia, recarga1.8s ou1.1s no estágio2, velocidade8/11 e dano1.35/1.8 multiplicado pelo dano da forma. A diferença de porte é visual. Colisão varrida e impacto único continuam; o efeito agora também aparece ao atingir parede. Projéteis de inimigos mantêm o asset anterior.

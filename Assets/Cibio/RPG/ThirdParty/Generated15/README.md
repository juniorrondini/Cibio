# A décima volta — arte original

Onze imagens originais criadas com **image_gen integrado**, usando como referências de identidade/estilo os próprios sprites de Cibio. As cinco folhas do chefe têm 128 quadros ao todo, em quatro direções: idle 4, walk 8, slash 6, spellcast 8, hurt 6. A fusão tem manopla de vuvuzela, faixa verde/amarela, estrela 13, cabelo castanho/grisalho e barba branca.

Também incluem arena na boca da vuvuzela, mundo-instrumento, oito etapas ilustradas da transformação do mundo, 16 quadros da ruptura orbital, 16 ondas de voz e nove ícones de talentos.

Sprites finais em `../../Resources/RpgArt`. Atlas do chefe: células 256, PPU 64, pivô (0,5; 0,1015625), pés registrados em y=230. VFX 4×4, células 256 com margem 16. Ícones 3×3, células 256 com margem 12. WorldFold15 usa 2×4 painéis. Prompts completos e referências em [assets15.json](assets15.json); originais preservados nesta pasta. [Import-Audit15.json](Import-Audit15.json) registra dimensões.

Para reproduzir: `python import15.py`, com Pillow, numpy e scipy. No projeto, atualiza Resources/RpgArt; extraído sozinho, grava na subpasta Final das próprias fontes. O importador apenas extrai células, dimensiona e registra as imagens geradas; não sintetiza desenhos substitutos. Alpha dos sprites/VFX preservado. Não exige API para importar as fontes já incluídas.

Derivados do projeto Cibio: CC BY-SA 4.0, mantendo atribuições anteriores. Nenhum desenho ou áudio foi extraído de franquias. Fontes-Convergencia-15.zip acompanha a distribuição, junto das fontes DSP dos dez efeitos sonoros originais.

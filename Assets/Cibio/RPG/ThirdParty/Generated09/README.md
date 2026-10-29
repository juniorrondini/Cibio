# Arte original — versão 09

Criada com image_gen integrado; referências dos personagens existentes de Cibio. Nenhum asset extraído de franquia. Originais e prompts integrais em assets09.json.

A importação apenas segmenta o alpha, recorta, registra os pés e redimensiona as imagens geradas. As fontes variam em escala: cada quadro é normalizado para uma altura de 104px, com margens transparentes na célula 192px/PPU64. A transição cresce de 90 para 104px. Esse registro evita mudanças bruscas de tamanho entre ações. A evolução tem oito quadros por direção; a perspectiva norte recebeu uma geração complementar.

Personagem e derivados: CC BY-SA 4.0; preservar créditos LPC e Cibio. Ícones e vapor: arte original do projeto, CC BY-SA 4.0.

Saídas: Resources/RpgArt/HeroAscendant_*.png, Transform_Ascendant09.png, UI_Skills09.png e Vfx_Sonic09.png. Para regenerar a importação: Python com Pillow, numpy e scipy; executar pack09.py nesta pasta.

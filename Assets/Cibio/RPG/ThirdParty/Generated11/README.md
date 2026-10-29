# Arte original — versão11

Criada com a ferramenta integrada image_gen: Seu Davi (32 quadros), quatro construções, três árvores, arbusto/pedras/juncos, lago, quatro materiais de chão e seis objetos urbanos. Manifesto e prompts finais integrais em assets11.json. O primeiro atlas de natureza e sua revisão de transparência foram preservados.

A referência enviada pelo usuário orientou traços e vestuário do NPC. A imagem de referência não foi incorporada ou redistribuída no projeto. Os sprites novos foram desenhados pela geração e não extraídos da imagem de referência.

Derivados visuais de Cibio: CC BY-SA4.0. Preservar as atribuições LPC herdadas em CharacterCredits.csv/Colonial/Roofs; referências de personagens e edifícios anteriores não foram removidas. Sem assets extraídos de franquias.

O importador apenas segmenta alpha, recorta, registra os pés e dimensiona o material gerado. Seu Davi: células128, PPU64, pivot(.5,.16), linhas norte/oeste/sul/leste; colunas0–3 idle,4–7 conversa. Objetos têm PPU próprio em RpgArt.ImagePPU. Texturas de terreno são recortes dos quadrantes do atlas. RGB colorido sob alpha0 não é fundo opaco; verificar a composição por alpha antes de interpretar prévias.

Reprodução: Python com Pillow, numpy e scipy; execute import11.py nesta pasta. O jogo e o editor não precisam de Python ou de chave de API para usar os assets incluídos.

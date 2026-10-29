# Companheiro — arte da versão 12

Sprites originais criados com **image_gen integrado**, modo padrão, a partir da referência visual enviada pelo usuário e do estilo de Davi11. A imagem de referência do usuário não é distribuída. O personagem tem terno escuro, gravata vinho, barba/cabelo brancos e estrela vermelha com o número branco **13** no peito.

Os cinco originais e os **prompts finais integrais** estão em [assets12.json](assets12.json). Os sprites finais ficam em ../../Resources/RpgArt/Companheiro12_*.png. São 128 quadros, quatro direções na ordem norte/oeste/sul/leste, células128px, PPU64, pivot(.5,.16). Idle4, walk9 (neutro + oito passos), slash6, spellcast7, hurt6. Os desenhos foram gerados; o importador apenas segmenta alpha, recorta, registra e dimensiona. A escala é fixa por direção/ação para conservar agachamento e queda, sem esticar cada quadro.

Reprodução: Python com Pillow, numpy e scipy; execute `python import12.py` nesta pasta. O importador usa exclusivamente os originais incluídos e gera o relatório Import-Audit12.json. O editor e o jogo não precisam de Python ou API para consumir os sprites.

Derivados visuais do projeto Cibio: CC BY-SA4.0. Preservar atribuições herdadas dos assets de referência do projeto, incluindo os créditos LPC anteriores. Nenhum sprite ou som foi extraído de franquias.

Seu Bonero mantém a arte e as animações da versão11; Davi11/RpgNpcDavi11 são identificadores internos preservados para não quebrar referências Unity. O campo captainDefeated continua no savev3 e agora representa a derrota do Companheiro.

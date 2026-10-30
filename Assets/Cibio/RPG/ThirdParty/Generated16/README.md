# Bonero Companheiro — divisão literal

Cinco atlas originais criados com **image_gen integrado** e referências do próprio Cibio: Seu Bonero, Companheiro e a fusão anterior. Rosto e traje divididos verticalmente: Bonero castanho/sem barba/faixa/gravata azul, Companheiro branco/com meia barba/estrela13/gravata vinho. Os prompts incluem a anatomia e a inversão da vista traseira.

128 quadros: idle4, walk8, slash6, spellcast8, hurt6 em quatro direções (cima, esquerda, baixo, direita). Células256, PPU64, pivô(.5,.1015625), pés em y230. O importador extrai silhuetas inteiras por alpha, remove componentes alheios, registra os pés e insere margens; não redesenha ou sintetiza partes. Originais e variantes anteriores preservados, prompts completos em assets16.json. Derivado Cibio CC BY-SA4.0, mantendo atribuições anteriores.

Reproduzir: `python import16.py` com Pillow/numpy/scipy. No projeto grava Resources/RpgArt; extraído sozinho usa Final/. Fontes-Fusao-16.zip acompanha a distribuição.

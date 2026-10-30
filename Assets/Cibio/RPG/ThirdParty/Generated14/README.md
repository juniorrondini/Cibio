# Costa da Brisa — assets14

Doze assets originais criados com **image_gen integrado**, modo padrão: materiais de areia/mar/madeira/rocha, farol, casa de pescador, barco, palmeira, pedras, trapiche e baú fechado/aberto. Referências são os assets11 e uma captura do próprio Cibio. Prompts finais completos em [assets14.json](assets14.json); três originais incluídos.

Texturas finais em ../../Resources/RpgArt: Terrain_*14.png (512²); Lighthouse14, Cabin14, Boat14, Palm14, CoastRock14, Pier14 e Cache14_closed/open. Registro e tamanhos em Import-Audit14.json; PPU individual em RpgArt.ImagePPU. Alpha genuíno, sem fundo opaco. O importador apenas segmenta alpha, recorta e dimensiona os desenhos gerados.

Reproduzir: `python import14.py` nesta pasta, com Pillow, numpy e scipy. Não exige API para consumir ou reimportar os arquivos incluídos. A composição da costa e o movimento do mar são materiais Unity usando essas texturas; o minimapa é um mapa de dados da geometria.

Derivados visuais de Cibio: CC BY-SA4.0, preservando atribuições herdadas e licenças anteriores. Nenhum asset extraído de franquia. Fontes-Costa-14.zip acompanha o player.

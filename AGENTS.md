# Projeto Cibio

- Projeto principal: `D:\Cibio`, Unity `6000.6.7f1`.
- Direção atual escolhida pelo usuário: RPG em pixel art detalhada em perspectiva, como Tibia/Poketibia. Movimento no plano XY, com sobreposição por altura do pé e câmera ortográfica. O salto é deslocamento visual; colisões continuam no chão.
- Cena principal: `Assets/Cibio/RPG/Scenes/03_VilaDaCentelha.unity`. Menu `Cibio > RPG > Abrir Vila da Centelha`.
- A cena já contém a vila editável. Para mudanças futuras, prefira editar e salvar a cena existente. `RpgBuilder.Build` recria o mapa e substitui a composição da cena.
- Código e arte da versão atual: `Assets/Cibio/RPG`. Preserve as versões antigas em `City`, `Scripts`, `Scenes`, `Prefabs` e `Art`.
- Preserve `.meta` e GUIDs. Não edite Library ou Temp como fontes.
- `RpgActor`, `RpgWorld`, `RpgGame`, `RpgAura`, `RpgEffects` e `RpgInterface` compõem o protótipo. `RpgSolid` define colisões em coordenadas do chão e acompanha o objeto movido.
- Sprites LPC: 64 px por quadro; direções cima, esquerda, baixo e direita. Idle 2, walk 9, run 8, jump 5, slash 6, spellcast 7. Hurt tem 6 quadros em uma direção. Não reduzir texturas a potências de dois; usar NPOT None, filtro Point e sem compressão/mipmaps.
- Antes de incorporar arte, conferir licenças e manter créditos por arquivo e originais em ThirdParty. Arte derivada LPC da versão 03 é CC BY-SA 4.0. Não usar arte extraída de franquias.
- O menu Créditos precisa manter a atribuição acessível. A distribuição inclui StreamingAssets/Cibio-Licenses.
- Verificação: gerar player com `Cibio.RPG.Editor.RpgBuilder.BuildPlayer`, argumento `-cibio-build` para escolher o EXE; depois executar player com `-cibio-review <pasta>` para testes e capturas. Esse modo não altera o save do jogador e fecha ao terminar. Capturas de janela oculta podem ficar pretas neste computador.
- A chave de progresso é `Cibio.RPG.v3.Progress`. Nunca apagar saves do usuário durante verificações.
- Interface, mensagens e documentação para o jogador em português.

## Atualização 04

- A transformação base está disponível desde o início: segurar C até 100% e soltar ativa 16s. O altar Coração Canarinho melhora para 24s. Não voltar a bloquear a transformação atrás do altar.
- Hero, HeroLift e HeroGold possuem silhuetas de cabelo distintas. HeroLift usa cabelo erguido durante a carga, HeroGold usa cabelo espetado dourado. Manter as perspectivas e ações completas.
- Aura, impactos, altares e poderes usam Vfx_AuraFlow e Vfx_BurstFlow com OrganicEnergy.shader. Não usar LineRenderer, anéis de contorno ou partículas quadradas para os efeitos de energia. Renderers novos devem ficar invisíveis até receber a textura e as propriedades do efeito.
- Combo: slash (6 quadros), thrust (8), backslash (13). Família de sprites inclui EnemyHeavy e EnemyCaster; inimigos possuem arquétipos corpo a corpo, blindado e conjurador à distância.
- RpgAudio usa sons CC0 com fontes em ThirdParty/Audio-CC0.txt. M alterna o som; o modo de revisão grava o áudio do próprio jogo.
- Para compilar preservando a cena, usar Cibio.RPG.Editor.RpgBuilder.BuildUpdatedPlayer e -cibio-build. Build/BuildPlayer antigos continuam recriando a vila e não devem ser usados para atualizações normais.
- Atualização 04 mantém a chave de save v3 para preservar o progresso existente.


## Atualização 05

- Forma desperta usa HeroAwakened com arte própria. Células de 128px, PPU64; demais famílias continuam em 64px, PPU32. Use RpgArt.CellSize/FrameCount em vez de assumir 64 para todos.
- Transform_Sequence05 tem seis etapas por quatro direções (768x512). RpgPowerForm mistura os quadros e mantém o registro no chão. A forma desperta possui nove atlas completos, incluindo os três golpes.
- Transformação dura 1,15s, dano desperto 2,3x e velocidade 1,3x. A liberação de C completo continua ativando a forma sem exigir altar.
- RpgTerrain/OrganicGround compõem terreno orgânico usando Terrain_Grass05, Terrain_Stone05 e Terrain_Mask05. Preservar a vila salva. RpgTerrainPreview oferece prévia transitória no editor; o arquivo da cena não foi substituído na atualização.
- Sons de Designed05 usam fontes CC0 e design próprio. Receitas e gravações em ThirdParty/SoundDesign05; créditos Audio-05.txt. A captura nativa verifica a mixagem; M mantém o mute.
- Originais gerados e receita de importação em ThirdParty/Generated05; prompts finais em ThirdParty/Prompts-05.txt. Não redesenhar sprites por aproximação procedural.

## Atualização 06

- Aura combina Vfx_AuraBody06, Vfx_AuraVeil06, Vfx_Wisp06, Vfx_Arc06 e Vfx_Surge06. Cada atlas tem 4x4 quadros, 1024px, alpha genuíno e margens transparentes. Originais, prompts e importador em ThirdParty/GeneratedVfx06 e Prompts-VFX-06.txt.
- RpgAura compõe cinco camadas, emissão dianteira/traseira, crescimento e movimento. RpgEnergyParticles usa ParticleSystem nativo e um emissor compartilhado para impactos. RpgParticleClock acompanha pausa. Não voltar a partículas quadradas ou LineRenderer.
- Pacote integrado necessário: com.unity.modules.particlesystem 1.0.0. As curvas x/y/z de VelocityOverLifetime devem usar o mesmo modo; z também usa TwoConstants, com zero nos dois extremos.
- Camada 29 reservada à energia; RpgEnergyBloom renderiza essa camada em baixa resolução e compõe apenas seu brilho no renderizador padrão. Não colocar cenário ou interface nessa camada.
- RpgEffects cria renderers desabilitados até receber propriedades e textura. EnergyVolume06 usa alpha pré-multiplicado; preservar a leitura do rosto na camada dianteira.
- -cibio-vfx-review gera capturas a 24fps fixos e mede 120 quadros sem captura em tempo real; -cibio-review mantém a revisão geral. Ambos preservam o save. Não usar o passo fixo de captura no gameplay normal.
- Atualização mantém cena salva, famílias de personagens e progresso v3. Continuar compilando por BuildUpdatedPlayer.

## Atualizacao 07
- Tecla 1 agora alterna voo continuo, sem teleporte. RpgActor.flying/flightHeight; 12 energia inicial, 10/s normal ou 7/s desperto, pouso por 1/Espaco ou energia zero; conserva colisoes do mapa.
- Hero_flight (64px) e HeroAwakened_flight (128px): 4 quadros, 4 direcoes. Originais e prompt em ThirdParty/Generated07.
- Walk recebe run explicito e usa fase por distancia; ataques travam direcao e tem quadro de contato por familia/golpe. Buffer de input cobre hit-stop.
- Onda tem antecipacao e alcance 3.8 normal/5.4 desperto. Testes visuais verificam dano apos release e knockback a 4.6.
- Designed07 contem sons CC0/DSP; RpgAudioLimiter no listener limita picos sem hard clipping.
- -cibio-review07 testa voo, combos e gait, grava audio em tempo real separado da captura visual fixa 24fps; salva relatorios. Nunca alterar save durante review.

## Atualização 08
- Cena ampliada de 48x36 para 48x60; Morro do Eco ao norte, NPC Seu Davi (-2,-6.3), 3 amplificadores e 7 inimigos novos. Mantém os 7 anteriores. Bounds é serializado. Duas árvores na entrada norte foram desativadas.
- RpgExpansion08.Append acrescenta a expansão à cena salva e é idempotente. RpgExpansion08.BuildPlayer chama Append + BuildUpdatedPlayer; não usar o gerador antigo Build.
- RpgQuest08 controla questStage 0/1/2/3, silenced bitmask e captainDefeated. Campos adicionais conservam versão/chave de save v3. Recompensa única: 100XP, voo/Coração se faltarem, estágio 2 e Palma.
- formStage 2 (FormStage consulta buff ativo) exige missão 3 e uma segunda carga C completa enquanto dourado; 25 energia, 12s, dano3.15x; aura vermelha com atlas e emissores existentes. Escala/acento interpolados e poses douradas preservadas durante transformação2.
- Voo normal/dourado/rubro 6/8/12; Shift multiplica1.25 e custa+6/s; Ctrl tem prioridade, freia até hover. Dreno10/7/9 por segundo. Pouso1/Espaço, C força pouso antes de carregar.
- [3] Palma:22energia, cooldown1.8/1.1s rubro; projétil com teste varrido por segmento e apenas um impacto. Boss enemyKind4 HP280 não renasce; batedor enemyKind3 HP78, aviso .6s e bote contínuo .24s; boss aviso .85s, leque3projéteis de16, cooldown2.8s.
- -cibio-review08 mantém base, valida missão e save antigo, voo e dano comparativo; exporta inspeção dos ciclos nas4direções e captura cinematográfica24fps. Não alterar o save real em testes.

## Atualização 09
- Estágio 2 substitui Rubra pela Forma Ascendente: novo toque de C enquanto dourado, sem nova carga nem missão; custo25, duração16s, sequência1.7s. HandleTransformInput é compartilhado pelo teclado e botãoC; não reativar enquanto a tecla permanece pressionada. TryAscend conserva voo, mas bloqueia movimento na sequência.
- HeroAscendant possui dez ciclos próprios: células192, PPU64, pivot(.5,.10666667), quad3. HeroAwakened128 e LPC64 permanecem. RpgPowerForm deve usar tamanho do sprite/PPU na malha, sem supor quad2. Transform_Ascendant09 tem8colunas x4direções; shader consulta _TransformColumns.
- Sonic em estágio2+voo+Shift exige18unidades de deslocamento real contínuo. Cruzeiro12, impulso15, sonic24; dreno9/15/18porsegundo. Freio, soltura, pouso, colisão bloqueada, inversão forte, morte/expiração reiniciam distância. Nunca acumular pela velocidade desejada contra parede.
- RpgFlightWake09 usa atlas orgânico Vfx_Sonic09 e fantasmas do sprite; sem LineRenderer ou contornos geométricos. Designed09 contém efeitos originais DSP. Fonte CibioUI derivada de Inter OFL, originais e licença preservados.
- RpgInterface usa cinco skills, tooltips, minimapa e objetivos recolhíveis. Textos em UTF-8; conferir acentos no player. Não voltar ao HUD de painéis grandes nem bloquear controles fora dos limites visuais dos botões.
- -cibio-review09 testa input real compartilhado, sonic, família nova, fontes, quadros nas4direções e grava áudio em tempo real separado. Preservar savev3 e cena/GUIDs; continuar BuildUpdatedPlayer.

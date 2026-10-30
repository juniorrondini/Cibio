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
- Verificação: gerar player com `Cibio.RPG.Editor.RpgBuilder.BuildUpdatedPlayer`, argumento `-cibio-build` para escolher o EXE; depois executar player com `-cibio-chronicle-review15 <pasta>` para testes e capturas. Esse modo não altera o save do jogador e fecha ao terminar. Capturas de janela oculta podem ficar pretas neste computador.
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

## Atualização10
- Supersônico: teto60u/s, arrancada imediata42u/s na ruptura, distância18 e dreno18/s preservados. SonicTopSpeed/SonicLaunchSpeed centralizam valores. Não usar teleporte.
- Boom em Designed10: DSP CC0, duplo transiente42ms, subgrave58→33Hz, clip2s. RpgAudio dá prioridade16 e mix2D com duck320ms; nunca alterar volume do sistema ou remover limiter. Vento supersônico separado.
- Câmera sonic9.4, follow.065s, previsão3.5; rastro.035s e nuvem7.4. Ctrl e colisões varridas preservados.
- -cibio-sonic-review10 executa base, evolução e teste de arrancada, velocidade, freio, colisões e captura sonora em tempo real. Preservar cena/GUIDs e savev3.

## Atualização 11
- Cena64x72, bounds(-32,-18,64,72), Largo das Palmeiras ao norte e ruas laterais. RpgExpansion11.Append/Details preservam IDs e são idempotentes; BuildPlayer executa ambos e BuildUpdatedPlayer. Para manutenção normal continuar BuildUpdatedPlayer. Não usar o gerador base.
- Seu Davi mantém o objeto/ancora(-2,-6.3). RpgNpcDavi11 usa Davi11:8colunas×4direções, células128/PPU64/pivot(.5,.16); col0–3 idle,4–7 conversa. Ele vira para o herói e fala por4.5s após interação. Não voltar a usar Hero_idle no NPC. Colisão elíptica pequena própria e painel de diálogo no RpgInterface.
- House11/Tree11/natureza/props têm PPU específico viaRpgArt.ImagePPU. Novas imagens22 e originais/prompts/importador em Generated11; CC BY-SA4.0, preservar créditos anteriores. A referência do usuário não está incluída nas fontes.
- RpgSolid.elliptical expande a elipse pelo raio do jogador; NPC, lagos, árvores e pequenos props usam. Colisões retangulares antigas e obstáculos continuam compatíveis.
- OrganicGround usa quatro materiais11 e máscara da vila original, caminhos com bordas suaves e repetição suavizada da grama no shader. SoftShadow11 é um shader de sombra de contato, sem bitmap substituto da arte.
- Preservar componentes/objetos anteriores: árvores que fechavam passagens foram desativadas; uma casa de(0,7) foi movida a(-5,10) para liberar o eixo central. Nenhum objeto original foi apagado. A capela fica em(0,48), sem corte no limite norte.
- -cibio-village-review11 valida missão/NPC/rotas/colisões,32quadros, mapa e amostra de desempenho. Savev3 intacto. O supersônico10 de60u/s continua disponível.

## Atualização 12
- Chefe enemyKind4 agora é Companheiro, mesma instância/280HP/26soco/leque3x16. Companheiro12 tem128quadros, células128/PPU64/pivot(.5,.16): idle4, walk9, slash6, spellcast7, hurt6 em quatro direções. Não reutilizar EnemyHeavy para o chefe; guardas blindados continuam EnemyHeavy.
- RpgBoss12.UpdateScene atualiza NPC/chefe existentes e é idempotente; BuildPlayer chama esse migrador e BuildUpdatedPlayer. Não recriar a vila. IDs/GUIDs anteriores preservados. A âncora do chefe foi ajustada de(0,38.5) para(2.6,39) para o amplificador não cobrir o corpo.
- Seu Bonero é o nome público do antigo Seu Davi. RpgNpcDavi11, Davi11 e campo captainDefeated são identificadores internos mantidos por compatibilidade; não renomear GUIDs nem migrar/destruir o savev3. Busca do NPC aceita o nome anterior como fallback.
- Chefe prefere soco abaixo de1.2u; poder só acima de1.8u, antecipação850ms em quadros0–4, liberação5 e recuperação6 durante240ms. Recarga2.8s preservada. Soco dura.6s e contato.55/quadro3. Barra de vida própria aparece até8u.
- Originais, prompts finais e importador em Generated12; fonte Fontes-Chefe-12.zip acompanha player. Usar image_gen integrado para nova arte; não desenhar sprites via código. Referência do usuário não distribuída.
- -cibio-boss-review12 valida base/evolução/vila/chefe,128quadros nativos, combate e conclusão da missão, sem alterar save real.

## Atualização 13
- Menu usa Title_Vila13 (vila e personagem atuais); zoom/deslocamento suaves preservados. UI_Palma13 é o ícone exclusivo da tecla3/index5,256px; os outros ícones continuam no atlas09.
- Vfx_PalmaFlight13 e Vfx_PalmaImpact13 usam4×4/16quadros de256px com margem>=16px,1024². RpgEffects.Paint reconhece Vfx_Palma como grade4×4. Não voltar a Vfx_Wisp06 no projétil amigável.
- RpgProjectile mantém custo22, recarga1.8/1.1, velocidade8/11, dano1.35/1.8 e colisão varrida/impacto único. O porte visual cresce por forma; cabeça do sprite registrada junto ao ponto de colisão mais elevação visual. Inimigos conservam Vfx_Wisp06.
- RpgPalmaImpact13 dura.5s e respeita pausa, usa bloom/camada29, atlas orgânico e saída suave. Impacto ocorre no alvo ou lado livre da parede; sem geometria de contorno e sem quadrado opaco antes de texturizar.
- Originais, prompts finais e importador em Generated13; Fontes-Visual-13.zip distribuído com créditos. Imagens criadas com image_gen integrado; importador só recorta/dimensiona/insere margens.
- -cibio-art-review13 verifica base/evolução/vila/chefe e Palma; captura menu/32quadros/12vistas. Preservar savev3, GUIDs e cena byte a byte. BuildUpdatedPlayer para compilar, sem migrar/recriar a cena.

## Atualização14
- Bounds(-48,-28,96,112), costa compartilhada RpgIsland14/OrganicGround, mar bloqueado e trapiche(35,8.5,11,3) acessível. Novos caminhos contornam a capela. RpgExpansion14.Append é idempotente, preserva objetos/GUIDs e acrescenta8 inimigos(total22),3 caches e paisagem. BuildPlayer chama Append+BuildUpdatedPlayer; nunca usar gerador base.
- Savev3 conserva tudo e acrescenta islandCaches(default0), máscara de3 baús com45XP e20vida/energia uma única vez. ID2 fica no trapiche; caches interagem comE no chão, sem inimigo a3.3u. Posições(-33,64),(30,70),(44.3,10).
- RequestAttack compartilha entradaJ/clique/UI, buffer.18s e fila de um golpe após25% da animação; Attack continua a API antiga. Recuperação aceita movimento após88%, esquiva após82%. Acertos usam arco frontal e HasClearPath, sem dano por parede. Stagger.16/.3 leve/finalizador; blindado fator.55, chefe só finalizador fator.25, sem cancelar rajada do chefe.
- Esquiva Alt/botão direito:10energia,.85cooldown,.2duracao,.13iframes;9/11.5/13.5u/s por forma. Usa Run/afterimage existentes e colisão Move com passos.12. Não permitir durante carga, voo, salto, transformação ou início de ataque. Estado limpo em morte/reset; fila limpa em pausa.
- Assets originais/prompts/importador em Generated14 e Fontes-Costa-14.zip, ferramenta image_gen integrada. PPU específico em RpgArt.ImagePPU. Sem novas imagens procedurais como substituto dos assets.
- -cibio-island-review14 executa regressões e ensaios de mapa/combate, grava Balance14.csv para alvos estacionários, capturas e FPS breve. Preservar save real. Os testes devem considerar22 inimigos e o novo bounds; originais continuam na lista.

## Atualização 15
- RpgTalents15 usa talentRanks (2 bits por talento, nove talentos) e pontos derivados de level-1+bonusTalentPoints. Não usar o bitmask perks dos altares para talentos. Sanitizar graus, dependências e orçamento ao carregar. Save v3 preservado. XP centralizado em GainExperience; Contratempo e ecos limpos entre sessões/morte. T usa mode4 e pausa as atualizações de jogo.
- RpgWorldSecret15 é runtime, sem reconstruir ou salvar a cena. FormStage2+voo+Shift atravessa tudo e faz wrap. TravelSonic recebe deslocamento cru, nunca distância do teleporte de borda. Voltadas pelo eixo inicial; recuo/bounce não contam. Freeze de buff/energia só durante órbita ativa. Ctrl/soltar/pousar restaura posição standable.
- Dez voltas iniciam revelação7.2s com WorldFold15 (2x4) e World_Vuvuzela15; arena mantém originais inativos e troca temporariamente a lista de inimigos. Restaurar bounds/lista/cenário ao sair/newgame. Save usa EntryPosition; descoberta pendente retoma arena. Nunca salvar coordenadas da arena sobre o retorno.
- Bonero Companheiro é enemyKind5, Family BoneroCompanheiro15, células256/PPU64/pivô(.5,.1015625). Controlador RpgFusionBoss15 separado da IA antiga; quatro movimentos e fase aos60%PV. Ondas nativas RpgHornShot15 com colisão varrida; voar não evita voz. Não reaproveitar Companheiro12 para a fusão. Recompensa350XP+1bonusTalentPoint única. Não alterar captainDefeated/questStage do chefe original.
- UI parcial RpgInterface15, arte11imagens em Generated15 e sons10 em SoundDesign15; originais/prompts/receitas/créditos incluídos. Imagegen integrado, sem imagens procedurais substitutas. BuildUpdatedPlayer compila sem modificar a cena. -cibio-chronicle-review15 valida regressões e novos sistemas, captura quadros por Camera.Render quando a janela não renderiza, preserva o save real.

## Atualização 16
- A órbita integra deslocamento cru assinado normalizado pelo bounds em ambos os eixos; não prender contagem ao heading inicial ou ao evento de wrap. LapProgress permite ver a fração real. Deslocamentos opostos curtos se anulam. FlightTier guarda máximo de voltas+fração para não perder velocidade ao corrigir direção; Speed=60*1.2^tier, máximo tier10.
- RpgGame.DriveHero/TickWorld são o caminho compartilhado por input e revisão. TickWorld chama Secret.Advance ao fim do movimento; Update avança apenas a revelação quando já está cinematográfica. Não restaurar o Update independente em RpgWorldSecret15 (avançaria duas vezes). Aceitar LeftShift e RightShift.
- RpgTemporalFlight16 na câmera principal, depois de RpgEnergyBloom, usa TemporalFlight16.shader e histórico de meia resolução. Centro preservado; limpar histórico por wrap, pausa, cancelamento e destruição. UI IMGUI desenhada depois. Sem timeScale global e sem tocar na velocidade da IA para simular efeito.
- Família BoneroCompanheiro16 possui 5 atlas/128 quadros, 256px, PPU64, pivô(.5,.1015625). Preservar divisão anatômica literal dos personagens. Fonte image_gen integrado, prompts/originais/importador em Generated16. Recursos antigos preservados para histórico, controlador usa família16.
- -cibio-orbit-review16 roda regressões anteriores e ensaios de Shift parado, mudança perpendicular, diagonais, 24/60/144Hz, velocidade por volta, revelação pelo TickWorld, shader e save. Camera.Render para evidência nativa; não tratar captura vazia da janela como revisão de HUD. BuildUpdatedPlayer sem recriar cena.

## Atualização17
- RpgChronal17 recebe OnLap no incremento real das voltas; preparação na4.6, entrada5, novo choque6..9, ápice10. Tick dentro de Secret.Advance, sem Update independente de tempo. Strength=0antes5, .66na5 até1na9. Não mudar contagem/velocidade nem usar timeScale global.
- RpgTemporalFlight16 compõe Vfx_ChronalRift17, história de1frame e memória a160ms com torção/periferia a partir5. Limpar ambosRenderTextures ao pausar/wrap/encerrar/destruir. Centro e HUD preservados; atlas16quadros4x4,256/célula,gutter16. Generated17 contém fonteimage_gen/prompts/importador. Não substituir por geometrias/ícones.
- SoundDesign17 contém8OGGs originais KenneySci-FiSounds1.0 CC0, licença oficial e decodePCM;7derivados48kHzestéreo comreceita seed1717. RpgChronal17 usa2loops2D; RpgAudio.Play reconhecechronal com prioridade20/spatialBlend0. BackgroundGain reduzcamadasantigas noschoques. M/limiterexistentespreservados. ÁudiofontesCC0, artesCCBY-SA4.0; não copiar sons de franquias.
- -cibio-chronal-review17 inclui regressões15/16 e ensaios de disparoexato5, preparaçãoúnica, reimpactos, pausa, ducking, saída e clips/PCM. Rendernativo72quadros na passagem4..6; audioProbe tenta mixerUnity. Não confundir préviaDSPoffline com captura nativa ou audição física. BuildUpdatedPlayer sem recriar cena/savev3/GUIDs.

## Atualização 18 — narrativa
- Direção do usuário: sátira política no Brasil da Vuvuzela, sofrimento com Companheiro, boletos, dívidas, juros abusivos, perseguição e fraude eleitoral no universo ficcional. RpgStory18 centraliza os textos públicos. Missão Brasil no Vermelho; regiões Distrito do Poder e Bairro do Trabalhador.
- RpgQuest08 e campos questStage/silenced/captainDefeated mantêm IDs e progresso. silenced representa as provas recuperadas 0..2. Não migrar nem apagar saves. Conclusão, 100 XP e desbloqueios anteriores preservados. RpgResonator08 troca só renderers em runtime por Cobranca18/Perseguicao18/Apuracao18, 512px/PPU192, mantendo objetos, GUIDs e cena byte a byte.
- N abre dossiê mode5 e pausa Playing; Esc/N retorna a mode1. PointerOverHud abrange modal e entrada; não permitir clique vazar para socos. RpgInterface18 usa fontes e estilos existentes. Só exibir conteúdo de provas conforme bits salvos.
- Preservar productName/companyName internos: alterá-los muda o local do PlayerPrefs mesmo mantendo SaveKey. Nomes antigos de arquivos, cenas e tipos permanecem para compatibilidade. Não usar Build para reescrever a cena.
- Generated18 e Fontes-Historia-18.zip contêm artes originais image_gen, prompts e importador. -cibio-story-review18 verifica missão, dossiê, legados, recompensa única, novas artes e regressões. A inspeção visual do dossiê ficou pendente após o usuário interromper Computer Use com Esc; não apresentar captura oculta vazia como evidência visual.

## Atualização 19 — nome e Web
- RpgProgress.playerName é opcional no save v3. RpgIdentity19 valida 2..18 caracteres e normaliza Unicode/espaços; Display faz fallback para Brasileiro. Não trocar SaveKey, companyName ou productName.
- Menu usa BeginJourney: mode6 recebe nome; ConfirmJourney chama StartGame(bool,string) apenas após validação. Cancelar não altera o progresso. Preservar StartGame(bool) compatível com código de revisão. HUD ajusta fonte à largura do nome.
- -cibio-identity-review19 inclui regressões 15..18 e 31 ensaios da identidade, sem tocar no save real.
- RpgWebBuild19.Build usa -buildTarget WebGL e -cibio-build <pasta>, valida a cena, aplica DXT5 só durante build e restaura metas/settings em finally. Nunca usar o gerador RpgBuilder.Build para publicar atualizações.
- Web/site contém a interface do site; Web/prepareweb19.py prepara o transporte Web a partir do player compilado. O manifest .openai/hosting.json conserva o ID do Sites: não criar outro Site ao atualizar. Assets binários compilados ficam no checkout próprio do Sites, não no código-fonte do projeto Unity.
- Site é single player público. Nome e progresso são locais ao navegador, sem conta ou backend multiplayer. Arte existente e créditos originais preservados. Não anunciar compatibilidade móvel sem testar controles próprios.

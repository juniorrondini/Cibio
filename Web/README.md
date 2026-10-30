# Cibio Web — versão 19

Jogue: https://cibio-brasil-no-vermelho.viniandfriends.chatgpt.site

Site público publicado pelo plugin Sites. Single player Unity WebGL 2, teclado e mouse; não há servidor multiplayer ou conta. Nome e progresso ficam no PlayerPrefs/IndexedDB do navegador, separados do Windows. Limpar dados do site ou usar outro navegador pode remover ou separar esse progresso.

O download inicial do jogo tem aproximadamente 54 MiB, além da página/imagens. O build usa DXT5 apenas no Web e entrega data/wasm gzip em partes de até 8 MiB, com SHA-256. Isso mantém os arquivos abaixo do limite de 25 MiB de assets estáticos. O loader verifica integridade, descomprime, trata falhas e só habilita tela cheia depois de iniciar. Os ZIPs históricos de fontes continuam no GitHub; créditos e licenças em texto acompanham o site.

## Gerar uma atualização

1. Preserve o projeto e o save. Use Unity 6000.6.7f1 com WebGL Support.
2. Compile com `-batchmode -quit -buildTarget WebGL -projectPath <projeto> -executeMethod Cibio.RPG.Editor.RpgWebBuild19.Build -cibio-build <pasta-player-Web> -logFile <buildweb.log>`. O builder valida a cena e restaura settings/metas após o build; não recria a cena.
3. Execute `python Web/prepareweb19.py --build-root <pasta-player-Web> --site-root <checkout-do-Sites>/dist --build-log <buildweb.log> --audit <Web-Audit.json>` com o frontend de `Web/site/` no checkout. Requer apenas Python padrão.
4. Use o workflow da skill Sites com o **mesmo project_id** de `site/.openai/hosting.json`. Renove a credencial se necessário, passe-a somente por stdin, envie o source exato, empacote e publique a versão salva. Mantenha acesso público, salvo instrução diferente do usuário. Não criar outro Site para atualizar este.

`publication.json` registra a versão e deployment confirmados. Binários compilados permanecem no repositório próprio do Sites e no pacote Web local; este diretório guarda o frontend e o processo reproduzível.

## Verificação

1.167 checks no player Windows, incluindo 31 da identidade. Web compilado com sucesso, partes gzip reproduzem data e wasm byte a byte; cena, metas e company/product settings originais preservados. No navegador local: carregamento do Unity, tela de nome e nome no HUD observados; página sem overflow horizontal e botão inteiro em 390px. Evidência e limites adicionais em `docs/verification/Web19-Audit.json` e nas imagens publicadas.

O site é para computadores com WebGL 2 e suporte DXT/S3TC. A página adapta-se ao celular; os controles do jogo continuam exigindo teclado e mouse. Áudio e desempenho não foram medidos nesta publicação.

Referências: [Deploy Web Unity](https://docs.unity.com/en-us/engine/6000.0/manual/platform-specific/webgl/building-distribution/deploying), [limite de assets Cloudflare](https://developers.cloudflare.com/workers/platform/limits/).

A versão pública também abriu por HTTPS sem autenticação, concluiu o carregamento do jogo e renderizou o menu Unity e a tela de nome. Nenhum erro de console foi observado nessa sessão.

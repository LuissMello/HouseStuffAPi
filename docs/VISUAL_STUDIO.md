# Visual Studio

## Pré-requisitos

- Visual Studio 2022 17.11 ou superior;
- carga de trabalho **ASP.NET e desenvolvimento Web**;
- carga de trabalho **Desenvolvimento com Node.js**;
- SDK .NET 8;
- Node.js 22.13 ou superior;
- `HouseStuffAPi` e `HouseStuffFront` lado a lado.

No Instalador do Visual Studio, escolha **Modificar**, marque as duas cargas de trabalho e aplique. A carga de Node.js é necessária para o Visual Studio carregar `HouseStuffFront.esproj`.

## Banco PostgreSQL real

O perfil não usa mock, banco em memória ou API do Fly.io. O navegador chama o frontend local, que encaminha `/api` para a API local. A API usa `ConnectionStrings:HouseStuff` dos User Secrets e aplica as migrations ao iniciar. O perfil também desabilita o seed de demonstração, portanto não cria `admin@housestuff.local`, potes ou tarefas fictícias nesse banco.

Configure a conexão uma vez, sem adicioná-la ao Git:

```powershell
dotnet user-secrets set "ConnectionStrings:HouseStuff" "<conexão PostgreSQL>" --project src/HouseStuff.Api
```

## Preparar e executar

No PowerShell, dentro de `HouseStuffAPi`:

```powershell
.\scripts\prepare-visual-studio.ps1
```

Depois:

1. abra `HouseStuff.VisualStudio.sln`;
2. na lista ao lado do botão de execução, escolha **HouseStuff completo**;
3. pressione `F5`;
4. acesse `http://localhost:3000/#/login`.

O perfil inicia `HouseStuff.Api` em `http://localhost:5049` e `HouseStuffFront` em `http://localhost:3000`. Se o perfil não aparecer, habilite **Ferramentas > Opções > Recursos de visualização > Habilitar perfis de inicialização de vários projetos**; como alternativa, clique com o botão direito na solução, escolha **Configurar Projetos de Inicialização** e marque os dois projetos como **Iniciar**.

Para repetir apenas as verificações sem reinstalar pacotes:

```powershell
.\scripts\prepare-visual-studio.ps1 -SkipInstall
```

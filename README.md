# Gemini Translator — Tradutor com Explicação de Nuances e Idioms

> **Produto Desenvolvido com Integração à API do Google Gemini**  
> **Backend**: .NET 9 Web API (C#) com Visual Studio Solution (`Translator.sln`)  
> **Frontend**: React Native para Web (React Native Web + Vite)  
> **Apresentação**: PowerPoint incluído (`Translator_Presentation.pptx`)

---

## 🎯 Conceito do Produto

**Gemini Translator** é uma plataforma de tradução inteligente com contextualização cultural e linguística. Ao contrário de ferramentas de tradução comuns que realizam substituições literais ("ao pé da letra"), o **Gemini Translator**:

1. **Traduz o texto** com alta fidelidade semântica para o idioma escolhido.
2. **Detecta e disseca expressões idiomáticas e gírias**, detalhando:
   - **Sentido Literal**: o que a frase significa palavra por palavra.
   - **Sentido Figurativo**: o real significado cultural pretendido.
   - **Contexto e Origem**: a explicação histórica ou cultural por trás da expressão (ex.: por que *"dar com os burros n'água"* significa fracassar; por que *"chutar o balde"* não é *"kick the bucket"*, mas sim *"throw in the towel"*).
   - **Nível de Formalidade**: gíria, coloquial, corporativo ou neutro.
3. **Explica as Escolhas de Tradução (Rationale)**: evidencia o porquê de termos específicos terem sido preferidos e termos literais descartados.
4. **Oferece Alternativas por Tom de Voz**: gera variações recomendadas para situações informais, corporativas, acadêmicas ou diplomáticas.

---

## 🏗️ Arquitetura do Sistema

O sistema é estruturado em três camadas independentes e desacopladas:

```
[ Frontend: React Native Web ] 
         │ (HTTP / JSON)
         ▼
[ Backend: .NET 9 Web API ]
    ├── TranslationController
    ├── TranslationRequestValidator (Regras de negócio)
    ├── IMemoryCache (Cache inteligente em memória)
    └── GeminiTranslationService (Prompt Engineering & Fallback)
         │ (HTTPS / JSON Schema)
         ▼
[ Google Gemini AI (3.8 Flash) ]
```

### Tecnologias Utilizadas
- **Backend**: C# 13, .NET 9.0 SDK, ASP.NET Core Web API, `Scalar.AspNetCore` (OpenAPI interativo), `Microsoft.Extensions.Caching.Memory`.
- **Frontend**: React 18, React Native for Web (`react-native-web`), Vite.
- **Inteligência Artificial**: Google Gemini API (`gemini-3.8-flash`), Google AI Studio.
- **Apresentação**: Microsoft PowerPoint (`.pptx`) gerado via script Python automatizado (`python-pptx`).

---

## 📂 Estrutura de Pastas

```
Translator/
├── backend/
│   ├── Translator.sln                             # Solução Visual Studio
│   └── src/
│       └── Translator.Api/
│           ├── Controllers/
│           │   └── TranslationController.cs       # Endpoints REST
│           ├── Models/
│           │   ├── TranslationRequest.cs          # DTO de entrada
│           │   ├── TranslationResponse.cs         # DTO de saída estruturada
│           │   ├── SupportedLanguages.cs          # Catálogo de idiomas e presets
│           │   └── GeminiApiModels.cs             # Contratos da API Gemini
│           ├── Options/
│           │   └── GeminiOptions.cs               # Configurações tipadas
│           ├── Services/
│           │   ├── ITranslationService.cs         # Interface de serviço
│           │   └── GeminiTranslationService.cs    # Integração com Gemini e cache
│           ├── Validators/
│           │   └── TranslationRequestValidator.cs # Regras de validação
│           ├── appsettings.json                   # Configurações gerais
│           ├── appsettings.Development.json       # Configurações de desenvolvimento
│           ├── Program.cs                         # Pipeline de DI e middlewares
│           └── Translator.Api.csproj
├── frontend/
│   ├── index.html                                 # Ponto de montagem Web
│   ├── package.json                               # Dependências React Native
│   ├── vite.config.js                             # Configuração Vite + React Native Web
│   └── src/
│       ├── main.jsx                               # Entrypoint React
│       ├── App.jsx                                # Container principal React Native
│       ├── api/
│       │   └── translatorApi.js                   # Cliente HTTP REST
│       ├── i18n/
│       │   └── translations.js                    # Dicionário pt-BR (padrão) e en-US
│       ├── theme/
│       │   └── colors.js                          # Paleta de cores moderna (Dark UI)
│       └── components/
│           ├── Header.jsx                         # Cabeçalho minimalista com alternador PT/EN
│           ├── LanguageSelector.jsx               # Seleção de idiomas e swap
│           ├── ToneSelector.jsx                   # Seletor de tom (Formal, Casual, etc.)
│           ├── QuickPresets.jsx                   # Menu suspenso compacto de expressões
│           ├── TranslationInput.jsx               # Área de entrada de texto
│           ├── TranslationOutput.jsx              # Exibição da tradução e síntese
│           ├── ExplanationPanel.jsx               # Painel minimalista com abas sob demanda
│           ├── IdiomsSection.jsx                  # Explicações de expressões idiomáticas
│           ├── ChoicesSection.jsx                 # Justificativa de escolhas linguísticas
│           └── AlternativesSection.jsx            # Variações por contexto
├── presentation/
│   ├── generate_slides.py                         # Script gerador da apresentação
│   └── Translator_Presentation.pptx               # Apresentação em PowerPoint
└── README.md
```

---

## ⚙️ Como Configurar e Executar

### Pré-requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (versão 18+)
- [Google Gemini API Key](https://aistudio.google.com/app/apikey) (gratuita no Google AI Studio)
- [Python 3](https://www.python.org/) (opcional, caso queira regerar a apresentação)

---

### 1. Configurando a Chave da API do Gemini

Obtenha sua chave gratuita em [Google AI Studio](https://aistudio.google.com/app/apikey).

Você pode informá-la de duas formas:

#### Opção A: Variável de Ambiente (Recomendado)
No terminal antes de rodar o backend:
```powershell
$env:GEMINI_API_KEY="SUA_CHAVE_AQUI"
```

#### Opção B: Arquivo `appsettings.json`
Edite o arquivo `backend/src/Translator.Api/appsettings.json`:
```json
"Gemini": {
  "ApiKey": "SUA_CHAVE_AQUI",
  "Model": "gemini-3.8-flash",
  "EndpointUrl": "https://generativelanguage.googleapis.com/v1beta/models",
  "TimeoutSeconds": 30,
  "EnableCache": true,
  "CacheDurationMinutes": 60
}
```

> **Nota de Resiliência / Modo Demonstração**:  
> Se nenhuma chave for configurada, a API ativa automaticamente o **Modo Demonstração**, respondendo com análises contextuais completas para expressões clássicas como *"Chutar o balde"*, *"Dar com os burros n'água"*, *"Break a leg"*, *"Bite the bullet"*, etc. Isso garante que a aplicação possa ser avaliada imediatamente mesmo antes da inserção de credenciais.

---

### 2. Executando o Backend (.NET 9)

#### Pelo Visual Studio:
1. Abra o arquivo `backend/Translator.sln` no Visual Studio.
2. Defina `Translator.Api` como projeto de inicialização.
3. Pressione `F5` ou `Ctrl + F5`.

#### Pelo Terminal:
```bash
cd backend/src/Translator.Api
dotnet run --urls "http://localhost:5225"
```

A API estará rodando em:
- **API Base**: `http://localhost:5225`
- **Health Check**: `http://localhost:5225/api/translation/health`
- **Documentação OpenAPI / Scalar**: `http://localhost:5225/scalar/v1`

---

### 3. Executando o Frontend (React Native Web)

Abra outro terminal:
```bash
cd frontend
npm install
npm run dev
```

Acesse no navegador:
👉 **`http://localhost:3000`**

> **Interface Minimalista e Suporte a Idiomas (i18n)**:  
> - A interface vem em **Português (pt-BR)** por padrão.  
> - Possui um alternador no cabeçalho para alternar instantaneamente para **Inglês (en-US)**.  
> - Design limpo e focado, no estilo DeepL/Google Translate, com as explicações detalhadas organizadas em abas sob demanda para evitar sobrecarga de informação.

---

### 4. Apresentação em PowerPoint (.pptx)

A apresentação pronta para entrega ao professor está localizada em:
📂 `presentation/Translator_Presentation.pptx`

Estrutura dos 7 slides:
1. **Capa**: ContextLingo — Tradutor Contextual Inteligente com Explicação de Idioms.
2. **Conceito do Produto**: Superando a tradução literal e os falsos amigos.
3. **Arquitetura da Solução**: Frontend React Native, Backend .NET 9 e Google Gemini AI.
4. **Regras de Negócio e Validações**: Validação de borda, ProblemDetails (RFC 9457), cache em memória.
5. **Integração com Gemini API**: Prompt Engineering, JSON Schema estruturado, custo e velocidade.
6. **Demonstrativo em Funcionamento**: Casos de teste práticos e comparativos de tom.
7. **Conclusão & Roadmap**: Requisitos atendidos e próximas evoluções do produto.

Para gerar novamente a apresentação via Python:
```bash
cd presentation
python generate_slides.py
```

---

## 🧪 Regras de Negócio Implementadas no Backend

- **Validação de Tamanho**: O texto de entrada deve possuir entre 1 e 2000 caracteres.
- **Validação de Idiomas**: Verifica conformidade com códigos de idioma suportados.
- **Bloqueio de Redundância**: O idioma de origem não pode ser idêntico ao idioma de destino.
- **Sanitização de Dados**: Limpeza de caracteres invisíveis e normalização de quebras de linha (`\r\n` -> `\n`).
- **Cache de Respostas**: `IMemoryCache` chaveado por `[origem:destino:tom:hash_do_texto]` com expiração deslizante. Requisições idênticas retornam em ~0ms economizando cotas do Gemini.
- **Tratamento de Erros RFC 9457**: Retorno padronizado de `ProblemDetails` para respostas HTTP 400 e 500.
- **Código Limpo**: Sem comentários nos arquivos de código, nomenclatura estritamente em inglês para classes, métodos e propriedades.

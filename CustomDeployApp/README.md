# CustomDeployApp

Frontend do sistema CustomDeploy, desenvolvido em React + TypeScript, utilizando Vite para build e desenvolvimento rápido.

## Principais Tecnologias
- **React** 19+
- **TypeScript**
- **Vite**
- **Redux Toolkit** (gerenciamento de estado)
- **react-router-dom** (roteamento)
- **styled-components** (estilização)
- **axios** (requisições HTTP)
- **@dnd-kit** (drag & drop)
- **react-toastify** (notificações)

## Estrutura de r
```
src/
  assets/           # Imagens e arquivos estáticos
  components/       # Componentes reutilizáveis
  hooks/            # Custom hooks
  infra/            # Infraestrutura/utilitários de baixo nível
  services/         # Serviços de API/lógica de negócio
  store/            # Configuração do Redux
  utils/            # Funções utilitárias
  views/            # Telas/páginas
  App.tsx           # Componente principal
  main.tsx          # Ponto de entrada
```

## Scripts
- `npm run dev` — Inicia o servidor de desenvolvimento
- `npm run build` — Gera build de produção
- `npm run lint` — Executa o ESLint
- `npm run preview` — Preview do build

## Como rodar o projeto
1. Instale as dependências:
   ```sh
   npm install
   ```
2. Inicie o servidor de desenvolvimento:
   ```sh
   npm run dev
   ```
3. Acesse `http://localhost:5173` (ou porta configurada pelo Vite)

## Build de produção
```sh
npm run build
```
Os arquivos finais estarão em `dist/`.

## Lint
```sh
npm run lint
```

## Observações
- O projeto utiliza tipagem forte e linting para garantir qualidade de código.

---

## 👨‍💻 Autor

**Eric** - [GitHub](https://github.com/EricCoisa)
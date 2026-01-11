# AI_CB

This is a practice project. I plan optimize, refactor, add features, etc.

---

## AI_CB API (AI_CB_API)

This ASP.NET Core project exposes endpoints used by the client.

### Available endpoints

- `GET /api/fortune/questions` — returns three random questions for the Fortune Teller UI.
- `POST /api/fortune/generate` — accepts `{ answers: string[] }` and returns a generated fortune.

### Configuration

Add AI provider keys to `appsettings.json` or use environment variables if you want the server to call an LLM provider for fortunes (OpenAI or Hugging Face). Example:

```json
{
  "AIProvider": "OpenAI",
  "OpenAI": { "ApiKey": "sk-..." }
}
```

Environment variables supported:
- `AI_PROVIDER` = `OpenAI` or `HuggingFace`
- `OPENAI_API_KEY` = your OpenAI key
- `HUGGINGFACE_API_KEY` = your Hugging Face key

### Run locally

```bash
dotnet run --project AI_CB_API.csproj
```

The API listens on `https://localhost:5010` by default (see `Program.cs`).

---

## Client (AI_CB_Client)

React + Vite frontend. The Fortune Teller UI uses the endpoints above. The chat UI that previously used `/api/openai/generate` requires the OpenAI controller; it's not available in this branch.

### Run the client

```bash
cd AI_CB_Client
npm install
npm run dev
```

Notes:
- Vite proxies `/api` to the backend during development (see `vite.config.ts`). If API calls return HTML, ensure the backend is running and proxy target matches the API scheme/port.

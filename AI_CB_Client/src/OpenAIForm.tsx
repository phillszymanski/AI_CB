import { useState, useRef } from 'react'

type Role = 'user' | 'assistant' | 'system'
type Message = { id: string; role: Role; content: string }

export default function OpenAIForm() {
  const [messages, setMessages] = useState<Message[]>([
    { id: 'm0', role: 'system', content: 'You are a helpful assistant.' },
  ])
  const [input, setInput] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const messagesEndRef = useRef<HTMLDivElement | null>(null)

  function scrollToBottom() {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }

  async function sendMessage(content: string) {
    if (!content.trim()) return
    setError(null)

    const userMsg: Message = { id: String(Date.now()), role: 'user', content }
    const convo = [...messages, userMsg]
    setMessages(convo)
    setInput('')
    setLoading(true)
    scrollToBottom()

    try {
      // Build a single prompt from the conversation to give context
      const prompt = convo
        .map((m) => {
          const speaker = m.role === 'user' ? 'User' : m.role === 'assistant' ? 'Assistant' : 'System'
          return `${speaker}: ${m.content}`
        })
        .join('\n')

      const resp = await fetch('/api/openai/generate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ prompt })
      })

      const text = await resp.text()
      if (!resp.ok) {
        setError(`Server returned ${resp.status}: ${text}`)
        return
      }

      // Try to parse assistant content from OpenAI-like response
      let assistantText: string
      try {
        const parsed = JSON.parse(text)
        if (parsed.choices && parsed.choices[0]?.message?.content) {
          assistantText = parsed.choices[0].message.content
        } else if (parsed.choices && parsed.choices[0]?.text) {
          assistantText = parsed.choices[0].text
        } else {
          assistantText = JSON.stringify(parsed, null, 2)
        }
      } catch (e) {
        assistantText = text
      }

      const assistantMsg: Message = { id: String(Date.now() + 1), role: 'assistant', content: assistantText }
      setMessages((m) => [...m, assistantMsg])
      scrollToBottom()
    } catch (err: any) {
      setError(err?.message || String(err))
    } finally {
      setLoading(false)
    }
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (loading) return
    sendMessage(input)
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      if (input.trim()) sendMessage(input)
    }
  }

  return (
    <section className="chat-container">
      <h2>Chat with the API</h2>

      <div className="messages" aria-live="polite">
        {messages.map((m) => (
          <div key={m.id} className={`message ${m.role}`}>
            <div className="message-content">{m.content}</div>
          </div>
        ))}
        {loading && (
          <div className="message assistant typing" key="typing">
            <div className="message-content">
              <span className="dot" />
              <span className="dot" />
              <span className="dot" />
            </div>
          </div>
        )}
        <div ref={messagesEndRef} />
      </div>

      {error && <div className="error">Error: {error}</div>}

      <form onSubmit={handleSubmit} className="input-row">
        <textarea
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder={loading ? 'Waiting for response…' : 'Type a message and press Enter'}
          rows={2}
          disabled={loading}
        />
        <div className="input-actions">
          <button type="submit" disabled={loading || !input.trim()}>
            {loading ? 'Sending…' : 'Send'}
          </button>
        </div>
      </form>
    </section>
  )
}

import { useEffect, useState } from 'react'

export default function FortuneTeller() {
  const [questions, setQuestions] = useState<string[] | null>(null)
  const [answers, setAnswers] = useState<string[]>([])
  const [index, setIndex] = useState(0)
  const [loading, setLoading] = useState(false)
  const [fortune, setFortune] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    // reset when component mounts
    reset()
  }, [])

  function reset() {
    setQuestions(null)
    setAnswers([])
    setIndex(0)
    setLoading(false)
    setFortune(null)
    setError(null)
  }

  async function start() {
    setLoading(true)
    setError(null)
    try {
      const resp = await fetch('/api/fortune/questions')
      if (!resp.ok) {
        const body = await resp.text()
        throw new Error(body || resp.statusText)
      }

      const ct = resp.headers.get('content-type') || ''
      let data: string[]
      if (ct.includes('application/json')) {
        data = await resp.json()
      } else {
        const text = await resp.text()
        throw new Error('Expected JSON from /api/fortune/questions but got: ' + (text.slice(0, 200) || resp.statusText))
      }
      setQuestions(data)
      setAnswers(new Array(data.length).fill(''))
      setIndex(0)
    } catch (e: any) {
      setError(e?.message || String(e))
    } finally {
      setLoading(false)
    }
  }

  function updateAnswer(i: number, value: string) {
    setAnswers((a) => {
      const copy = [...a]
      copy[i] = value
      return copy
    })
  }

  async function submitAnswers() {
    setLoading(true)
    setError(null)
    setFortune(null)
    try {
      const resp = await fetch('/api/fortune/generate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ answers })
      })
      if (!resp.ok) {
        const body = await resp.text()
        throw new Error(body || resp.statusText)
      }

      const ct = resp.headers.get('content-type') || ''
      if (ct.includes('application/json')) {
        const json = await resp.json()
        setFortune(json.fortune)
      } else {
        const text = await resp.text()
        throw new Error('Expected JSON from /api/fortune/generate but got: ' + (text.slice(0, 200) || resp.statusText))
      }
    } catch (e: any) {
      setError(e?.message || String(e))
    } finally {
      setLoading(false)
    }
  }

  if (!questions) {
    return (
      <section className="fortune-card">
        <h2>Fortune Teller</h2>
        <p>Answer three quick questions to reveal your fortune.</p>
        <div style={{marginTop:12}}>
          <button onClick={start} disabled={loading}>{loading ? 'Loading…' : 'Start'}</button>
        </div>
        {error && <div className="error">{error}</div>}
      </section>
    )
  }

  if (fortune) {
    return (
      <section className="fortune-card">
        <h2>Your Fortune</h2>
        <div className="fortune-result">{fortune}</div>
        <div style={{marginTop:12}}>
          <button onClick={reset}>Do it again</button>
        </div>
      </section>
    )
  }

  return (
    <section className="fortune-card">
      <h2>Fortune Teller</h2>
      <div className="fortune-qa">
        <div className="question">{questions[index]}</div>
        <textarea
          value={answers[index] ?? ''}
          onChange={(e) => updateAnswer(index, e.target.value)}
          rows={3}
          placeholder="Type your answer here"
        />
        <div style={{marginTop:8}}>
          <button onClick={() => setIndex((i) => Math.max(0, i - 1))} disabled={index === 0}>Back</button>
          {index < questions.length - 1 ? (
            <button onClick={() => setIndex((i) => Math.min(questions.length - 1, i + 1))} style={{marginLeft:8}}>Next</button>
          ) : (
            <button onClick={submitAnswers} style={{marginLeft:8}} disabled={loading}> {loading ? 'Revealing…' : 'Reveal Fortune'}</button>
          )}
        </div>
        {error && <div className="error">{error}</div>}
      </div>
    </section>
  )
}

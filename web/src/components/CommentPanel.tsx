import { useState } from 'react'
import { MessageSquarePlus, Send } from 'lucide-react'
import { useAddComment, useComments } from '@/features/comments/api'
import type { CommentSubject } from '@/features/comments/api'
import { formatDateTime } from '@/lib/utils'

const commentTypeLabels: Record<string, string> = {
  General: 'Genel',
  CustomerInfo: 'Müşteri Bilgisi',
  Operation: 'Operasyon',
  Finance: 'Finans',
  Customs: 'Gümrük',
  Risk: 'Risk',
  Management: 'Yönetim',
  Carrier: 'Taşıyıcı',
}

export function CommentPanel({ subject }: { subject: CommentSubject }) {
  const { data: comments, isLoading } = useComments(subject)
  const add = useAddComment(subject)
  const [body, setBody] = useState('')
  const [type, setType] = useState('Operation')

  const submit = async () => {
    if (!body.trim()) return
    const mentions = body.match(/@\w+/g) ?? []
    await add.mutateAsync({ type, body: body.trim(), mentions })
    setBody('')
  }

  return (
    <div className="border-t px-5 py-4">
      <div className="mb-2 flex items-center gap-2 text-sm font-semibold text-slate-900">
        <MessageSquarePlus className="h-4 w-4 text-primary" /> Yorumlar
      </div>

      <div className="mb-3 space-y-2">
        <div className="flex gap-2">
          <select
            value={type}
            onChange={(e) => setType(e.target.value)}
            className="rounded-lg border bg-white px-2 py-1.5 text-xs outline-none"
          >
            {Object.entries(commentTypeLabels).map(([k, v]) => (
              <option key={k} value={k}>{v}</option>
            ))}
          </select>
          <span className="text-xs text-muted-foreground">@etiket ile kişi/departman belirtebilirsiniz</span>
        </div>
        <div className="flex gap-2">
          <textarea
            value={body}
            onChange={(e) => setBody(e.target.value)}
            placeholder="Yorum yazın... (örn. @Finans tahsilat durumu?)"
            rows={2}
            className="flex-1 resize-none rounded-lg border bg-white px-3 py-2 text-sm outline-none focus:border-primary"
          />
          <button
            onClick={submit}
            disabled={add.isPending || !body.trim()}
            className="flex h-9 items-center gap-1 self-end rounded-lg bg-primary px-3 text-sm font-medium text-white disabled:opacity-50"
          >
            <Send className="h-4 w-4" />
          </button>
        </div>
      </div>

      {isLoading ? (
        <div className="py-3 text-center text-sm text-muted-foreground">Yükleniyor...</div>
      ) : !comments?.length ? (
        <div className="py-3 text-center text-sm text-muted-foreground">Henüz yorum yok.</div>
      ) : (
        <ul className="space-y-3">
          {comments.map((c) => (
            <li key={c.id} className="rounded-lg bg-secondary/50 p-3">
              <div className="mb-1 flex items-center justify-between gap-2 text-xs">
                <span className="font-semibold text-slate-800">{c.authorName}</span>
                <span className="text-muted-foreground">
                  {commentTypeLabels[c.type] ?? c.type} · {formatDateTime(c.createdAt)}
                </span>
              </div>
              <p className="whitespace-pre-wrap text-sm text-slate-700">{c.body}</p>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

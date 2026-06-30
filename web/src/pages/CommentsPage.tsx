import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { MessageSquare, ExternalLink, Loader2 } from 'lucide-react'
import { Card } from '@/components/ui/Card'
import { useRecentComments, type CommentItem } from '@/features/comments/api'
import { cn } from '@/lib/utils'

const GROUPS = ['Tümü', 'Deniz', 'Kara', 'Hava', 'Finans'] as const
type GroupTab = (typeof GROUPS)[number]

const groupRoute: Record<string, string> = { Deniz: '/deniz', Kara: '/karayolu', Hava: '/hava', Finans: '/finans' }

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

function relativeTime(iso: string): string {
  const m = Math.floor((Date.now() - new Date(iso).getTime()) / 60000)
  if (m < 1) return 'az önce'
  if (m < 60) return `${m} dk önce`
  const h = Math.floor(m / 60)
  if (h < 24) return `${h} saat önce`
  const d = Math.floor(h / 24)
  if (d < 30) return `${d} gün önce`
  return new Date(iso).toLocaleDateString('tr-TR')
}

export function CommentsPage() {
  const navigate = useNavigate()
  const [tab, setTab] = useState<GroupTab>('Tümü')
  const { data, isLoading } = useRecentComments(tab === 'Tümü' ? undefined : tab, 100)

  const openSource = (c: CommentItem) => {
    if (c.boardKey && c.group && groupRoute[c.group]) {
      navigate(`${groupRoute[c.group]}?board=${encodeURIComponent(c.boardKey)}&q=${encodeURIComponent(c.recordRef ?? '')}`)
    } else if (c.operationId) {
      navigate(`/operasyonlar?op=${c.operationId}`)
    }
  }

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Yorumlar ve İç İletişim</h1>
        <p className="text-sm text-muted-foreground">
          Tüm operasyon ve takip tablosu satırlarına eklenen en yeni yorumlar — kaynağına tıklayarak ilgili satıra gidin.
        </p>
      </div>

      <div className="flex flex-wrap gap-2 border-b">
        {GROUPS.map((g) => (
          <button
            key={g}
            onClick={() => setTab(g)}
            className={cn(
              'border-b-2 px-3 py-2 text-sm font-medium transition',
              tab === g ? 'border-primary text-primary' : 'border-transparent text-muted-foreground hover:text-slate-900',
            )}
          >
            {g}
          </button>
        ))}
      </div>

      {isLoading ? (
        <div className="flex items-center gap-2 py-12 text-sm text-muted-foreground">
          <Loader2 className="h-4 w-4 animate-spin" /> Yükleniyor...
        </div>
      ) : !data?.length ? (
        <Card className="flex flex-col items-center gap-2 py-12 text-center">
          <MessageSquare className="h-8 w-8 text-slate-300" />
          <p className="text-sm text-muted-foreground">
            {tab === 'Tümü' ? 'Henüz yorum eklenmemiş.' : `${tab} grubunda yorum yok.`}
          </p>
          <p className="text-xs text-muted-foreground">
            Bir satıra yorum eklemek için ilgili sekmede satıra tıklayıp yan panelden ekleyebilirsiniz.
          </p>
        </Card>
      ) : (
        <div className="space-y-2.5">
          {data.map((c) => (
            <Card key={c.id} className="p-4">
              <div className="flex items-start justify-between gap-3">
                <div className="flex min-w-0 items-center gap-2">
                  <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
                    {c.authorName.charAt(0)}
                  </div>
                  <div className="min-w-0">
                    <span className="text-sm font-semibold text-slate-900">{c.authorName}</span>
                    <span className="ml-2 rounded bg-secondary px-1.5 py-0.5 text-[11px] font-medium text-slate-600">
                      {commentTypeLabels[c.type] ?? c.type}
                    </span>
                  </div>
                </div>
                <span className="shrink-0 text-xs text-muted-foreground">{relativeTime(c.createdAt)}</span>
              </div>

              <p className="mt-2 whitespace-pre-line text-sm text-slate-700">{c.body}</p>

              {c.mentions.length > 0 && (
                <div className="mt-2 flex flex-wrap gap-1">
                  {c.mentions.map((m) => (
                    <span key={m} className="rounded-full bg-sky-50 px-2 py-0.5 text-[11px] font-medium text-sky-700">
                      @{m}
                    </span>
                  ))}
                </div>
              )}

              {(c.boardTitle || c.recordRef) && (
                <button
                  onClick={() => openSource(c)}
                  className="mt-3 inline-flex items-center gap-1.5 text-xs font-medium text-primary transition hover:underline"
                >
                  <ExternalLink className="h-3.5 w-3.5" />
                  {c.boardTitle ?? 'Operasyon'}{c.recordRef ? ` · ${c.recordRef}` : ''}
                </button>
              )}
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}

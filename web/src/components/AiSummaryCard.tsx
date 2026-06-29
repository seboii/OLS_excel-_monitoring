import { Sparkles } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card'
import { useAiSummary } from '@/features/ai/api'

export function AiSummaryCard() {
  const { data, isLoading } = useAiSummary()

  return (
    <Card>
      <CardHeader className="flex-row items-center gap-2">
        <Sparkles className="h-5 w-5 text-primary" />
        <CardTitle>AI Yönetim Yorumu</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3 pt-0">
        {isLoading ? (
          <div className="text-sm text-muted-foreground">Hazırlanıyor...</div>
        ) : (
          data?.sections.map((s) => (
            <div key={s.title}>
              <div className="text-sm font-semibold text-slate-800">{s.title}</div>
              <p className="whitespace-pre-line text-sm text-muted-foreground">{s.body}</p>
            </div>
          ))
        )}
        <p className="border-t pt-2 text-xs text-muted-foreground/60">
          Kural tabanlı özet — kesin karar değil, öneri niteliğindedir.
        </p>
      </CardContent>
    </Card>
  )
}

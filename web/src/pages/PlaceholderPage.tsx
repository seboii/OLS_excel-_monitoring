import { Construction } from 'lucide-react'

export function PlaceholderPage({ title }: { title: string }) {
  return (
    <div className="space-y-5">
      <h1 className="text-2xl font-bold text-slate-900">{title}</h1>
      <div className="flex flex-col items-center justify-center gap-3 rounded-xl border border-dashed bg-card py-20 text-center">
        <div className="flex h-14 w-14 items-center justify-center rounded-full bg-primary/10 text-primary">
          <Construction className="h-7 w-7" />
        </div>
        <div className="text-lg font-semibold text-slate-800">Bu modül yakında</div>
        <p className="max-w-md text-sm text-muted-foreground">
          Sprint planına göre geliştiriliyor. Veri modeli, API ve risk motoru altyapısı hazır;
          bu ekran sonraki sprintlerde devreye alınacak.
        </p>
      </div>
    </div>
  )
}

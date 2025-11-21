import { useMemo } from 'react'
import { useReactTable, getCoreRowModel } from '@tanstack/react-table'
import { Article } from '@types'

export const useArticlesTable = (data: Article[]) => {
  const columns = useMemo(() => [
    { accessorKey: 'titulo', header: 'Título' },
    { accessorKey: 'autores', header: 'Autores' },
    { accessorKey: 'ano_publicacao', header: 'Ano' },
  ], [])

  return useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
  })
}

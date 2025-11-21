import { useMemo, useState } from 'react'
import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  SortingState,
  ColumnFiltersState,
  PaginationState,
} from '@tanstack/react-table'
import type { Article } from '@types'

export function useArticlesTable(articles: Article[]) {
  const [sorting, setSorting] = useState<SortingState>([
    { id: 'data_processamento', desc: true },
  ])
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([])
  const [globalFilter, setGlobalFilter] = useState('')
  const [pagination, setPagination] = useState<PaginationState>({
    pageIndex: 0,
    pageSize: 50,
  })

  const columns = useMemo(
    () => [
      {
        accessorKey: 'titulo',
        header: 'Título',
        size: 350,
      },
      {
        accessorKey: 'ano_publicacao',
        header: 'Ano',
        size: 80,
      },
      {
        accessorKey: 'autores',
        header: 'Autores',
        cell: (info: any) => {
          const autores = info.getValue()
          return autores && autores.length > 0
            ? `${autores[0].nome} ${autores.length > 1 ? `et al.` : ''}`
            : '-'
        },
        size: 150,
      },
      {
        accessorKey: 'status',
        header: 'Status',
        cell: (info: any) => {
          const status = info.getValue()
          return status === 'finalizado' ? '✅ Finalizado' : '📝 Rascunho'
        },
        size: 120,
      },
      {
        accessorKey: 'data_processamento',
        header: 'Data',
        cell: (info: any) => {
          const date = new Date(info.getValue())
          return date.toLocaleDateString('pt-BR')
        },
        size: 120,
      },
      {
        accessorKey: 'editado_manualmente',
        header: 'Editado',
        cell: (info: any) => {
          return info.getValue() ? '🖊️ Sim' : 'Não'
        },
        size: 100,
      },
    ],
    []
  )

  const table = useReactTable({
    data: articles,
    columns,
    state: {
      sorting,
      columnFilters,
      globalFilter,
      pagination,
    },
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    onGlobalFilterChange: setGlobalFilter,
    onPaginationChange: setPagination,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    globalFilterFn: 'includesString',
  })

  return {
    table,
    globalFilter,
    setGlobalFilter,
    sorting,
    setSorting,
  }
}

export interface PaginatedResult<T> {
  items: T[]
  pageIndex: number
  totalPages: number
  pageSize: number
  totalCount: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

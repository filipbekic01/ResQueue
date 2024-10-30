import { format, formatDistance, isFuture } from 'date-fns'

const humanDateTime = (dt?: string) => {
  if (!dt) {
    return null
  }

  const isf = isFuture(new Date(dt))
  const dt1 = format(new Date(dt), 'MMM dd HH:mm:ss')
  const dt2 = formatDistance(new Date(dt), new Date())

  return `${dt1} (${isf ? 'in ' : ''}${dt2}${isf ? '' : ' ago'})`
}

export { humanDateTime }

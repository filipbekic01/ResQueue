export type HighlightColorOptions = {
  keyColor?: string
  numberColor?: string
  stringColor?: string
  trueColor?: string
  falseColor?: string
  nullColor?: string
}

export function highlightJson(json: any, colorOptions: HighlightColorOptions = {}): string {
  const entityMap: { [key: string]: string } = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;',
    '`': '&#x60;',
    '=': '&#x3D;'
  }

  function escapeHtml(html: string): string {
    return String(html).replace(/[&<>"'`=]/g, (s) => entityMap[s])
  }

  let jsonString: string
  const valueType = typeof json

  if (valueType !== 'string') {
    jsonString = JSON.stringify(json, null, 4) || valueType
  } else {
    jsonString = json
  }

  const colors: HighlightColorOptions = {
    ...{
      keyColor: '#1e3a8a', // Tailwind Blue-800 for keys
      numberColor: '#16a34a', // Tailwind Green-600 for numbers
      stringColor: '#dc2626', // Tailwind Red-600 for strings
      trueColor: '#22c55e', // Tailwind Green-500 for true
      falseColor: '#ef4444', // Tailwind Red-500 for false
      nullColor: '#1e3a8a' // Same as key color (Tailwind Blue-800)
    },
    ...colorOptions
  }

  jsonString = jsonString.replace(/&/g, '&').replace(/</g, '<').replace(/>/g, '>')

  return jsonString.replace(
    /("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+]?\d+)?)/g,
    (match) => {
      let color = colors.numberColor!
      let style = ''

      if (/^"/.test(match)) {
        if (/:$/.test(match)) {
          color = colors.keyColor!
        } else {
          color = colors.stringColor!
          match = `"${escapeHtml(match.substr(1, match.length - 2))}"`
          style = 'word-wrap:break-word;white-space:pre-wrap;'
        }
      } else {
        color = /true/.test(match)
          ? colors.trueColor!
          : /false/.test(match)
            ? colors.falseColor!
            : /null/.test(match)
              ? colors.nullColor!
              : color
      }

      return `<span style="${style}color:${color}">${match}</span>`
    }
  )
}

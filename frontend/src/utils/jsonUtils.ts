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
      keyColor: '#0451a5', // Visual Studio Code Blue for keys
      numberColor: '#339933', // Visual Studio Code Light greenish for numbers
      stringColor: '#a31515', // Visual Studio Code Dark red for strings
      trueColor: '#098658', // Visual Studio Code Green for true
      falseColor: '#b31b1b', // Visual Studio Code Red for false
      nullColor: '#3a3d41' // Visual Studio Code Greyish color for null
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

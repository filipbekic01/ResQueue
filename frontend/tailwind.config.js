/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  darkMode: 'class',
  // eslint-disable-next-line @typescript-eslint/no-require-imports
  plugins: [require('tailwindcss-primeui')],
}

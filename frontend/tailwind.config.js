/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  darkMode: 'class',
  // eslint-disable-next-line no-undef
  plugins: [require('tailwindcss-primeui')]
}

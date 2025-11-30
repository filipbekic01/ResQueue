const fs = require("fs");
const path = require("path");
const os = require("os");

function getTsConfigCompilerOptionsPaths() {
  const data = fs.readFileSync(path.join(__dirname, "./tsconfig.app.json"), "utf8");
  const dataJson = JSON.parse(data);
  const keys = Object.entries(dataJson.compilerOptions.paths).map(([key, value]) => `(${key.replace("/*", "/(.*)")})`);
  return keys;
}

module.exports = {
  printWidth: 120,
  plugins: ["@ianvs/prettier-plugin-sort-imports", "prettier-plugin-tailwindcss"],
  importOrder: [`^(${getTsConfigCompilerOptionsPaths().join("|")})$`, "^((.)|(..))/(.*)$"],
  endOfLine: os.EOL === "\r\n" ? "crlf" : "lf",
};

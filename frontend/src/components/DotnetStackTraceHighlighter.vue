<script lang="ts" setup>
import { computed } from "vue";

const props = defineProps<{
  stackTrace: string;
}>();

interface StackTraceLine {
  type: "frame" | "message" | "separator" | "empty";
  raw: string;
  parsed?: {
    atPrefix: string;
    namespace: string;
    className: string;
    methodName: string;
    genericParams?: string;
    parameters: string;
    fileInfo?: {
      path: string;
      lineNumber: string;
    };
  };
}

const parseStackTrace = computed((): StackTraceLine[] => {
  if (!props.stackTrace) return [];

  const lines = props.stackTrace.split("\n");

  return lines.map((line): StackTraceLine => {
    const trimmed = line.trim();

    if (!trimmed) {
      return { type: "empty", raw: line };
    }

    // Match stack trace separator (async continuation marker)
    if (trimmed.startsWith("---") && trimmed.endsWith("---")) {
      return { type: "separator", raw: line };
    }

    // Match .NET stack trace frame pattern
    // Example: at Namespace.Class.Method(Type param) in /path/file.cs:line 123
    const frameRegex =
      /^(at\s+)?([\w.+<>`,\[\]]+)\.([\w<>]+)\s*(\[[\w,\s]+\])?\s*\(([^)]*)\)(\s+in\s+(.+):line\s+(\d+))?$/;

    const match = trimmed.match(frameRegex);

    if (match) {
      const fullPath = match[2] ?? "";
      const parts = fullPath.split(".");
      const namespace = parts.slice(0, -1).join(".");
      const className = parts[parts.length - 1] ?? "";

      return {
        type: "frame",
        raw: line,
        parsed: {
          atPrefix: match[1] ?? "",
          namespace,
          className,
          methodName: match[3] ?? "",
          genericParams: match[4],
          parameters: match[5] ?? "",
          fileInfo:
            match[7] && match[8]
              ? {
                  path: match[7],
                  lineNumber: match[8],
                }
              : undefined,
        },
      };
    }

    // Alternative pattern for async state machine frames
    // Example: at Namespace.Class.<Method>d__5.MoveNext()
    const asyncRegex = /^(at\s+)?([\w.+]+)\.<(\w+)>([a-z]__\d+)\.(MoveNext)\s*\(([^)]*)\)(\s+in\s+(.+):line\s+(\d+))?$/;

    const asyncMatch = trimmed.match(asyncRegex);

    if (asyncMatch) {
      const fullPath = asyncMatch[2] ?? "";
      const parts = fullPath.split(".");
      const namespace = parts.slice(0, -1).join(".");
      const className = parts[parts.length - 1] ?? "";

      return {
        type: "frame",
        raw: line,
        parsed: {
          atPrefix: asyncMatch[1] ?? "",
          namespace,
          className: `${className}.<${asyncMatch[3]}>${asyncMatch[4]}`,
          methodName: asyncMatch[5] ?? "",
          genericParams: undefined,
          parameters: asyncMatch[6] ?? "",
          fileInfo:
            asyncMatch[8] && asyncMatch[9]
              ? {
                  path: asyncMatch[8],
                  lineNumber: asyncMatch[9],
                }
              : undefined,
        },
      };
    }

    // Pattern for compiler-generated closure/display class with local functions
    // Example: at Namespace.Class`1.<>c__DisplayClass5_0.<<Send>g__SendAsync|1>d.MoveNext()
    const closureRegex =
      /^(at\s+)?([\w.+`]+)\.<>c__DisplayClass[\w_]+\.<<(\w+)>g__(\w+)\|[\d]+>d\.(MoveNext)\s*\(([^)]*)\)(\s+in\s+(.+):line\s+(\d+))?$/;

    const closureMatch = trimmed.match(closureRegex);

    if (closureMatch) {
      const fullPath = closureMatch[2] ?? "";
      const parts = fullPath.split(".");
      const namespace = parts.slice(0, -1).join(".");
      const className = parts[parts.length - 1] ?? "";
      const outerMethod = closureMatch[3] ?? "";
      const localFunc = closureMatch[4] ?? "";

      return {
        type: "frame",
        raw: line,
        parsed: {
          atPrefix: closureMatch[1] ?? "",
          namespace,
          className: `${className}.<${outerMethod}>`,
          methodName: `<${localFunc}>`,
          genericParams: undefined,
          parameters: closureMatch[6] ?? "",
          fileInfo:
            closureMatch[8] && closureMatch[9]
              ? {
                  path: closureMatch[8],
                  lineNumber: closureMatch[9],
                }
              : undefined,
        },
      };
    }

    // Treat as message/exception line
    return { type: "message", raw: line };
  });
});

const formatParameters = (params: string): { type: string; name: string }[] => {
  if (!params.trim()) return [];

  const result: { type: string; name: string }[] = [];
  const paramList = params.split(",");

  for (const param of paramList) {
    const trimmed = param.trim();
    const lastSpace = trimmed.lastIndexOf(" ");

    if (lastSpace > 0) {
      result.push({
        type: trimmed.substring(0, lastSpace),
        name: trimmed.substring(lastSpace + 1),
      });
    } else {
      result.push({ type: trimmed, name: "" });
    }
  }

  return result;
};
</script>

<template>
  <div class="font-mono text-xs leading-relaxed">
    <div v-for="(line, index) in parseStackTrace" :key="index" class="hover:bg-base-300/30 whitespace-nowrap">
      <template v-if="line.type === 'empty'">
        <br />
      </template>

      <template v-else-if="line.type === 'message'">
        <span class="text-error font-medium">{{ line.raw }}</span>
      </template>

      <template v-else-if="line.type === 'separator'">
        <span class="text-base-content/40 italic">{{ line.raw }}</span>
      </template>

      <template v-else-if="line.type === 'frame' && line.parsed">
        <span class="text-base-content/40">{{ line.parsed.atPrefix }}</span>
        <span v-if="line.parsed.namespace" class="text-base-content/60">{{ line.parsed.namespace }}.</span>
        <span class="text-info">{{ line.parsed.className }}</span>
        <span class="text-base-content/40">.</span>
        <span class="text-warning">{{ line.parsed.methodName }}</span>
        <span v-if="line.parsed.genericParams" class="text-success/70">{{ line.parsed.genericParams }}</span>
        <span class="text-base-content/40">(</span>
        <template v-for="(param, paramIndex) in formatParameters(line.parsed.parameters)" :key="paramIndex">
          <span v-if="paramIndex > 0" class="text-base-content/40">, </span>
          <span class="text-success">{{ param.type }}</span
          ><span v-if="param.name" class="text-base-content/70">{{ " " + param.name }}</span>
        </template>
        <span class="text-base-content/40">)</span>

        <template v-if="line.parsed.fileInfo">
          <span class="text-base-content/40"> in </span>
          <span class="text-base-content/40">{{ line.parsed.fileInfo.path }}</span>
          <span class="text-base-content/40">:line </span>
          <span class="text-base-content/70">{{ line.parsed.fileInfo.lineNumber }}</span>
        </template>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

interface Props {
  // 接收联合类型：可以是对象，也可以是字符串
  args: string | Record<string, any>;
}

const props = defineProps<Props>();

// 计算属性：统一转换为 { key, value } 数组
const parsedArgs = computed(() => {
  // 1. 空值处理
  if (!props.args) return [];

  let content = props.args;

  // 2. 如果是字符串，尝试解析为对象
  if (typeof content === 'string') {
    try {
      const parsed = JSON.parse(content);
      // 只有解析结果是“非数组的对象”时，才视为字典处理
      if (parsed && typeof parsed === 'object' && !Array.isArray(parsed)) {
        content = parsed;
      } else {
        // 虽然解析成功但不是字典（如数组、数字），或者解析失败，都视为原始字符串展示
        return [{ key: 'Raw', value: content, type: 'string' }];
      }
    } catch (e) {
      // JSON 解析异常，直接展示原始字符串
      return [{ key: 'Raw', value: content, type: 'string' }];
    }
  }

  // 3. 此时 content 应该是一个对象，进行最后的校验并遍历
  if (content && typeof content === 'object' && !Array.isArray(content)) {
    return Object.keys(content).map(key => ({
      key,
      value: (content as Record<string, any>)[key],
      type: typeof (content as Record<string, any>)[key]
    }));
  }

  // 4. 兜底：既不是字符串也不是合法对象，强转字符串展示
  return [{ key: 'Raw', value: String(props.args), type: 'string' }];
});

// 辅助函数：格式化特定的值
const formatValue = (val: any) => {
  if (val === null) return 'null';
  if (typeof val === 'boolean') return val ? 'True' : 'False';
  if (typeof val === 'object') return JSON.stringify(val);
  return String(val);
};
</script>

<template>
  <div class="arg-viewer">
    <div v-if="parsedArgs.length === 0" class="empty-args">
      无参数
    </div>

    <div v-else class="arg-list">
      <div
        v-for="item in parsedArgs"
        :key="item.key"
        class="arg-item"
      >
        <span class="arg-key">{{ item.key }}:</span>

        <code v-if="item.type === 'string' && (item.value as string).length > 50" class="arg-value long-text">
          {{ item.value }}
        </code>

        <span v-else :class="['arg-value', item.type]">
          {{ formatValue(item.value) }}
        </span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.arg-viewer {
  background-color: #f8f9fa;
  border-radius: 6px;
  padding: 8px 12px;
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 0.9em;
  border: 1px solid #e9ecef;
}

.empty-args {
  color: #adb5bd;
  font-style: italic;
}

.arg-item {
  display: flex;
  align-items: baseline;
  margin-bottom: 4px;
  line-height: 1.5;
}

.arg-item:last-child {
  margin-bottom: 0;
}

.arg-key {
  color: #495057;
  font-weight: 600;
  margin-right: 8px;
  flex-shrink: 0; /* 防止 Key 被压缩 */
}

.arg-value {
  color: #212529;
  word-break: break-all; /* 允许在任意字符间换行 */
}

.arg-value.boolean {
  color: #d63384; /* 布尔值用洋红色 */
}

.arg-value.number {
  color: #0d6efd; /* 数字用蓝色 */
}

.arg-value.long-text {
  display: block;
  background-color: #fff;
  border: 1px solid #dee2e6;
  padding: 4px;
  border-radius: 4px;
  margin-top: 4px;
  white-space: pre-wrap; /* 保留换行符 */
  color: #d9534f; /* 字符串用红色 */
  width: 100%;
}
</style>

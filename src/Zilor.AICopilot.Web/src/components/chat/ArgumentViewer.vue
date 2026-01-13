<script setup lang="ts">
import { computed } from 'vue';

interface Props {
  // 接收原始的 JSON 字符串
  jsonString: string;
}

const props = defineProps<Props>();

// 计算属性：将 JSON 字符串安全解析为对象数组
const parsedArgs = computed(() => {
  try {
    if (!props.jsonString) return [];

    const obj = JSON.parse(props.jsonString);

    // 将对象转换为 { key, value } 的数组形式方便遍历
    return Object.keys(obj).map(key => ({
      key,
      value: obj[key],
      // 简单判断值的类型，用于后续样式区分
      type: typeof obj[key]
    }));
  } catch (e) {
    // 如果解析失败，返回原始字符串作为单一条目
    return [{ key: 'Raw', value: props.jsonString, type: 'string' }];
  }
});

// 辅助函数：格式化特定的值
const formatValue = (val: any) => {
  if (val === null) return 'null';
  if (typeof val === 'boolean') return val ? 'True' : 'False';
  if (typeof val === 'object') return JSON.stringify(val); // 对于嵌套对象，降级显示
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

        <code v-if="item.type === 'string' && item.value.length > 50" class="arg-value long-text">
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

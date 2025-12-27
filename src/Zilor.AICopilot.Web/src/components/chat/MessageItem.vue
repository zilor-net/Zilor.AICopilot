<script setup lang="ts">
import { computed } from 'vue';
import { UserFilled, Service } from '@element-plus/icons-vue';
import { type ChatMessage, MessageRole } from '@/types/protocols.ts';
import { renderMarkdown } from '@/utils/markdown.ts';

// 定义 Props 类型
const props = defineProps<{
  message: ChatMessage
}>();

// 判断是否为用户消息
const isUser = computed(() => props.message.role === MessageRole.User);

// 渲染 Markdown 内容
const htmlContent = computed(() => {
  return renderMarkdown(props.message.content);
});
</script>

<template>
  <div class="message-row" :class="{ 'row-reverse': isUser }">
    <div class="avatar-container">
      <el-avatar
        :size="36"
        :icon="isUser ? UserFilled : Service"
        :class="isUser ? 'avatar-user' : 'avatar-ai'"
      />
    </div>

    <div class="content-container">
      <div class="message-bubble" :class="isUser ? 'bubble-user' : 'bubble-ai'">

        <div v-if="!isUser && message.isStreaming && !message.content" class="typing-indicator">
          <span></span><span></span><span></span>
        </div>

        <div
          v-else
          class="markdown-body"
          v-html="htmlContent"
        ></div>

        <span v-if="message.isStreaming" class="cursor-blink">|</span>
      </div>

      <div v-if="message.widgets && message.widgets.length > 0" class="widgets-area">
        <div class="widget-placeholder">
          [图表组件占位符: {{ message.widgets.length }} 个组件]
        </div>
      </div>

      <div class="timestamp">
        {{ new Date(message.timestamp).toLocaleTimeString() }}
      </div>
    </div>
  </div>
</template>

<style scoped>
.message-row {
  display: flex;
  margin-bottom: 24px;
  gap: 12px;
  align-items: flex-start; /* 顶部对齐 */
}

.row-reverse {
  flex-direction: row-reverse; /* 用户消息反向排列：头像在右 */
}

.avatar-user {
  background-color: var(--brand-color);
}

.avatar-ai {
  background-color: #10a37f; /* ChatGPT 风格绿 */
}

.content-container {
  max-width: 70%; /* 限制气泡最大宽度 */
  display: flex;
  flex-direction: column;
}

.row-reverse .content-container {
  align-items: flex-end; /* 用户消息右对齐 */
}

.message-bubble {
  padding: 12px 16px;
  border-radius: 12px;
  font-size: 15px;
  line-height: 1.6;
  position: relative;
  word-wrap: break-word; /* 强制长单词换行 */
}

.bubble-user {
  background-color: var(--bubble-bg-user);
  color: var(--text-primary);
  border-bottom-right-radius: 2px; /* 调整圆角，使其像气泡 */
}

.bubble-ai {
  background-color: var(--bubble-bg-ai);
  color: var(--text-primary);
  border-bottom-left-radius: 2px;
}

.timestamp {
  font-size: 12px;
  color: #9ca3af;
  margin-top: 4px;
}

/* 光标闪烁动画 */
.cursor-blink {
  font-weight: bold;
  animation: blink 1s step-end infinite;
}

@keyframes blink {
  0%, 100% { opacity: 1; }
  50% { opacity: 0; }
}

/* 简单的打字中 Loading 动画 */
.typing-indicator span {
  display: inline-block;
  width: 6px;
  height: 6px;
  background-color: #b0b0b0;
  border-radius: 50%;
  margin: 0 2px;
  animation: bounce 1.4s infinite ease-in-out both;
}

.typing-indicator span:nth-child(1) { animation-delay: -0.32s; }
.typing-indicator span:nth-child(2) { animation-delay: -0.16s; }

@keyframes bounce {
  0%, 80%, 100% { transform: scale(0); }
  40% { transform: scale(1); }
}

/* 占位符样式 */
.widget-placeholder {
  margin-top: 10px;
  padding: 10px;
  border: 1px dashed #ccc;
  border-radius: 8px;
  background: #fff;
  color: #666;
  font-size: 12px;
}
</style>

<style>
.markdown-body p {
  margin: 0 0 8px 0;
}
.markdown-body p:last-child {
  margin-bottom: 0;
}
.markdown-body pre {
  background-color: #282c34;
  color: #abb2bf;
  padding: 12px;
  border-radius: 6px;
  overflow-x: auto;
}
.markdown-body code {
  font-family: Consolas, Monaco, 'Andale Mono', monospace;
}
</style>

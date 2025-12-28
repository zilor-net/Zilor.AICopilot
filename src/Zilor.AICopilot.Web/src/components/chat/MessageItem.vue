<script setup lang="ts">
import { computed, ref } from 'vue';
import { UserFilled, Service, Opportunity, DataLine } from '@element-plus/icons-vue';
import { type ChatMessage, MessageRole } from '@/types/protocols.ts';
import { renderMarkdown } from '@/utils/markdown.ts';

const props = defineProps<{
  message: ChatMessage
}>();

const isUser = computed(() => props.message.role === MessageRole.User);

// 渲染 Markdown
const analysisHtml = computed(() => renderMarkdown(props.message.analysis?.content || ''));
const finalHtml = computed(() => renderMarkdown(props.message.finalContent || ''));

// 控制折叠面板状态
// 'analysis' 存在时，我们默认展开名为 'analysisPane' 的面板
const activeNames = ref(['analysisPane']);

// 意图置信度颜色
const confidenceColor = computed(() => {
  const score = props.message.intent?.confidence || 0;
  if (score > 0.8) return 'success';
  if (score > 0.5) return 'warning';
  return 'danger';
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

      <div v-if="!isUser && message.intent" class="intent-section">
        <el-collapse>
          <el-collapse-item name="intentPane">
            <template #title>
              <div class="intent-header">
                <el-icon class="header-icon"><Opportunity /></el-icon>
                <span>意图识别: </span>
                <el-tag size="small" effect="plain" class="ml-2">{{ message.intent?.intent }}</el-tag>
                <el-tag size="small" :type="confidenceColor" effect="light" class="ml-2">
                  {{ (message.intent?.confidence * 100).toFixed(0) }}%
                </el-tag>
              </div>
            </template>

            <div class="intent-detail">
              <p v-if="message.intent.query"><strong>提取关键词:</strong> {{
                  message.intent?.query
                }}</p>
              <p v-if="message.intent.reasoning"><strong>推理逻辑:</strong>
                {{ message.intent?.reasoning }}</p>
            </div>
          </el-collapse-item>
        </el-collapse>
      </div>

      <div v-if="!isUser && (message.analysis.content || message.analysis.widgets.length > 0)" class="analysis-section">
        <el-collapse v-model="activeNames">
          <el-collapse-item name="analysisPane">
            <template #title>
              <div class="analysis-header">
                <el-icon class="header-icon"><DataLine /></el-icon>
                <span>深度思考与执行</span>
                <span v-if="message.isStreaming && !message.finalContent" class="typing-dot">...</span>
              </div>
            </template>

            <div class="analysis-body">
              <div class="markdown-body text-gray" v-html="analysisHtml"></div>

              <div v-if="message.analysis.widgets.length > 0" class="widgets-area">
                <div v-for="widget in message.analysis.widgets" :key="widget.id" class="widget-wrapper">
                  <component :is="widget.type" :data="widget.data" />
                </div>
              </div>
            </div>
          </el-collapse-item>
        </el-collapse>
      </div>

      <div
        v-if="message.finalContent || isUser"
        class="message-bubble"
        :class="isUser ? 'bubble-user' : 'bubble-ai'"
      >
        <div class="markdown-body" v-html="finalHtml"></div>
        <span v-if="message.isStreaming && !isUser" class="cursor-blink">|</span>
      </div>

    </div>
  </div>
</template>

<style scoped>
/* 复用之前的 message-row, avatar 等样式，这里只补充新增的 */

.content-container {
  max-width: 85%; /* 稍微调宽一点，方便显示图表 */
  display: flex;
  flex-direction: column;
  gap: 8px; /* 各板块之间的间距 */
}

/* 意图板块样式 */
.intent-section {
  background: #fff;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid #ebeef5;
}
.intent-header {
  display: flex;
  align-items: center;
  font-size: 13px;
  color: #606266;
  padding-left: 8px;
}
.intent-detail {
  padding: 8px 12px;
  font-size: 13px;
  color: #666;
  background-color: #f9fafe;
}
.intent-detail p {
  margin: 4px 0;
}

/* 分析板块样式 */
.analysis-section {
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid #dcdfe6; /* 稍微深一点的边框表示强调 */
  background-color: #fafafa;
}
.analysis-header {
  display: flex;
  align-items: center;
  color: #409eff; /* 蓝色表示正在处理 */
  font-weight: 500;
  padding-left: 8px;
}
/* 覆盖 Element Collapse 默认样式使其更紧凑 */
:deep(.el-collapse-item__header) {
  height: 40px;
  background-color: #f2f6fc;
}
:deep(.el-collapse-item__content) {
  padding: 16px;
  background-color: #fff;
}
.text-gray {
  color: #555;
  font-size: 14px;
}

/* 通用 Helper */
.ml-2 { margin-left: 8px; }
.header-icon { margin-right: 6px; font-size: 16px; }

.typing-dot {
  animation: blink 1.5s infinite;
  margin-left: 4px;
}
</style>

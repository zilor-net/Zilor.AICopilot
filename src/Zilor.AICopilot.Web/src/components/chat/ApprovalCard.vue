<script setup lang="ts">
import { computed, ref } from 'vue';
import type { ApprovalChunk } from '@/types/models';
import ArgumentViewer from './ArgumentViewer.vue';

// 定义 Props
interface Props {
  chunk: ApprovalChunk;
}

const props = defineProps<Props>();

// 定义 Events
// 组件只负责展示和交互，具体的 API 调用逻辑交由父组件或 Store 处理
const emit = defineEmits<{
  (e: 'approve', callId: string): void;
  (e: 'reject', callId: string): void;
}>();

// 本地 loading 状态，防止重复点击
const isProcessing = ref(false);

// 提取核心数据
const request = computed(() => props.chunk.request);
const status = computed(() => props.chunk.status);

// 判断当前是否处于可交互状态
const isPending = computed(() => status.value === 'pending');

const handleApprove = () => {
  if (isProcessing.value) return;
  isProcessing.value = true;
  // 抛出事件，携带 CallId
  emit('approve', request.value.callId);
};

const handleReject = () => {
  if (isProcessing.value) return;
  isProcessing.value = true;
  emit('reject', request.value.callId);
};
</script>

<template>
  <div class="approval-card" :class="status">
    <div class="card-header">
      <div class="header-icon">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
        </svg>
      </div>
      <div class="header-content">
        <h3 class="title">请求执行敏感操作</h3>
        <p class="subtitle">AI 正在请求权限以执行外部工具调用</p>
      </div>
      <div v-if="!isPending" class="status-badge" :class="status">
        {{ status === 'approved' ? '已批准' : '已拒绝' }}
      </div>
    </div>

    <div class="card-body">
      <div class="function-info">
        <span class="label">目标工具:</span>
        <code class="function-name">{{ request.name }}</code>
      </div>

      <div class="arguments-section">
        <span class="label">参数详情:</span>
        <ArgumentViewer :args="request.args" />
      </div>
    </div>

    <div class="card-footer">
      <template v-if="isPending">
        <button
          class="btn btn-reject"
          @click="handleReject"
          :disabled="isProcessing"
        >
          拒绝执行
        </button>
        <button
          class="btn btn-approve"
          @click="handleApprove"
          :disabled="isProcessing"
        >
          <span v-if="isProcessing">处理中...</span>
          <span v-else>批准执行</span>
        </button>
      </template>

      <div v-else class="result-message">
        <span v-if="status === 'approved'" class="text-success">
          操作已授权。
        </span>
        <span v-else class="text-danger">
          操作已被用户拦截。
        </span>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* 卡片容器：默认样式 */
.approval-card {
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: white;
  margin: 12px 0;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
  overflow: hidden;
  transition: all 0.3s ease;
  max-width: 600px;
}

/* 状态修饰符：已拒绝 */
.approval-card.rejected {
  opacity: 0.7;
  border-color: #cbd5e1;
  background: #f8fafc;
}

/* 状态修饰符：已批准 */
.approval-card.approved {
  border-color: #bbf7d0;
  background: #f0fdf4;
}

/* --- Header 区域 --- */
.card-header {
  display: flex;
  align-items: center;
  padding: 12px 16px;
  background: #fff7ed; /* 浅橙色背景警告 */
  border-bottom: 1px solid #ffedd5;
}

.approval-card.approved .card-header {
  background: #dcfce7; /* 浅绿色 */
  border-bottom-color: #bbf7d0;
}

.approval-card.rejected .card-header {
  background: #f1f5f9; /* 浅灰色 */
  border-bottom-color: #e2e8f0;
}

.header-icon {
  width: 24px;
  height: 24px;
  margin-right: 12px;
  color: #ea580c; /* 深橙色图标 */
}

.header-content {
  flex: 1;
}

.title {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: #1e293b;
}

.subtitle {
  margin: 0;
  font-size: 0.8rem;
  color: #64748b;
}

/* --- Body 区域 --- */
.card-body {
  padding: 16px;
}

.function-info {
  display: flex;
  align-items: center;
  margin-bottom: 12px;
}

.label {
  font-size: 0.85rem;
  font-weight: 600;
  color: #64748b;
  margin-right: 8px;
  width: 70px; /* 固定宽度对齐 */
}

.function-name {
  background: #e0e7ff;
  color: #4338ca;
  padding: 2px 6px;
  border-radius: 4px;
  font-family: monospace;
  font-weight: bold;
}

.risk-alert {
  margin-top: 12px;
  padding: 8px;
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #991b1b;
  font-size: 0.85rem;
  border-radius: 4px;
}

/* --- Footer 区域 --- */
.card-footer {
  padding: 12px 16px;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

/* 按钮样式 */
.btn {
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  border: 1px solid transparent;
  transition: all 0.2s;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-reject {
  background: white;
  border-color: #cbd5e1;
  color: #475569;
}

.btn-reject:hover:not(:disabled) {
  background: #f1f5f9;
  color: #ef4444; /* 悬停变红 */
  border-color: #ef4444;
}

.btn-approve {
  background: #2563eb; /* 品牌蓝 */
  color: white;
}

.btn-approve:hover:not(:disabled) {
  background: #1d4ed8;
  box-shadow: 0 2px 4px rgba(37, 99, 235, 0.3);
}

.status-badge {
  font-size: 0.8rem;
  padding: 2px 8px;
  border-radius: 12px;
  font-weight: 600;
}

.status-badge.approved {
  background: #166534;
  color: white;
}

.status-badge.rejected {
  background: #64748b;
  color: white;
}

.result-message {
  font-size: 0.9rem;
  font-weight: 500;
}

.text-success { color: #166534; }
.text-danger { color: #991b1b; }
</style>

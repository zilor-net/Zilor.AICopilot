import {
  type ChatChunk,
  type FunctionApprovalRequest,
  type IntentResult,
  MessageRole,
  type Widget
} from "@/types/protocols.ts";

// ---------------------- 前端数据结构 ----------------------

/**
 * 函数调用信息结构
 * FunctionCallContent + FunctionResultContent 合并
 */
export interface FunctionCall {
  id: string;
  name: string;
  args: string;
  result?: string;
  status: 'calling' | 'completed';
}

/**
 * 扩展消息块-意图识别块
 */
export interface IntentChunk extends ChatChunk {
  intents: IntentResult[]
}

/**
 * 扩展消息块-函数调用块
 */
export interface FunctionCallChunk extends ChatChunk {
  functionCall: FunctionCall;
}

/**
 * 扩展消息块-组件块
 */
export interface WidgetChunk extends ChatChunk {
  widget: Widget;
}

/**
 * 扩展消息块-审批请求片段
 * 用于在消息列表中渲染审批卡片
 */
export interface ApprovalChunk extends ChatChunk {
  // 复用传输层的载体数据
  request: FunctionApprovalRequest;

  // 审批单的当前状态
  // pending: 等待用户操作
  // approved: 用户已批准
  // rejected: 用户已拒绝
  status: 'pending' | 'approved' | 'rejected';
}

/**
 * 前端使用的消息模型
 */
export interface ChatMessage {
  sessionId: string;
  role: MessageRole;
  chunks: ChatChunk[];
  isStreaming: boolean;
  timestamp: number;
}

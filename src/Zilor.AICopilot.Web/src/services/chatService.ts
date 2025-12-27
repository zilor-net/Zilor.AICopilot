import {fetchEventSource} from '@microsoft/fetch-event-source';
import { apiClient } from './apiClient';
import {ChunkType, type StreamChunk, type MessageDto} from '../types/protocols';
import { token, baseUrl } from "@/appsetting";

/**
 * 定义流式回调函数的接口
 * 上层调用者（Store）通过这些回调接收数据
 */
interface StreamCallbacks {
  onText: (text: string) => void;       // 当收到文本块时
  onWidget: (widgetJson: string) => void; // 当收到组件JSON时
  onComplete: () => void;               // 当流结束时
  onError: (err: any) => void;          // 当发生错误时
}

export const chatService = {
  /**
   * 获取会话列表
   */
  async getSessions() {
    return await apiClient.get<any[]>('/session/list');
  },

  /**
   * 创建新会话
   */
  async createSession() {
    return await apiClient.post<any>('/session', { });
  },

  /**
   * 获取指定会话的历史消息
   */
  async getHistoryMessages(sessionId: string) {
    // 假设 apiClient.baseUrl 已经是 '/api'，这里拼接后就是 '/api/aigateway/messages'
    return await apiClient.get<MessageDto[]>(`/messages?sessionId=${sessionId}`);
  },

  /**
   * 发送消息并接收流式响应
   * @param sessionId 会话ID
   * @param message 用户输入的内容
   * @param callbacks 回调函数集合
   */
  async sendMessageStream(sessionId: string, message: string, callbacks: StreamCallbacks) {
    const ctrl = new AbortController();

    try {
      // 使用微软的库发起 SSE 请求
      await fetchEventSource(`${baseUrl}/chat`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          sessionId: sessionId,
          message: message
        }),
        signal: ctrl.signal,

        // 1. 处理连接打开
        async onopen(response) {
          if (response.ok) {
            return; // 连接成功
          } else {
            throw new Error(`Connection failed: ${response.status}`);
          }
        },

        // 2. 处理消息接收 (核心逻辑)
        onmessage(msg) {
          try {
            // 解析后端发来的 ChatChunk JSON
            const chunk: StreamChunk = JSON.parse(msg.data);

            // 根据类型分发处理
            if (chunk.type === ChunkType.Text) {
              callbacks.onText(chunk.content);
            }
            else if (chunk.type === ChunkType.Widget) {
              // Widget 内容通常是转义过的 JSON 字符串，需要二次处理
              callbacks.onWidget(chunk.content);
            }
          } catch (err) {
            console.error('Failed to parse chunk:', err);
          }
        },

        // 3. 处理连接关闭
        onclose() {
          callbacks.onComplete();
        },

        // 4. 处理错误
        onerror(err) {
          callbacks.onError(err);
          throw err; // 抛出错误以中断重试
        },

        // 保持连接，即使页面进入后台
        openWhenHidden: true
      });
    } catch (err) {
      callbacks.onError(err);
    }
  }
};

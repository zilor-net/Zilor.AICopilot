import {defineStore} from 'pinia';
import {computed, ref} from 'vue';
import {chatService} from '@/services/chatService.ts';
import {
  type ChatChunk,
  ChunkType,
  type IntentResult,
  MessageRole,
  type Session, type Widget,
  type FunctionApprovalRequest
} from "@/types/protocols";
import type {
  ChatMessage,
  FunctionCall,
  FunctionCallChunk, IntentChunk,
  WidgetChunk,
  ApprovalChunk
} from "@/types/models.ts";

export const useChatStore = defineStore('chat', () => {
  // ================= 状态 (State) =================

  // 会话列表
  const sessions = ref<Session[]>([]);

  // 当前选中的会话 ID
  const currentSessionId = ref<string | null>(null);

  // 消息记录字典：Key是会话ID，Value是该会话的消息列表
  const messagesMap = ref<Record<string, ChatMessage[]>>({});

  // 正在接收消息的标志
  const isStreaming = ref(false);

  // 是否正在等待用户审批
  const isWaitingForApproval = ref(false);

  // ================= 计算属性 =================

  /**
   * 获取当前会话的所有消息
   */
  const currentMessages = computed(() => {
    if (!currentSessionId.value) return [];
    return messagesMap.value[currentSessionId.value] || [];
  });

  /**
   * 获取当前选中的会话对象
   */
  const currentSession = computed(() => {
    if (!currentSessionId.value) return { title: '当前没有选择会话' } as Session;
    return sessions.value
      .find(session => session.id === currentSessionId.value)
  });

  // ================= 动作 (Actions) =================

  /**
   * 初始化：加载会话列表
   */
  async function init() {
    try {
      sessions.value = await chatService.getSessions();
    } catch (error) {
      console.error('无法加载会话', error);
    }
  }

  /**
   * 创建新会话并选中
   */
  async function createNewSession() {
    const newSession = await chatService.createSession();
    sessions.value.unshift(newSession);
    currentSessionId.value = newSession.id;
    messagesMap.value[newSession.id] = [];
  }

  /**
   * 切换会话
   */
  async function selectSession(id: string) {
    currentSessionId.value = id;
    isWaitingForApproval.value = false;
  }

  /**
   * 发送消息的核心逻辑
   */
  async function sendMessage(input: string) {
    if (!currentSessionId.value || isStreaming.value) return;

    const sessionId = currentSessionId.value;

    // 1. 在 UI 上立即显示用户的消息
    const userMsg: ChatMessage = {
      sessionId,
      role: MessageRole.User,
      chunks : [{
        source: 'User',
        type: ChunkType.Text,
        content: input
      }],
      isStreaming: false,
      timestamp: Date.now()
    };
    addMessage(sessionId, userMsg);

    // 2. 预先创建一个空的 AI 回复消息（占位符）
    const aiMsg: ChatMessage = {
      sessionId,
      role: MessageRole.Assistant,
      chunks: [], // 初始为空，随流动态增加
      isStreaming: true,
      timestamp: Date.now()
    };
    const targetMsg = addMessage(sessionId, aiMsg);

    isStreaming.value = true;

    // 3. 调用 API 服务，开始接收流
    await chatService.sendMessageStream(sessionId, input, {
      onChunkReceived: (chunk: ChatChunk) => {
        switch (chunk.type)
        {
          case ChunkType.Text:
            addTextChunk(targetMsg, chunk);
            break;
          case ChunkType.Intent:
            addIntentChunk(targetMsg, chunk);
            break;
          case ChunkType.FunctionCall:
            addFunctionCallChunk(targetMsg, chunk);
            break;
          case ChunkType.FunctionResult:
            addFunctionResultChunk(targetMsg, chunk);
            break;
          case ChunkType.Widget:
            addWidgetChunk(targetMsg, chunk);
            break;
          // 处理审批请求
          case ChunkType.ApprovalRequest:
            addApprovalRequestChunk(targetMsg, chunk);
            break;
        }
      },

      // 完成时
      onComplete: () => {
        isStreaming.value = false;
        targetMsg.isStreaming = false;
      },

      // 错误时
      onError: (err) => {
        console.error('API 调用错误:', err);
        isStreaming.value = false;
      }
    });
  }

  // ================= 辅助函数 (Internal) =================

  /**
   * 发送消息的核心逻辑
   */
  function addMessage(sid: string, msg: ChatMessage): ChatMessage {
    if (!messagesMap.value[sid]) {
      messagesMap.value[sid] = [];
    }
    const list = messagesMap.value[sid];
    list.push(msg);
    return list[list.length - 1]!;
  }

  /**
   * 添加文本块
   */
  function addTextChunk(msg: ChatMessage, chunk: ChatChunk) {
    const preChunk = msg.chunks[msg.chunks.length - 1];

    if (preChunk === undefined) {
      msg.chunks.push(chunk);
      return;
    }

    if (preChunk.source === chunk.source && preChunk.type === ChunkType.Text) {
      preChunk.content += chunk.content;
    } else {
      msg.chunks.push(chunk);
    }
  }

  /**
   * 添加意图识别块
   */
  function addIntentChunk(msg: ChatMessage, chunk: ChatChunk) {
    const intents = JSON.parse(chunk.content) as IntentResult[];
    const intentChunk = {
      ...chunk,
       intents
    } as IntentChunk;
    msg.chunks.push(intentChunk);
  }

  /**
   * 添加函数调用块
   */
  function addFunctionCallChunk(msg: ChatMessage, chunk: ChatChunk) {
    const functionCall = JSON.parse(chunk.content) as FunctionCall;
    functionCall.status = 'calling';

    const fcChunk = {
      ...chunk,
      functionCall
    } as FunctionCallChunk;
    msg.chunks.push(fcChunk);
  }

  /**
   * 添加函数结果块
   */
  function addFunctionResultChunk(msg: ChatMessage, chunk: ChatChunk) {
    const functionResult = JSON.parse(chunk.content) as FunctionCall;
    const functionCallChunks = msg.chunks
      .filter(c => c.type === ChunkType.FunctionCall) as FunctionCallChunk[];
    const fcChunk = functionCallChunks.find(c => c.functionCall.id === functionResult.id);
    if (fcChunk) {
      fcChunk.functionCall.result = functionResult.result;
      fcChunk.functionCall.status = 'completed';
    }
  }

  /**
   * 添加组件块
   */
  function addWidgetChunk(msg: ChatMessage, chunk: ChatChunk) {
    const widget = JSON.parse(chunk.content) as Widget;
    const widgetChunk = {
      ...chunk,
      widget
    } as WidgetChunk
    msg.chunks.push(widgetChunk);
  }

  /**
   * 处理审批请求数据块
   */
  function addApprovalRequestChunk(msg: ChatMessage, chunk: ChatChunk) {
    // 1. 反序列化后端传递的 Payload
    // 注意：content 字段是 FunctionApprovalRequestContent 的 JSON 字符串
    const request = JSON.parse(chunk.content) as FunctionApprovalRequest;

    // 2. 构造前端使用的 ViewModel
    const approvalChunk: ApprovalChunk = {
      ...chunk,              // 保留 source, type 等基础元数据
      request,
      status: 'pending'      // 初始状态默认为“待处理”
    };

    // 3. 将块追加到当前消息的消息体中
    msg.chunks.push(approvalChunk);

    // 4. 触发全局锁定
    // 这是一个关键的副作用：告知整个应用现在进入“人机协作模式”
    // 输入框组件监听到此状态后，应变为禁用状态
    isWaitingForApproval.value = true;
  }

  // 导出
  return {
    sessions,
    currentSessionId,
    currentSession,
    currentMessages,
    isStreaming,
    isWaitingForApproval,
    init,
    createNewSession,
    selectSession,
    sendMessage
  };
});

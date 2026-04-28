import {defineStore} from 'pinia';
import {computed, ref} from 'vue';
import {chatService} from '@/services/chatService.ts';
import {
  type ChatChunk,
  ChunkType, type FunctionApprovalRequest,
  type IntentResult,
  MessageRole,
  type Session, type Widget
} from "@/types/protocols";
import type {
  ApprovalChunk,
  ChatMessage,
  FunctionCall,
  FunctionCallChunk, IntentChunk,
  WidgetChunk
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
  // 当此值为 true 时，聊天输入框应当被禁用或锁定
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

    isStreaming.value = false;
    isWaitingForApproval.value = false;
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
        processChunk(targetMsg!, chunk);
      },

      // 完成时
      onComplete: () => {
        isStreaming.value = false;
        targetMsg.isStreaming = false;
      },

      // 错误时
      onError: (err) => {
        isStreaming.value = false;
      }
    });
  }

  /**
   * 提交审批
   * @param callId 审批单 ID
   * @param chunk 审批数据块
   */
  async function submitApproval(callId: string, chunk: ApprovalChunk) {
    if (!currentSessionId.value) return;

    const sessionId = currentSessionId.value;

    try {
      // 1. 准备接收新的流
      isStreaming.value = true;

      // 找到要追加的目标消息（即包含审批请求的那条 AI 消息）
      let targetMsg = getLastAssistantMessage(sessionId);
      // 如果找不到（极少见），则创建一条新的
      if (!targetMsg) {
        targetMsg = addMessage(sessionId, {
          sessionId,
          role: MessageRole.Assistant,
          chunks: [],
          isStreaming: true,
          timestamp: Date.now()
        });
      }

      // 2. 调用服务
      const messageText = chunk.status === 'approved' ? "批准" : "拒绝";
      await chatService.sendMessageStream(
        sessionId,
        messageText,
        {
          onChunkReceived: (chunk: ChatChunk) => {
            // 回调逻辑复用了之前的 chunk 处理函数
            // 无论是初始对话还是恢复对话，只要是 ChatChunk，处理方式都是一样的
            processChunk(targetMsg!, chunk);
          },
          onComplete: () => {
            isStreaming.value = false;
            if (targetMsg) targetMsg.isStreaming = false;

            // 流结束意味着本次人机交互闭环完成
            // 解除全局挂起锁，允许用户发送新消息
            isWaitingForApproval.value = false;
          },
          onError: (err) => {
            console.error('审批响应流中断:', err);
            isStreaming.value = false;
            isWaitingForApproval.value = false;
          }
        },
        [callId]
      );

    } catch (error) {
      console.error('提交审批失败:', error);
      isStreaming.value = false;
    }
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
    try {
      // 1. 反序列化后端传递的 Payload
      // 注意：content 字段是 FunctionApprovalRequestContent 的 JSON 字符串
      const requestPayload = JSON.parse(chunk.content) as FunctionApprovalRequest;

      // 2. 构造前端使用的 ViewModel
      const approvalChunk: ApprovalChunk = {
        ...chunk,              // 保留 source, type 等基础元数据
        request: requestPayload,
        status: 'pending'      // 初始状态默认为“待处理”
      };

      // 3. 将块追加到当前消息的消息体中
      // 这样 UI 层的 v-for 循环就能渲染出对应的 ApprovalCard 组件
      msg.chunks.push(approvalChunk);

      // 4. 触发全局锁定
      // 这是一个关键的副作用：告知整个应用现在进入“人机协作模式”
      // 输入框组件监听到此状态后，应变为 Disabled 状态
      isWaitingForApproval.value = true;

      console.log(`收到审批请求: [${requestPayload.name}] ID: ${requestPayload.callId}`);

    } catch (error) {
      console.error('解析审批请求失败:', error, chunk.content);
      // 在生产环境中，这里可能需要生成一个 ErrorChunk 来提示用户
    }
  }

  /**
   * 获取当前正在生成的 AI 消息
   * 用于审批恢复后，将后续内容追加到同一条消息气泡中
   */
  function getLastAssistantMessage(sid: string): ChatMessage | null {
    const list = messagesMap.value[sid];
    if (!list || list.length === 0) return null;

    const lastMsg = list[list.length - 1]!;
    if (lastMsg.role === MessageRole.Assistant) {
      return lastMsg;
    }
    return null;
  }

  /**
   * 将 processChunk 提取为独立函数
   * 原本在 sendMessage 中的 switch case 逻辑，现在被两个 Action 复用
   */
  function processChunk(msg: ChatMessage, chunk: ChatChunk) {
    switch (chunk.type) {
      case ChunkType.Text:
        addTextChunk(msg, chunk);
        break;
      case ChunkType.Intent:
        addIntentChunk(msg, chunk);
        break;
      case ChunkType.FunctionCall:
        addFunctionCallChunk(msg, chunk);
        break;
      case ChunkType.FunctionResult:
        addFunctionResultChunk(msg, chunk);
        break;
      case ChunkType.Widget:
        addWidgetChunk(msg, chunk);
        break;
      case ChunkType.ApprovalRequest:
        addApprovalRequestChunk(msg, chunk);
        break;
    }
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
    sendMessage,
    submitApproval
  };
});

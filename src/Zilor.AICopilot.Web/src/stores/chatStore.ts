import {defineStore} from 'pinia';
import {computed, ref} from 'vue';
import {chatService} from '../services/chatService';
import {
  type ChatMessage, type IWidgetData, MessageRole, type Session, type IntentResult,
  type StreamChunk
} from '../types/protocols';

export const useChatStore = defineStore('chat', () => {
  // ================= 状态 (State) =================

  // 会话列表
  const sessions = ref<Session[]>([]);

  // 当前选中的会话 ID
  const currentSessionId = ref<string | null>(null);

  // 消息记录字典：Key是会话ID，Value是该会话的消息列表
  // 这样设计可以在切换会话时瞬间加载，不需要重新请求
  const messagesMap = ref<Record<string, ChatMessage[]>>({});

  // 正在接收消息的标志（用于 UI 显示 Loading）
  const isStreaming = ref(false);

  // ================= 计算属性 (Getters) =================

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
        .find(session => session.id === currentSessionId.value) || { title: '未找到当前会话' } as Session;
  });

  // ================= 动作 (Actions) =================

  /**
   * 初始化：加载会话列表
   */
  async function init() {
    try {
      sessions.value = await chatService.getSessions();
    } catch (error) {
      console.error('Failed to load sessions', error);
    }
  }

  /**
   * 创建新会话并选中
   */
  async function createNewSession() {
    const newSession = await chatService.createSession();

    sessions.value.unshift(newSession); // 加到列表开头
    currentSessionId.value = newSession.id;
    messagesMap.value[newSession.id] = []; // 初始化消息列表
  }

  /**
   * 加载特定会话的历史消息
   */
  async function loadSessionMessages(sessionId: string) {
    // 1. [新增] 缓存检查
    // 如果内存中已经有该会话的消息，且消息数量大于0，直接返回，不再请求 API
    if (messagesMap.value[sessionId] && messagesMap.value[sessionId].length > 0) {
      return;
    }

    try {
      // 2. 调用 API 获取数据
      const history = await chatService.getHistoryMessages(sessionId);

      // 3. 将 DTO 转换为前端内部模型
      messagesMap.value[sessionId] = history.map(dto => ({
        id: dto.id.toString(),
        sessionId: sessionId,
        role: dto.type === 'User' ? MessageRole.User : MessageRole.Assistant,
        finalContent: dto.content,
        intent: undefined,
        analysis: { content: '', widgets: [] },
        widgets: [], // 暂时为空
        isStreaming: false,
        timestamp: new Date(dto.createdAt).getTime()
      }));

    } catch (error) {
      console.error(`Failed to load history for session ${sessionId}`, error);
    }
  }

  /**
   * 切换会话
   */
  async function selectSession(id: string) {
    currentSessionId.value = id;
    // 切换时，立即加载历史记录
    await loadSessionMessages(id);
  }

  /**
   * 发送消息的核心逻辑
   */
  async function sendMessage(content: string) {
    if (!currentSessionId.value || isStreaming.value) return;

    const sessionId = currentSessionId.value;

    // 1. 在 UI 上立即显示用户的消息
    const userMsg: ChatMessage = {
      id: Date.now().toString(), // 临时ID
      sessionId,
      role: MessageRole.User,
      finalContent: content, // 用户发的内容算作 finalContent
      analysis: { content: '', widgets: [] },
      isStreaming: false,
      timestamp: Date.now()
    };
    addMessage(sessionId, userMsg);

    // 2. 预先创建一个空的 AI 回复消息（占位符）
    const aiMsgId = (Date.now() + 1).toString();
    const aiMsg: ChatMessage = {
      id: aiMsgId,
      sessionId,
      role: MessageRole.Assistant,
      intent: undefined,                  // 初始无意图
      analysis: { content: '', widgets: [] }, // 初始无分析
      finalContent: '',                   // 初始无回复
      isStreaming: true, // 标记为正在输入
      timestamp: Date.now()
    };
    addMessage(sessionId, aiMsg);

    isStreaming.value = true;

    // 3. 调用 API 服务，开始接收流
    await chatService.sendMessageStream(sessionId, content, {

      onChunkReceived: (chunk: StreamChunk) => {
        const targetMsg = findMessage(sessionId, aiMsgId);
        if (!targetMsg) return;
        // 1. 意图识别 (IntentRoutingExecutor)
        if (chunk.source === 'IntentRoutingExecutor') {
          // 意图数据通常是 JSON，累积起来
          targetMsg.intent = JSON.parse(chunk.content);
        }

        // 2. 数据分析 (DataAnalysisExecutor)
        else if (chunk.source === 'DataAnalysisExecutor') {
          if (chunk.type === 'Text') {
            targetMsg.analysis.content += chunk.content;
          } else if (chunk.type === 'Widget') {
            try {
              const widgetData = JSON.parse(chunk.content);
              targetMsg.analysis.widgets.push({
                id: `w-${Date.now()}-${Math.random()}`,
                type: widgetData.widget_type,
                title: widgetData.title,
                data: widgetData
              });
            } catch (e) { console.error('Widget parse error', e); }
          }
        }

        // 3. 最终回复 (FinalProcessExecutor 或其他)
        else {
          // 默认为最终回复
          if (chunk.type === 'Text') {
            targetMsg.finalContent += chunk.content;
          }
          // Final 阶段通常没有 Widget，如果有也可以处理
        }
      },

      // 完成时
      onComplete: () => {
        isStreaming.value = false;
        const targetMsg = findMessage(sessionId, aiMsgId);
        if (targetMsg) {
          targetMsg.isStreaming = false;
        }
      },

      // 错误时
      onError: (err) => {
        isStreaming.value = false;
        const targetMsg = findMessage(sessionId, aiMsgId);
        if (targetMsg) {
          targetMsg.isStreaming = false;
          targetMsg.finalContent += `\n[系统错误: ${err.message}]`;
        }
      }
    });
  }

  // ================= 辅助函数 =================

  function addMessage(sid: string, msg: ChatMessage) {
    if (!messagesMap.value[sid]) {
      messagesMap.value[sid] = [];
    }
    messagesMap.value[sid].push(msg);
  }

  function findMessage(sid: string, msgId: string) {
    return messagesMap.value[sid]?.find(m => m.id === msgId);
  }

  // 导出需要在组件中使用的内容
  return {
    sessions,
    currentSessionId,
    currentSession,
    currentMessages,
    isStreaming,
    init,
    createNewSession,
    selectSession,
    sendMessage
  };
});

import {defineStore} from 'pinia';
import {computed, ref} from 'vue';
import {chatService} from '../services/chatService';
import {type ChatMessage, type IWidgetData, MessageRole, type Session} from '../types/protocols';

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
        content: dto.content,
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
      content: content,
      widgets: [],
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
      content: '', // 初始为空，等待流式填充
      widgets: [],
      isStreaming: true, // 标记为正在输入
      timestamp: Date.now()
    };
    addMessage(sessionId, aiMsg);

    isStreaming.value = true;

    // 3. 调用 API 服务，开始接收流
    await chatService.sendMessageStream(sessionId, content, {

      // 收到文本块：追加到当前 AI 消息的 content 中
      onText: (text) => {
        const targetMsg = findMessage(sessionId, aiMsgId);
        if (targetMsg) {
          targetMsg.content += text;
        }
      },

      // 收到组件块：解析 JSON 并加入 widgets 列表
      onWidget: (widgetJson) => {
        const targetMsg = findMessage(sessionId, aiMsgId);
        if (targetMsg) {
          try {
            // 后端发来的 widgetJson 包含了 title, data, type 等信息
            const widgetData = JSON.parse(widgetJson);

            // 为组件生成唯一ID
            const widgetItem: IWidgetData = {
              id: `w-${Date.now()}`,
              type: widgetData.type || widgetData.WidgetType, // 兼容大小写
              title: widgetData.title || widgetData.Title,
              data: widgetData
            };

            targetMsg.widgets.push(widgetItem);
          } catch (e) {
            console.error('Widget parsing failed', e);
          }
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
          targetMsg.content += `\n[系统错误: ${err.message}]`;
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

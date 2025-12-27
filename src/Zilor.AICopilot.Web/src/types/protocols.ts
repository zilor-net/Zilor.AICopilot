/**
 * 对应后端的 ChunkType 枚举
 * 决定了消息流中的数据块是纯文本还是可视化组件
 */
export enum ChunkType {
  Error = 'Error',
  Text = 'Text',
  Widget = 'Widget',
  FunctionResult = 'FunctionResult',
  FunctionCall = 'FunctionCall'
}

/**
 * 对应后端的 ChatChunk 类
 * 这是流式响应中每一次传输的最小单元
 */
export interface StreamChunk {
  executorId: string; // 执行器ID，用于追踪是谁生成的
  type: ChunkType;    // 数据类型
  content: string;    // 内容载体（文本或JSON字符串）
}

/**
 * 对应后端的 Session 实体
 * 简化的会话信息
 */
export interface Session {
  id: string;
  title: string;
}

/**
 * 消息发送者枚举
 */
export enum MessageRole {
  User = 'User',
  Assistant = 'Assistant'
}

/**
 * 对应后端历史消息 API 返回的数据结构
 * JSON: { id: 1, content: "...", type: "User", createdAt: "..." }
 */
export interface MessageDto {
  id: number;
  content: string;
  type: MessageRole;
  createdAt: string;
}

/**
 * 前端使用的消息模型
 * 注意：这不是后端的实体，而是为了前端渲染优化的结构
 */
export interface ChatMessage {
  id: string;
  sessionId: string;
  role: MessageRole;
  content: string;         // 累积的文本内容
  widgets: IWidgetData[];  // 该消息包含的可视化组件列表
  isStreaming: boolean;    // 是否正在接收中（用于显示光标闪烁效果）
  timestamp: number;
}

// ---------------------- 可视化组件相关定义 ----------------------

/**
 * 基础组件接口
 */
export interface IWidgetData {
  id: string;      // 组件唯一标识
  type: string;    // 组件类型：'Chart', 'StatsCard', 'DataTable'
  title?: string;  // 组件标题
  data: any;       // 具体的数据载体，根据类型不同而不同
}

/**
 * 对应后端的 ChartWidget
 */
export interface ChartWidgetData extends IWidgetData {
  type: 'Chart';
  data: {
    chartType: 'Bar' | 'Line' | 'Pie';
    xAxis: string[];
    series: Array<{
      name: string;
      data: number[];
    }>;
  };
}

/**
 * 对应后端的 StatsCardWidget
 */
export interface StatsWidgetData extends IWidgetData {
  type: 'StatsCard';
  data: {
    label: string;
    value: string;
    trend?: 'Up' | 'Down' | 'Neutral';
    changeRate?: string;
  };
}

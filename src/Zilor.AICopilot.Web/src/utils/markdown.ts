import MarkdownIt from 'markdown-it';

// 初始化实例
const md = new MarkdownIt({
  html: false,        // 禁用 HTML 标签以防止 XSS 攻击
  linkify: true,      // 自动识别 URL 为链接
  breaks: true,       // 转换换行符为 <br>
  typographer: true   // 启用一些语言学的替换和引号美化
});

// 可以在这里扩展插件，例如代码高亮（后续阶段可加）

/**
 * 将 Markdown 文本渲染为 HTML 字符串
 */
export function renderMarkdown(text: string): string {
  if (!text) return '';
  return md.render(text);
}

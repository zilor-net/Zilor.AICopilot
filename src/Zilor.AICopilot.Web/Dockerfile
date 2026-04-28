# ===============================
# Stage 1: 构建阶段 (Build Environment)
# ===============================
FROM node:20-alpine AS build

WORKDIR /app

# 1. 优先复制依赖定义文件，利用 Docker 缓存层加速构建
COPY package.json package-lock.json ./

# 2. 安装所有依赖 (包括 devDependencies，因为构建工具如 vite/vue-tsc 需要它们)
# 如果使用 npm
RUN npm ci
# 如果使用 pnpm，请取消注释下面两行并注释上面一行

# 3. 复制项目源代码
COPY . .

# 4. 执行构建 (生成 dist 目录)
# 注意：这里不会读取 aicopilot_httpapi_http 环境变量，因为代理是在运行时由 Nginx 处理的
RUN npm run build

# ===============================
# Stage 2: 运行阶段 (Production Runtime)
# ===============================
FROM nginx:alpine

# 1. 从构建阶段复制打包好的静态文件到 Nginx 默认目录
COPY --from=build /app/dist /usr/share/nginx/html

# 2. 复制 Nginx 配置模板
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

# 3. 暴露端口（匹配docker-compose）
EXPOSE 8000

# 4. 启动 Nginx (默认 Entrypoint 会处理模板替换)
CMD ["nginx", "-g", "daemon off;"]
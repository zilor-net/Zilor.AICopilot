<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, nextTick } from 'vue';
import * as echarts from 'echarts';
import type { ChartWidget } from "@/types/protocols.ts";

const props = defineProps<{
  widget: ChartWidget
}>();

// DOM 引用：用于挂载 Canvas
const chartRef = ref<HTMLElement | null>(null);
// ECharts 实例引用
let chartInstance: echarts.ECharts | null = null;
// ResizeObserver 实例
let resizeObserver: ResizeObserver | null = null;

// ================== 核心逻辑：数据转换 (Adapter) ==================

/**
 * 将后端 DTO 转换为 ECharts Option
 * 这是连接业务数据与可视化库的桥梁
 */
const getChartOption = () => {
  const chartType = props.widget.data.category;
  const { x, y } = props.widget.data.encoding;
  const title = props.widget.title;
  const source = props.widget.data.dataset.source;
  const dimensions = props.widget.data.dataset.dimensions;

  // 1. 通用基础配置
  const baseOption: any = {
    title: {
      text: title,
      left: 'center',
      textStyle: { fontSize: 14, color: '#333' }
    },
    tooltip: {
      trigger: chartType === 'Pie' ? 'item' : 'axis',
      confine: true
    },
    legend: {
      bottom: 0,
      type: 'scroll'
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '15%',
      containLabel: true
    },
    // 使用 dataset 管理数据，ECharts 会自动处理维度映射
    dataset: {
      dimensions: dimensions,
      source: source
    }
  };

  // 2. 根据图表类型构建配置
  const typeLower = chartType.toLowerCase();

  if (chartType === 'Pie') {
    // ---- 饼图逻辑 ----
    return {
      ...baseOption,
      series: [
        {
          type: 'pie',
          radius: ['40%', '70%'], // 环形图设计，更现代
          itemStyle: {
            borderRadius: 6,
            borderColor: '#fff',
            borderWidth: 2
          },
          label: {
            show: true,
            formatter: '{b}: {d}%' // 显示名称和百分比
          },
          // 饼图映射：value 取 y 数组的第一个字段，name 取 x 字段
          encode: {
            itemName: x,
            value: y[0]
          }
        }
      ]
    };
  } else {
    // ---- 柱状图 / 折线图逻辑 ----
    return {
      ...baseOption,
      xAxis: {
        type: 'category',
        // 自动截断过长的标签
        axisLabel: {
          interval: 0,
          width: 80,
          overflow: 'truncate'
        }
      },
      yAxis: { type: 'value' },
      // 针对 y 数组中的每个字段生成一个系列 (Series)
      series: y.map(yKey => ({
        type: typeLower,
        name: yKey, // 图例名称默认使用字段名
        smooth: typeLower === 'line', // 折线图开启平滑
        barMaxWidth: 40, // 柱状图限制最大宽度
        encode: {
          x: x,
          y: yKey
        }
      }))
    };
  }
};

// ================== 生命周期管理 ==================

/**
 * 初始化图表
 */
const initChart = () => {
  if (!chartRef.value) return;

  // 初始化实例
  chartInstance = echarts.init(chartRef.value, null, { renderer: 'canvas' });

  // 设置数据
  try {
    const option = getChartOption();
    chartInstance.setOption(option);
  } catch (e) {
    console.error('ECharts Option Error:', e);
  }
};

/**
 * 处理窗口大小变化
 */
const setupResizeObserver = () => {
  if (!chartRef.value) return;

  resizeObserver = new ResizeObserver(() => {
    chartInstance?.resize();
  });
  resizeObserver.observe(chartRef.value);
};

onMounted(async () => {
  await nextTick();
  initChart();
  setupResizeObserver();
});

onUnmounted(() => {
  resizeObserver?.disconnect();
  chartInstance?.dispose();
});

watch(() => props.widget, () => {
  if (chartInstance) {
    chartInstance.setOption(getChartOption(), true);
  }
}, { deep: true });

</script>

<template>
  <div class="chart-container">
    <div ref="chartRef" class="echarts-dom"></div>
  </div>
</template>

<style scoped>
.chart-container {
  width: 100%;
  max-width: 100%;
  background: #fff;
  border-radius: 8px;
  border: 1px solid #e4e7ed;
  padding: 16px;
  margin-top: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}

.echarts-dom {
  width: 100%;
  height: 350px;
}
</style>

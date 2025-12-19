using Zilor.AICopilot.Core.DataAnalysis.Aggregates.BusinessDatabase;

namespace Zilor.AICopilot.MigrationWorkApp.SeedData;

public static class DataAnalysisData
{
    public static IEnumerable<BusinessDatabase> GetDatabases()
    {
        // 模拟一个 ERP 数据库连接
        // 注意：在实际开发环境中，这里应该指向一个真实存在的测试库
        // 本示例假设本地有一个名为 'erp_demo' 的 PostgreSQL 数据库
        var erpDb = new BusinessDatabase(
            "ERP_Core",
            "负责管理产品主数据、客户关系与销售业务。包含：SKU基础信息、价格表、客户档案、销售订单（订单号/金额/状态）以及账面库存总量。当用户询问“卖了多少”、“谁买的”、“订单金额”、“还有多少库存（总量）”时，请选择此库。",
            "Host=localhost;Port=5432;Database=erp_demo;Username=postgres;Password=123456",
            DbProviderType.PostgreSql
        );  

        // 模拟一个 WMS 数据库连接
        var wmsDb = new BusinessDatabase(
            "WMS_Warehouse",
            "负责管理仓库内的具体作业与实物流动。包含：具体的货架/货位管理、拣货打包记录、包裹重量、快递运单号、出入库的详细操作流水。当用户询问“货物在哪个货架”、“包裹发走了没”、“快递单号是多少”、“何时入库的”时，请选择此库。",
            "Host=localhost;Port=5432;Database=wms_demo;Username=postgres;Password=123456",
            DbProviderType.PostgreSql
        );
        return [erpDb, wmsDb];
    }
}
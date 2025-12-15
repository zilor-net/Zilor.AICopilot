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
            "ERP核心数据库，包含产品信息、销售订单和客户资料。查询销售数据、库存状态时请使用此库。",
            "Host=localhost;Port=5432;Database=erp_demo;Username=postgres;Password=123456",
            DbProviderType.PostgreSql
        );

        // 模拟一个 WMS 数据库
        var wmsDb = new BusinessDatabase(
            "WMS_Warehouse",
            "WMS数据库，包含货位管理、出入库流水日志。查询物流、发货详情时请使用此库。",
            "Host=localhost;Port=5432;Database=wms_demo;Username=postgres;Password=123456",
            DbProviderType.PostgreSql
        );

        return [erpDb, wmsDb];
    }
}
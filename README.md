# 齐套分析缺料表

## 功能

在齐套分析 WebPart `MFG.MO.StartAnalysisUI` 主界面工具栏增加【缺料表】按钮。按钮打开独立标准 U9 WebPart：

`U9Custom.ManufactureSimulateShortageTable.Independent`

独立页面提供单据编码、生产订单、备料料号、匹配类型、供应单号和供应日期查询，支持标准分页、横向滚动、列设置和 Excel 导出。

## 业务规则

- 数据来源为当前组织、当前齐套分析单据的 `MO_SimuDemandPick` 明细。
- 仅展示 `ScarceQty > 0` 的缺料明细，按生产订单计划开工日期正序。
- 采购订单行状态为 `0、1、2`，使用计划行 `DeficiencyQtyTU` 作为可匹配数量。
- 采购订单先于采购收货匹配；一条缺料允许依次匹配多条供应行。
- 采购收货行条件为 `Status <> 5` 且 `SplitFlag <> 1`。
- 采购订单供应日期取采购订单行交期；采购收货供应日期取收货单据日期。
- 采购订单、采购收货和生产订单候选数据均限定当前组织。
- 采购收货匹配后仍有缺料时，匹配生产订单 `ProductQty - TotalRcvQty`，并过滤 `DocState <> 3`、`IsCancel <> true`、`IsHoldRelease <> true`。
- 生产订单按 `CompleteDate` 正序匹配；生产订单匹配类型固定为“生产订单”，供应单号显示生产订单号，行号为空，供应日期取 `CompleteDate`。
- 结果列包含业务员：采购订单取 `PurOper` 名称，采购收货留空，生产订单取 `BusinessPerson` 名称；供应单号与行号分列显示。
- 每条匹配结果显示匹配数量和匹配后的剩余缺料。

## 部署文件

- `U9Custom.UI.ManufactureSimulateShortageTable.dll`：主界面按钮插件，部署到 `Portal\UILib`。
- `U9Custom.UI.ManufactureSimulateShortageTable.Independent.dll`：独立缺料表 WebPart，部署到 `Portal\UILib`。
- `U9Custom.UI.OutsourceShortageStatistics.Independent.dll`：委外欠料统计 WebPart，部署到 `Portal\UILib`。
- `WebPartExtend_ManufactureSimulateShortageTable.config`：主界面扩展注册片段。当前 U9CE Portal 会自动扫描 `WebPartExtend_*.config`，无需合并到标准 `WebPartExtend.config`。
- `Install_WebPartExtend.ps1`：清理旧版本合并注册，避免重复加载。
- `Install_ShortageTable.sql`：安装独立 WebPart 的 U9 UI 元数据和菜单。
- `Install_OutsourceShortageStatistics.sql`：安装委外欠料统计 WebPart 的 U9 UI 元数据和菜单。

## 部署顺序

1. 备份当前数据库和 `Portal\WebPartExtend.config`。
2. 执行 `Install_ShortageTable.sql`。
3. 复制两个 DLL 到 `Portal\UILib`。
4. 确认独立扩展配置位于 `Portal` 根目录。
5. 重启 `U9 AppPool CLR4` 和 `DefaultAppPool`，重新登录验证页面。

## 日志

主插件日志：`C:\temp\ManufactureSimulateShortageTable.log`。

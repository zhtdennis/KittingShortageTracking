# 齐套分析缺料表正式生产部署包

版本：2026-09-04

本包包含以下三个独立功能：

1. 独立缺料表：`U9Custom.ManufactureSimulateShortageTable.Independent`
2. 独立委外欠料统计：`U9Custom.OutsourceShortageStatistics.Independent`
3. 委外欠料写回 BP：`U9Custom.OutsourceShortageWritebackBP.WriteToMO`

## 目录说明

- `01_Portal\UILib`：三个 UI DLL，复制到 `Portal\UILib`
- `01_Portal\ApplicationServer\Libs`：BP 服务端 DLL、Agent、Deploy、`.ubfsvc`，复制到 `Portal\ApplicationServer\Libs`
- `01_Portal\ApplicationLib`：BP 的 Agent、Deploy，复制到 `Portal\ApplicationLib`
- `01_Portal\Root`：`WebPartExtend_ManufactureSimulateShortageTable.config`，复制到 `Portal` 根目录
- `02_Database\Install_ShortageTable.sql`：独立缺料表 UI 元数据
- `02_Database\Install_OutsourceShortageStatistics.sql`：独立委外欠料统计 UI 元数据
- `02_Database\Install_WriteToMO_BP_Metadata.sql`：委外欠料写回 BP 元数据
- `03_Scripts\Install_WebPartExtend.ps1`：清理旧合并注册的辅助脚本，按需使用
- `03_Scripts\Deploy_Portal.ps1`：参数化 Portal 文件部署脚本，仅自动备份和复制文件

## 推荐部署顺序

1. 备份生产数据库、`Portal\WebPartExtend.config`、同名 DLL 和配置文件。
2. 在目标 U9 业务数据库执行三个 SQL 脚本，建议顺序为：
   - `Install_ShortageTable.sql`
   - `Install_OutsourceShortageStatistics.sql`
   - `Install_WriteToMO_BP_Metadata.sql`
3. 手工停止 U9 应用池及相关服务，确保 Portal DLL 未被占用。
4. 执行 Portal 部署脚本，脚本仅备份并复制文件，不操作 WAS 或应用池。
5. 确认配置片段位于 `Portal` 根目录。标准 `WebPartExtend.config` 使用 `configSource` 时，不要直接覆盖标准文件。
6. 手工启动 U9 应用池，并按现场要求重启 U9 应用服务和 Job 服务。
7. 用户重新登录后验证三个入口。

## 使用 Portal 部署脚本

在管理员 PowerShell 中执行：

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\03_Scripts\Deploy_Portal.ps1 -PortalRoot 'E:\yonyou\U9CE\Portal'
```

执行前请确认目标 Portal 的应用池已由现场人员手工停止；执行完成后再手工启动应用池。

## 验证范围

- 齐套分析页面能显示【缺料表】按钮，并能打开独立缺料表。
- 独立缺料表能查询、滚动、列设置和输出 Excel。
- 独立委外欠料统计能查询当前组织数据。
- 写回 BP 调用前先确认目标生产订单和欠料结果，生产环境首次验证应使用测试订单。

## 回滚

停止应用池后，用部署前备份恢复同名文件；数据库元数据按现场数据库备份或经审核的回滚脚本处理。恢复文件后重新启动应用池并重新登录。

本包不保存数据库账号密码，不自动执行 SQL，不自动修改业务数据。

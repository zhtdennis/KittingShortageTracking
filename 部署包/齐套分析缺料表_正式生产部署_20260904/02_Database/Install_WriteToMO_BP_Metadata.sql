/* Register the custom BP in the Manufacturing application used by the page. */
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.UBF_Assemble_AppComponents
    WHERE Application = 3025
      AND Component = N'U9Custom.OutsourceShortageWritebackBP.dll'
      AND ComponentType = N'BP'
)
BEGIN
    INSERT INTO dbo.UBF_Assemble_AppComponents
    (
        ID, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, SysVersion,
        Component, ComponentID, ComponentType, Application
    )
    SELECT
        ISNULL(MAX(ID), 1000000000000000) + 1,
        GETDATE(), N'CODEX', GETDATE(), N'CODEX', 0,
        N'U9Custom.OutsourceShortageWritebackBP.dll',
        N'7A5A5C4C-1A1A-4B3C-9E5D-7E7A4A75F9C2',
        N'BP', 3025
    FROM dbo.UBF_Assemble_AppComponents;
END;

/* ComponentID in UBF_Assemble_AppComponents points to this primary metadata
   component record. */
IF NOT EXISTS
(
    SELECT 1 FROM dbo.UBF_MD_Component
    WHERE ID = '7A5A5C4C-1A1A-4B3C-9E5D-7E7A4A75F9C2'
)
BEGIN
    INSERT INTO dbo.UBF_MD_Component
    (
        Local_ID, CreateTime, CreateOp, ModifyTime, ModifyOp, SysVersion,
        Name, AssemblyName, Version, ID, Kind, MD_Module_ID
    )
    SELECT
        ISNULL(MAX(Local_ID), 1000000000000000) + 1,
        GETDATE(), N'CODEX', GETDATE(), N'CODEX', 0,
        N'OutsourceShortageWritebackBP',
        N'U9Custom.OutsourceShortageWritebackBP', N'',
        '7A5A5C4C-1A1A-4B3C-9E5D-7E7A4A75F9C2', N'BP',
        '7A5A5C4C-1A1A-4B3C-9E5D-7E7A4A75F9C2'
    FROM dbo.UBF_MD_Component;
END;

/* BPMap is generated from the BP class metadata. The component rows alone
   are insufficient for the service locator to expose IWriteToMO. */
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.UBF_MD_Class
    WHERE FullName = N'U9Custom.OutsourceShortageWritebackBP.WriteToMO'
      AND ClassType = 7
)
BEGIN
    INSERT INTO dbo.UBF_MD_Class
    (
        Local_ID, CreateTime, CreateOp, ModifyTime, ModifyOp, SysVersion,
        Name, FullName, ID, ClassType, MD_Component_ID
    )
    SELECT
        ISNULL(MAX(Local_ID), 1000000000000000) + 1,
        GETDATE(), N'CODEX', GETDATE(), N'CODEX', 0,
        N'WriteToMO', N'U9Custom.OutsourceShortageWritebackBP.WriteToMO',
        'F3C8D9E0-4A7B-4F21-9C62-8D0E5A7B1C34', 7,
        '7A5A5C4C-1A1A-4B3C-9E5D-7E7A4A75F9C2'
    FROM dbo.UBF_MD_Class;
END;

/* Normalize the generated BP class fields expected by the metadata loader. */
UPDATE dbo.UBF_MD_Class
SET MD_ParentClass_ID = '00000000-0000-0000-0000-000000000000',
    Local_Component_ID = (SELECT Local_ID FROM dbo.UBF_MD_Component
                          WHERE ID = '7A5A5C4C-1A1A-4B3C-9E5D-7E7A4A75F9C2'),
    Discriminator = N'UFIDA.UBF.MD.Business.Operation',
    IsAuthority = 1,
    IsConcrete = 0,
    IsAbstract = 0,
    ReturnTypeID = '7D3E6B14-2A4C-4F8D-9B10-6E5C2A1F8D33'
WHERE FullName = N'U9Custom.OutsourceShortageWritebackBP.WriteToMO'
  AND ClassType = 7;

IF NOT EXISTS
(
    SELECT 1 FROM dbo.UBF_MD_Component
    WHERE MD_Module_ID = '7A5A5C4C-1A1A-4B3C-9E5D-7E7A4A75F9C2'
      AND Name = N'WriteToMOBP' AND Kind = N'BP'
)
BEGIN
    INSERT INTO dbo.UBF_MD_Component
    (Local_ID, CreateTime, CreateOp, ModifyTime, ModifyOp, SysVersion,
     Name, AssemblyName, Version, ID, Kind, MD_Module_ID)
    SELECT ISNULL(MAX(Local_ID), 1000000000000000) + 1,
           GETDATE(), N'CODEX', GETDATE(), N'CODEX', 0,
           N'WriteToMOBP', N'U9Custom.OutsourceShortageWritebackBP', N'',
           'B8D2E3A4-1C6F-4D28-9D3A-0F5D9B77C1E4', N'BP',
           '7A5A5C4C-1A1A-4B3C-9E5D-7E7A4A75F9C2'
    FROM dbo.UBF_MD_Component;
END;

IF NOT EXISTS
(
    SELECT 1 FROM dbo.UBF_MD_Component
    WHERE MD_Module_ID = '7A5A5C4C-1A1A-4B3C-9E5D-7E7A4A75F9C2'
      AND Name = N'IWriteToMO' AND Kind = N'BP'
)
BEGIN
    INSERT INTO dbo.UBF_MD_Component
    (Local_ID, CreateTime, CreateOp, ModifyTime, ModifyOp, SysVersion,
     Name, AssemblyName, Version, ID, Kind, MD_Module_ID)
    SELECT ISNULL(MAX(Local_ID), 1000000000000000) + 1,
           GETDATE(), N'CODEX', GETDATE(), N'CODEX', 0,
           N'IWriteToMO', N'U9Custom.OutsourceShortageWritebackBP', N'',
           'E2C3A4B5-2D70-4E39-AE4B-1A6E8C88D2F5', N'BP',
           '7A5A5C4C-1A1A-4B3C-9E5D-7E7A4A75F9C2'
    FROM dbo.UBF_MD_Component;
END;

DELETE FROM dbo.UBF_Assemble_AppComponents
WHERE Application = 3025
  AND Component = N'U9Custom.OutsourceShortageWritebackBP.Deploy.dll'
  AND ComponentType = N'BP';

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.UBF_MD_Component
    WHERE MD_Module_ID = '7A5A5C4C-1A1A-4B3C-9E5D-7E7A4A75F9C2'
      AND Name = N'WriteToMO'
      AND Kind = N'BP'
)
BEGIN
    INSERT INTO dbo.UBF_MD_Component
    (
        Local_ID, CreateTime, CreateOp, ModifyTime, ModifyOp, SysVersion,
        Name, AssemblyName, Version, ID, Kind, MD_Module_ID
    )
    SELECT
        ISNULL(MAX(Local_ID), 1000000000000000) + 1,
        GETDATE(), N'CODEX', GETDATE(), N'CODEX', 0,
        N'WriteToMO', N'U9Custom.OutsourceShortageWritebackBP', N'',
        '9DB5CD8E-0D70-4CC9-A104-1A861C852692', N'BP',
        '7A5A5C4C-1A1A-4B3C-9E5D-7E7A4A75F9C2'
    FROM dbo.UBF_MD_Component;
END;

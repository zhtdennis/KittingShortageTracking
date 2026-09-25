SET NOCOUNT ON;

DECLARE @SNIndex bigint;
DECLARE @MenuURI nvarchar(510) = N'U9Custom.ManufactureSimulateShortageTable.Independent';
DECLARE @MenuGUID nvarchar(100) = N'A8AE2A44-CCD2-4FD3-ACBF-493FF1772A5A';
DECLARE @MenuName nvarchar(100) = N'齐套分析缺料表';
DECLARE @FormID nvarchar(100) = N'8FBEE7F8-535F-42F0-8974-DDCADC2E90FF';
DECLARE @ComponentUID nvarchar(100) = N'D11FF0EB-3481-487A-A8A3-A7494F064917';
DECLARE @ModelUID nvarchar(100) = N'06EA90C8-AC69-4CF0-8BB1-F12C7BE098E5';
DECLARE @ViewUID nvarchar(100) = N'A79676C5-BB1B-4E9D-891E-743581D457F7';
DECLARE @ParentMenuID bigint = 1001101160309206;
DECLARE @ApplicationID bigint = 3025;
DECLARE @AssemblyName nvarchar(200) = N'U9Custom.UI.ManufactureSimulateShortageTable.Independent';
DECLARE @ClassName nvarchar(300) = N'U9Custom.UI.ManufactureSimulateShortageTable.Independent.ShortageTableUIFormWebPart';
DECLARE @MenuSequence int;
DECLARE @ExistingMenuID bigint;

IF OBJECT_ID('InnerAllocSerials') IS NULL
    EXEC dbo.AllocSerials @AllocCount = 200, @StartSN = @SNIndex OUTPUT;
ELSE
    EXEC dbo.InnerAllocSerials @AllocCount = 200, @StartSN = @SNIndex OUTPUT;

DELETE E
FROM UBF_MD_UIEvent AS E
LEFT JOIN UBF_MD_UIControl AS C ON E.Container = C.UID
LEFT JOIN UBF_MD_UIForm AS F ON C.Container = F.UID
WHERE F.UID = @FormID;
DELETE C
FROM UBF_MD_UIControl AS C
LEFT JOIN UBF_MD_UIForm AS F ON C.Container = F.UID
WHERE F.UID = @FormID;
DELETE FROM UBF_MD_UIForm WHERE UID = @FormID;
DELETE FROM UBF_MD_UIFormParamter WHERE Container = @FormID;
DELETE FROM UBF_MD_UIFormDetail WHERE Container = @FormID;
DELETE FROM aspnet_parts WHERE FormId = @FormID;
DELETE P
FROM UBF_MD_UIProperty AS P
LEFT JOIN UBF_MD_UIModel AS M ON P.Container = M.UID
WHERE M.UID = @ModelUID;
DELETE P
FROM UBF_MD_UIProperty AS P
LEFT JOIN UBF_MD_UIView AS V ON P.Container = V.UID
LEFT JOIN UBF_MD_UIModel AS M ON V.Container = M.UID
WHERE M.UID = @ModelUID;
DELETE F
FROM UBF_MD_UIField AS F
LEFT JOIN UBF_MD_UIView AS V ON F.UIView = V.ID
LEFT JOIN UBF_MD_UIModel AS M ON V.UIModel = M.ID
WHERE M.UID = @ModelUID;
DELETE V
FROM UBF_MD_UIView AS V
LEFT JOIN UBF_MD_UIModel AS M ON V.UIModel = M.ID
WHERE M.UID = @ModelUID;
DELETE A
FROM UBF_MD_UIAction AS A
LEFT JOIN UBF_MD_UIModel AS M ON A.UIModel = M.ID
WHERE M.UID = @ModelUID;
DELETE L
FROM UBF_MD_UILink AS L
LEFT JOIN UBF_MD_UIModel AS M ON L.Container = M.UID
WHERE M.UID = @ModelUID;
DELETE FROM UBF_MD_UIModel WHERE UID = @ModelUID;
DELETE FROM UBF_MD_UIComponent WHERE UID = @ComponentUID;

SELECT TOP 1 @ExistingMenuID = ID
FROM UBF_Assemble_Menu
WHERE GUID = @MenuGUID OR URI = @MenuURI OR Name = @MenuName
ORDER BY CASE WHEN GUID = @MenuGUID THEN 0 WHEN URI = @MenuURI THEN 1 ELSE 2 END, ID;

DELETE FROM UBF_Assemble_ColumnPart
WHERE PageColumn IN (
    SELECT PC.ID FROM UBF_Assemble_PageColumn AS PC
    INNER JOIN UBF_Assemble_Page AS P ON PC.Page = P.ID
    WHERE P.URI = @MenuURI OR P.Code = @MenuURI OR P.Name = @MenuName
)
OR Part IN (
    SELECT PT.ID FROM UBF_Assemble_Part AS PT
    INNER JOIN UBF_Assemble_Page AS P ON PT.Page = P.ID
    WHERE P.URI = @MenuURI OR P.Code = @MenuURI OR P.Name = @MenuName
);
DELETE FROM UBF_Assemble_Part_Trl
WHERE ID IN (
    SELECT PT.ID FROM UBF_Assemble_Part AS PT
    INNER JOIN UBF_Assemble_Page AS P ON PT.Page = P.ID
    WHERE P.URI = @MenuURI OR P.Code = @MenuURI OR P.Name = @MenuName
);
DELETE FROM UBF_Assemble_Part
WHERE Page IN (SELECT ID FROM UBF_Assemble_Page WHERE URI = @MenuURI OR Code = @MenuURI OR Name = @MenuName);
DELETE FROM UBF_Assemble_PageColumn_Trl
WHERE ID IN (
    SELECT PC.ID FROM UBF_Assemble_PageColumn AS PC
    INNER JOIN UBF_Assemble_Page AS P ON PC.Page = P.ID
    WHERE P.URI = @MenuURI OR P.Code = @MenuURI OR P.Name = @MenuName
);
DELETE FROM UBF_Assemble_PageColumn
WHERE Page IN (SELECT ID FROM UBF_Assemble_Page WHERE URI = @MenuURI OR Code = @MenuURI OR Name = @MenuName);
DELETE FROM UBF_Assemble_Page_Trl
WHERE ID IN (SELECT ID FROM UBF_Assemble_Page WHERE URI = @MenuURI OR Code = @MenuURI OR Name = @MenuName);
DELETE FROM UBF_Assemble_Page WHERE URI = @MenuURI OR Code = @MenuURI OR Name = @MenuName;

INSERT INTO UBF_MD_UIComponent(ID, UID, Name, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, AssemblyName)
VALUES(@SNIndex + 0, @ComponentUID, @AssemblyName, N'Administrator', GETDATE(), N'Administrator', GETDATE(), @AssemblyName);
INSERT INTO UBF_MD_UIModel(ID, UID, Name, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, UIComponent)
VALUES(@SNIndex + 1, @ModelUID, N'ShortageTableModel', N'Administrator', GETDATE(), N'Administrator', GETDATE(), @SNIndex + 0);
INSERT INTO UBF_MD_UIView(ID, UID, Name, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, UIModel, IsMainView, IsExtendView, FilterOriginalOPath, FilterOrderBy, DataSource, Container, MappingID)
VALUES(@SNIndex + 2, @ViewUID, N'ShortageTable', N'Administrator', GETDATE(), N'Administrator', GETDATE(), @SNIndex + 1, 1, 0, N'', N'', @ViewUID, @ModelUID, NULL);

DECLARE @Fields TABLE(Seq int IDENTITY(1,1), UID nvarchar(100), Name nvarchar(100), ToolTips nvarchar(200), DataType nvarchar(100));
INSERT INTO @Fields(UID, Name, ToolTips, DataType) VALUES
(N'47EFAE85-EEA2-4096-B339-05580797A865', N'ID', N'Key.ID', N'ba391065-6c27-4c82-acc8-b52b1c93a910'),
(N'C5B1EA92-9699-4FFA-B214-60DBA137CE2F', N'ProductionOrder', N'Misc.ProductionOrder', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'09A0A048-EFBA-4216-969E-10878968CE1C', N'MaterialCode', N'Misc.MaterialCode', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'D1635DFA-2156-47AE-9EAB-8484FDC1CB97', N'MaterialName', N'Misc.MaterialName', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'F89E24E8-091D-4018-8417-3ECE022EBB5B', N'DemandQty', N'Misc.DemandQty', N'a5242caa-f9ee-4159-b8c9-d0952a79175a'),
(N'94FB779A-0351-4BDA-9B65-E7B73EC87BFB', N'PlanStartDate', N'Misc.PlanStartDate', N'c9e6bc50-2e39-4f27-9519-da0c7859d37e'),
(N'E8956492-BF0E-490D-AEE7-27F4C3D33F87', N'ReqDate', N'Misc.ReqDate', N'c9e6bc50-2e39-4f27-9519-da0c7859d37e'),
(N'666008E6-0DF0-4EC3-A05C-42B4FC1178AE', N'ScarceQty', N'Misc.ScarceQty', N'a5242caa-f9ee-4159-b8c9-d0952a79175a'),
(N'A6EE5BE1-2598-413E-95A0-96005B05026B', N'SupplyType', N'Misc.SupplyType', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'C2B3C0AF-2B9E-4B84-AC4B-2D6F3C1D4C01', N'BusinessPerson', N'Misc.BusinessPerson', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'204433DC-4A28-4D11-8345-BFD651A3B7B2', N'SupplyNo', N'Misc.SupplyNo', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'E05A5D22-46C5-4A4A-AB46-2A1F0DD5E9A1', N'SupplyLineNo', N'Misc.SupplyLineNo', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'A49BA199-0863-41C0-B2B9-4C1D6DC59443', N'SupplyDate', N'Misc.SupplyDate', N'c9e6bc50-2e39-4f27-9519-da0c7859d37e'),
(N'93F71466-D9C1-4D7A-BFCE-AA5EF37342D8', N'MatchedQty', N'Misc.MatchedQty', N'a5242caa-f9ee-4159-b8c9-d0952a79175a'),
(N'C34435DD-3F59-474F-B648-05008A56665D', N'RemainingShortage', N'Misc.RemainingShortage', N'a5242caa-f9ee-4159-b8c9-d0952a79175a');

INSERT INTO UBF_MD_UIField(ID, UID, Name, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, UIView, ToolTips, DataType, DefaultValue, GroupName, IsReadOnly, IsBinding, Container, MappingID)
SELECT @SNIndex + 20 + Seq, UID, Name, N'Administrator', GETDATE(), N'Administrator', GETDATE(), @SNIndex + 2, ToolTips, DataType, N'', N'Misc', 1, 0, @ViewUID, NULL
FROM @Fields;

INSERT INTO UBF_MD_UIAction(ID, UID, Name, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, UIModel, Container, GroupName)
VALUES(@SNIndex + 3, N'310603C6-1B11-461B-89FB-1FDE86B1CF4E', N'OnQuery', N'Administrator', GETDATE(), N'Administrator', GETDATE(), @SNIndex + 1, @ModelUID, N'Group');
INSERT INTO UBF_MD_UIAction(ID, UID, Name, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, UIModel, Container, GroupName)
VALUES(@SNIndex + 4, N'1E6523ED-538C-42F2-BF8B-A29D1AD96748', N'OnClear', N'Administrator', GETDATE(), N'Administrator', GETDATE(), @SNIndex + 1, @ModelUID, N'Group');

INSERT INTO UBF_MD_UIForm(ID, UID, Container, UIComponent, Name, DataSource, IsMain, IsAuthority, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, ClassName, AssemblyName, URI, PublishDetail, Width, Height)
VALUES(@SNIndex + 5, @FormID, @ComponentUID, @SNIndex + 0, N'ShortageTableUIForm', @ModelUID, 1, 1, N'Administrator', GETDATE(), N'Administrator', GETDATE(), @ClassName, @AssemblyName, @MenuURI, 0, 1600, 690);
INSERT INTO aspnet_parts(ID, FormId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, ClassName, Assembly, Path, URI)
VALUES(@SNIndex + 5, @FormID, N'Administrator', GETDATE(), N'Administrator', GETDATE(), @ClassName, @AssemblyName, @AssemblyName, @MenuURI);

SELECT @MenuSequence = ISNULL(MAX(Sequence), -1) + 1 FROM UBF_Assemble_Menu WHERE Parent = @ParentMenuID;
IF @MenuSequence < 1 SET @MenuSequence = 1;
INSERT INTO UBF_Assemble_Page(ID, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, SysVersion, Name, ShortName, Code, URI, Orientation, Application, Attribute, DefaultEntity)
VALUES(@SNIndex + 80, GETDATE(), N'Administrator', GETDATE(), N'Administrator', 0, @MenuName, N'', @MenuURI, @MenuURI, 0, @ApplicationID, NULL, N'');
INSERT INTO UBF_Assemble_Page_Trl(ID, SysMLFlag, Description, Title) VALUES(@SNIndex + 80, N'zh-CN', N'', @MenuName);
INSERT INTO UBF_Assemble_Part(ID, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, SysVersion, Component, Page, Height, Width, Code, PageCode)
VALUES(@SNIndex + 81, GETDATE(), N'Administrator', GETDATE(), N'Administrator', 0, LOWER(@FormID), @SNIndex + 80, 690, 1600, N'p0', @MenuURI);
INSERT INTO UBF_Assemble_Part_Trl(ID, SysMLFlag, Title) VALUES(@SNIndex + 81, N'zh-CN', @MenuName);
INSERT INTO UBF_Assemble_PageColumn(ID, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, SysVersion, Page, WidthProportion, HeightProportion, IsManRegion)
VALUES(@SNIndex + 82, GETDATE(), N'Administrator', GETDATE(), N'Administrator', 0, @SNIndex + 80, 100, 0, 1);
INSERT INTO UBF_Assemble_PageColumn_Trl(ID, SysMLFlag, Title) VALUES(@SNIndex + 82, N'zh-CN', N'');
INSERT INTO UBF_Assemble_ColumnPart(ID, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, SysVersion, Part, PageColumn)
VALUES(@SNIndex + 83, GETDATE(), N'Administrator', GETDATE(), N'Administrator', 0, @SNIndex + 81, @SNIndex + 82);

IF @ExistingMenuID IS NULL
BEGIN
    SET @ExistingMenuID = @SNIndex + 84;
    INSERT INTO UBF_Assemble_Menu(ID, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, SysVersion, Parent, Image, Action, IsLeaf, IsDialog, Sequence, Name, ShortName, Code, PageParameter, URI, Application, Kind, AppPackage, Encoder, GUID)
    VALUES(@ExistingMenuID, GETDATE(), N'Administrator', GETDATE(), N'Administrator', 0, @ParentMenuID, N'', N'', 1, 0, @MenuSequence, @MenuName, N'new menu', @MenuGUID, N'', @MenuURI, @ApplicationID, 2, 0, N'M10', @MenuGUID);
END
ELSE
BEGIN
    UPDATE UBF_Assemble_Menu
    SET ModifiedOn = GETDATE(), ModifiedBy = N'Administrator', Parent = @ParentMenuID, Image = N'', Action = N'', IsLeaf = 1, IsDialog = 0, Sequence = @MenuSequence, Name = @MenuName, Code = @MenuGUID, PageParameter = N'', URI = @MenuURI, Application = @ApplicationID, Kind = 2, AppPackage = 0, Encoder = N'M10', GUID = @MenuGUID
    WHERE ID = @ExistingMenuID;
END
IF EXISTS (SELECT 1 FROM UBF_Assemble_Menu_Trl WHERE ID = @ExistingMenuID AND SysMLFlag = N'zh-CN')
    UPDATE UBF_Assemble_Menu_Trl SET Description = N'', DisplayName = @MenuName WHERE ID = @ExistingMenuID AND SysMLFlag = N'zh-CN';
ELSE
    INSERT INTO UBF_Assemble_Menu_Trl(ID, SysMLFlag, Description, DisplayName) VALUES(@ExistingMenuID, N'zh-CN', N'', @MenuName);

SELECT @MenuName AS MenuName, @MenuURI AS URI, @FormID AS FormID, @AssemblyName AS AssemblyName, @ParentMenuID AS ParentMenuID;

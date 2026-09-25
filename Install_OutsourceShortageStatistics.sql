SET NOCOUNT ON;

DECLARE @SNIndex bigint;
DECLARE @MenuURI nvarchar(510) = N'U9Custom.OutsourceShortageStatistics.Independent';
DECLARE @MenuGUID nvarchar(100) = N'7739436F-A404-4FC1-BF9A-48F12DA648E6';
DECLARE @MenuName nvarchar(100) = N'委外欠料统计';
DECLARE @FormID nvarchar(100) = N'72FA010B-210D-46A8-83C5-5C94F67586B0';
DECLARE @ComponentUID nvarchar(100) = N'2A70AC7E-D0F1-44AF-962E-6F2EA15DC92B';
DECLARE @ModelUID nvarchar(100) = N'19CD83B9-7624-4072-B422-AD5B99659A50';
DECLARE @ViewUID nvarchar(100) = N'7C1F9ACC-1BF0-4355-A8F3-CD1C2CF8CF20';
DECLARE @ParentMenuID bigint = 1001101160309206;
DECLARE @ApplicationID bigint = 3025;
DECLARE @AssemblyName nvarchar(200) = N'U9Custom.UI.OutsourceShortageStatistics.Independent';
DECLARE @ClassName nvarchar(300) = N'U9Custom.UI.OutsourceShortageStatistics.Independent.OutsourceShortageUIFormWebPart';
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
VALUES(@SNIndex + 1, @ModelUID, N'OutsourceShortageModel', N'Administrator', GETDATE(), N'Administrator', GETDATE(), @SNIndex + 0);
INSERT INTO UBF_MD_UIView(ID, UID, Name, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, UIModel, IsMainView, IsExtendView, FilterOriginalOPath, FilterOrderBy, DataSource, Container, MappingID)
VALUES(@SNIndex + 2, @ViewUID, N'OutsourceShortage', N'Administrator', GETDATE(), N'Administrator', GETDATE(), @SNIndex + 1, 1, 0, N'', N'', @ViewUID, @ModelUID, NULL);

DECLARE @Fields TABLE(Seq int IDENTITY(1,1), UID nvarchar(100), Name nvarchar(100), ToolTips nvarchar(200), DataType nvarchar(100));
INSERT INTO @Fields(UID, Name, ToolTips, DataType) VALUES
(N'22964AA7-035D-4B0C-9BA3-123B10A82F98', N'ID', N'Key.ID', N'ba391065-6c27-4c82-acc8-b52b1c93a910'),
(N'8BF251D3-0C47-4A36-9258-3AFBF91F51DF', N'SupplierCode', N'Misc.SupplierCode', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'87096366-CCD5-418F-80EC-17AFBBB631BB', N'SupplierName', N'Misc.SupplierName', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'A860F530-3528-424C-843B-F25A09D06FBF', N'PurchaseOrderNo', N'Misc.PurchaseOrderNo', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'86CCC448-9850-48A7-992B-01D8B293E56A', N'POLineNo', N'Misc.POLineNo', N'ba391065-6c27-4c82-acc8-b52b1c93a910'),
(N'6EEEC7BA-40EA-450A-B932-2E238A1FEEA0', N'PickLineNo', N'Misc.PickLineNo', N'ba391065-6c27-4c82-acc8-b52b1c93a910'),
(N'D44DA663-70DC-4351-B323-67360C8B6FF2', N'BusinessDate', N'Misc.BusinessDate', N'c9e6bc50-2e39-4f27-9519-da0c7859d37e'),
(N'DC0367D6-B77A-4A23-8377-D736ECF7EE55', N'MaterialCode', N'Misc.MaterialCode', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'730F79DD-099A-47E5-9E07-23F653ECCE4C', N'MaterialName', N'Misc.MaterialName', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'4CB6A7F1-34D7-4254-AA17-99DE9F914023', N'SupplyWhCode', N'Misc.SupplyWhCode', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'82ECD707-514F-4369-B91A-D9A82D8A1A8B', N'SupplyWhName', N'Misc.SupplyWhName', N'3d174255-fd12-47f7-8844-3b5e4fae9e8c'),
(N'98A53531-8206-4981-A38F-19BE18F7168D', N'ActualReqQty', N'Misc.ActualReqQty', N'a5242caa-f9ee-4159-b8c9-d0952a79175a'),
(N'069172D1-EDE1-4D31-A130-88AD86EB8464', N'IssuedQty', N'Misc.IssuedQty', N'a5242caa-f9ee-4159-b8c9-d0952a79175a'),
(N'1A33C9A3-E636-4B73-AC6B-A4E06B3CDA66', N'UnissuedQty', N'Misc.UnissuedQty', N'a5242caa-f9ee-4159-b8c9-d0952a79175a'),
(N'AC09638C-D656-4C71-B5CD-1D485AF11E2A', N'InventoryDeductQty', N'Misc.InventoryDeductQty', N'a5242caa-f9ee-4159-b8c9-d0952a79175a'),
(N'C81530E8-616E-4861-8797-365461539E46', N'ShortageQty', N'Misc.ShortageQty', N'a5242caa-f9ee-4159-b8c9-d0952a79175a'),
(N'FF9FCC74-0FE8-4A81-961D-32072DCC9FE2', N'ItemID', N'Misc.ItemID', N'ba391065-6c27-4c82-acc8-b52b1c93a910'),
(N'0C067673-BDFF-44F9-8A29-0ED142E6550D', N'IssueUOMID', N'Misc.IssueUOMID', N'ba391065-6c27-4c82-acc8-b52b1c93a910'),
(N'416E464C-BCA0-42A7-A12F-82CFB81B56D3', N'SupplyWhID', N'Misc.SupplyWhID', N'ba391065-6c27-4c82-acc8-b52b1c93a910');

INSERT INTO UBF_MD_UIField(ID, UID, Name, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, UIView, ToolTips, DataType, DefaultValue, GroupName, IsReadOnly, IsBinding, Container, MappingID)
SELECT @SNIndex + 20 + Seq, UID, Name, N'Administrator', GETDATE(), N'Administrator', GETDATE(), @SNIndex + 2, ToolTips, DataType, N'', N'Misc', 1, 0, @ViewUID, NULL
FROM @Fields;

INSERT INTO UBF_MD_UIAction(ID, UID, Name, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, UIModel, Container, GroupName)
VALUES(@SNIndex + 3, N'66DFB472-13FA-45BD-9D06-730D027B2439', N'OnQuery', N'Administrator', GETDATE(), N'Administrator', GETDATE(), @SNIndex + 1, @ModelUID, N'Group');
INSERT INTO UBF_MD_UIAction(ID, UID, Name, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, UIModel, Container, GroupName)
VALUES(@SNIndex + 4, N'5D85D340-2B61-4FF4-9A8B-2036226C0E0E', N'OnClear', N'Administrator', GETDATE(), N'Administrator', GETDATE(), @SNIndex + 1, @ModelUID, N'Group');
INSERT INTO UBF_MD_UIAction(ID, UID, Name, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, UIModel, Container, GroupName)
VALUES(@SNIndex + 6, N'493F6FB6-57B7-4F0A-A035-302D08D3C50D', N'OnOutput', N'Administrator', GETDATE(), N'Administrator', GETDATE(), @SNIndex + 1, @ModelUID, N'Group');
INSERT INTO UBF_MD_UIAction(ID, UID, Name, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, UIModel, Container, GroupName)
VALUES(@SNIndex + 7, N'A66A8141-7E67-4A32-BF66-4D292CB3CC86', N'OnWriteMO', N'Administrator', GETDATE(), N'Administrator', GETDATE(), @SNIndex + 1, @ModelUID, N'Group');

INSERT INTO UBF_MD_UIForm(ID, UID, Container, UIComponent, Name, DataSource, IsMain, IsAuthority, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, ClassName, AssemblyName, URI, PublishDetail, Width, Height)
VALUES(@SNIndex + 5, @FormID, @ComponentUID, @SNIndex + 0, N'OutsourceShortageUIForm', @ModelUID, 1, 1, N'Administrator', GETDATE(), N'Administrator', GETDATE(), @ClassName, @AssemblyName, @MenuURI, 0, 1600, 690);
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

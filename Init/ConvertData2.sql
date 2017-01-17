--select top 100 * from tblPatient
--select top 100 * from tblCard
--select top 100 * from tblPatientInfo
--select top 100 * from tblReferralVendor

-- �������� ������ �������
	DELETE FROM tblPatient

	DROP TABLE #CardNumbers
	GO

	DECLARE @ErrPatientID as uniqueidentifier --, @ErrorCard as uniqueidentifier
	SET @ErrPatientID = NEWID()
	SELECT [� ��������] as Number , NEWID() as ID
	INTO #CardNumbers
	FROM [������ ��������]
	UNION
	SELECT -1 as Number, @ErrPatientID AS ID -- ��������� �������, ��� ����� ������ ������ ������������

	-- SELECT Number, ID FROM #CardNumbers   where number IS NULL


	-- �������� ���������
	DECLARE @MIN_DATECREATE DATETIME
	SELECT @MIN_DATECREATE = MIN([���� ��������]) FROM [������ ��������] -- ������ ���� �������� � ���� �����������, ������� ������ ��� ���� ���� ���, �� ������ �����������

	INSERT INTO tblPatient(ID, FirstName, LastName, FatherName, BirthDay, DateCreate)
	SELECT      CardNumbers.ID, [������ ��������].�������, [������ ��������].���, [������ ��������].��������, [������ ��������].[���� ��������], ISNULL([������ ��������].[���� ��������], @MIN_DATECREATE) AS DateCreate
	FROM            [������ ��������]  RIGHT OUTER JOIN
                             (SELECT Number, ID FROM #CardNumbers) AS CardNumbers ON [������ ��������].[� ��������] = CardNumbers.Number

	UPDATE tblPatient -- ��������� ���������� ��������
	SET FirstName = '������',
	    LastName = '������ ������ ��',
		FatherName = '������ ������ ��', 
		DateDelete = GETDATE(), 
		BirthDay = GETDATE()
	WHERE ID = @ErrPatientID

	-- ��������� �������������� ���������
    SET IDENTITY_INSERT tblPatientInfo ON	
	INSERT INTO tblPatientInfo(ID, [Address], PaspSeriya, PaspNumber, PaspIssuing, 
				Job, Post, Phon, Number,
				Kind, 
				Location, 
				Printed)
	SELECT        CardNumbers.ID, [������ ��������].������, [������ ��������].[����� ��������], [������ ��������].[� �������], [������ ��������].[������� �����], [������ ��������].������, 
				[������ ��������].���������, [������ ��������].�������, CardNumbers.Number, 
				CASE WHEN [������ ��������].[���������� �����] = 1 THEN 1 ELSE 0 END AS Kind, 
				CASE WHEN oper.[� ��������] IS NOT NULL THEN 2 ELSE 1 END AS location, 1 AS printed
	FROM  (SELECT [� ��������]
			FROM            �������������
			GROUP BY [� ��������]) AS oper RIGHT OUTER JOIN
		 (SELECT        Number, ID
		    FROM            [#CardNumbers]) AS CardNumbers ON oper.[� ��������] = CardNumbers.Number LEFT OUTER JOIN
							 [������ ��������] ON CardNumbers.Number = [������ ��������].[� ��������]
	GO
	SET IDENTITY_INSERT tblPatientInfo OFF
	GO	

-- Ok

-- ����������� �����������
	DELETE FROM tblEmployees
	GO
	INSERT INTO tblEmployees (ShortName)
	SELECT [������� �����]
	FROM ������������
	GROUP BY [������� �����]
	GO
--Ok


-- ����������� ���������
-- ����������� �����������
-- TODO : �������� ������ ���������! ����� �������� � ��
	DELETE FROM tblReferralVendor
	GO
	INSERT INTO tblReferralVendor (ShortName)
	SELECT [������� �����]
	FROM [dbo].[���� �����������]
	GROUP BY [������� �����]
	GO

-- 1. ����������� ������ �����������
	-- 1.1 �����������:
	-- ������

	DELETE FROM tblOffer
	GO
	INSERT INTO tblOffer(ShortName, DocName, ProductModel, DateDelete, DefaultPrice, Classification,
						 MonitorText, 
						 PriceDependence,
						 AnaliseText,
						 DontDriveText, 
						 PatientRequired)
		-- ������
		SELECT        [��� ��������], [��� ��������] AS Expr1, NULL AS Expr2, NULL/*GETDATE()*/ AS datedelete, MAX([��������� ��������]) AS DefaultPrice, 1 AS Expr3,
				   --IIF(COUNT([� ���_��])>0, 0 /*DocTemplateType.Cataract*/ , 1 /*DocTemplateType.Default*/) AS DocTemplateType, -- ������ ��� ���� ���� ������� �� ��� ������� ��� ���������
				   --3 as DefaudMonitorInfo -- 1 (������) ������
				   CASE WHEN ([��� ��������] LIKE '%�����%') OR ([��� ��������] LIKE '%���%') OR ([��� ��������] LIKE '%���%') THEN '6 (�����) �������' ELSE '1 (������) ������' END AS MonitorText,
				   CASE WHEN ([��� ��������] LIKE '%���%') THEN '� ���������� �������������� �����' 
				        WHEN ([��� ��������] LIKE '%�����%') OR ([��� ��������] LIKE '%���%') THEN '� ���������� � ���������' ELSE '' END AS PriceDependence,
	               CASE WHEN ([��� ��������] LIKE '%���%') THEN 'RW, ��� + ������������ ���������, ���' ELSE '��� ������������� � �� ���������� �����������' END AS AnaliseText,
				   CASE WHEN ([��� ��������] LIKE '%���%') THEN '14 �����' ELSE '7 �����' END AS DontDriveText,
				   CASE WHEN ([��� ��������] LIKE '%�����%') OR ([��� ��������] LIKE '%���%') OR ([��� ��������] LIKE '%���%') THEN '�� ������ ���������� ����� � ������� 2-� ������ �� ����������� � ����������� ��������' ELSE '' END AS PatientRequired
		FROM            ��������
		GROUP BY [��� ��������]
		HAVING        (NOT ([��� ��������] IS NULL)) OR
								 ([��� ��������] = '')

	INSERT INTO tblOffer(ShortName, DocName, ProductModel, DateDelete, DefaultPrice, Classification)
		-- ������
		SELECT ����������, ����������, ������, NULL /*GETDATE()*/ AS datedelete,  MAX([��������� ��������]) AS DefaultPrice ,
		 4 /* OfferClassification.Product = 4 */
		 --, null AS DocTemplateType -- DocTemplateType - ��� ��������� ������������ ����� ���������
		 --, null
		FROM ��������
		GROUP BY ����������, ������
		HAVING NOT( (���������� IS NULL) OR ((����������)  = N''))
	GO
	-- ��������������
	--select {Registratura_1_PrintClass.TypeOper} 
		--case '�����', '���', '���' : '6 (�����) �������' default : '1 (������) ������' 
	UPDATE  tblOffer
	SET MonitorText = CASE WHEN (DocName = '�����') OR (DocName = '���') OR (DocName = '���') THEN '6 (�����) �������' ELSE '1 (������) ������' END
	WHERE (Classification = 1/*OfferClassification.Operation = 1*/)



	--if ((SELECT tblOffer.ID 
	--	 FROM tblOffer INNER JOIN tblOfferService ON tblOffer.ID = tblOfferService.ID 
	--				   INNER JOIN tblOfferProtuct ON tblOffer.ID = tblOfferProtuct.ID) IS NOT NULL)
	--	PRINT ('Offer �� ����� � ����� ID � �������� tblOfferService � tblOfferProtuct !!!')
	--GO
--Ok

-- ����������� �����������

	DROP TABLE #DocID -- ��� ����� ����������� � ������ ����� ���������
	GO
	SELECT [� ��������] as tempDocNumb, ��� as tempYear, NEWID() as ManipulationID, 0 as MType -- 0 ������
	INTO #DocID
	FROM ��������
	UNION
	SELECT [� ��������] as tempDocNumb, ��� as tempYear, NEWID() as ManipulationID, 1 as MType -- 1 �������
	FROM ��������
	WHERE (��������.[� ����] IS NOT NULL) OR (��������.[� ���_��] IS NOT NULL)
	-- select tempDocNumb, tempYear, ManipulationID, MType from #DocID

	-- 1 ����������� ��������

	DELETE FROM tblDocumentCommon
	DELETE FROM tblManipulation
	GO
	DECLARE @ErrPatientID as uniqueidentifier
	SELECT @ErrPatientID = ID FROM tblPatientInfo WHERE Number = -1
	-- SELECT @ErrPatientID = ID FROM tblPatient where (tempNumb is null) and (DateDelete is not null) -- ������� ID ���������� ��������
	INSERT INTO tblManipulation(ID, PatientID, Eye, Price, OfferID, DateRealization, Note)
	SELECT DocID.ManipulationID, 
		   ISNULL(tblPatientInfo.ID, @ErrPatientID) AS PatientID, 
		   CASE WHEN ��������.���� = 'OS' THEN 1 WHEN ��������.���� = 'OD' THEN 2 WHEN ��������.���� = 'OU' THEN 3 ELSE 0 END AS Eye, 
		   ��������.[��������� ��������] AS Price, 
		   tblOffer.ID AS OfferID, 
		   ISNULL(��������.[���� ��������], GETDATE()) AS DateReal, 
		   ��������.���������� AS Note
			--��������.[� ��������], ��������.���
	FROM (SELECT        tempDocNumb, tempYear, ManipulationID, MType FROM [#DocID]) AS DocID INNER JOIN
							 �������� ON DocID.tempDocNumb = ��������.[� ��������] AND DocID.tempYear = ��������.��� INNER JOIN
							 tblOffer ON ��������.[��� ��������] = tblOffer.DocName LEFT OUTER JOIN
							 tblPatientInfo ON ��������.[� ��������] = tblPatientInfo.Number
	WHERE DocID.MType = 0

	-- 2  ����������� ������ (���� ������������)
	INSERT INTO tblManipulation(ID, PatientID, Eye, Price, OfferID, DateRealization, Note)
	SELECT  DocID.ManipulationID, 
			ISNULL(tblPatientInfo.ID, @ErrPatientID) AS PatientID, 
			CASE WHEN ��������.���� = 'OS' THEN 1 WHEN ��������.���� = 'OD' THEN 2 WHEN ��������.���� = 'OU' THEN 3 ELSE 0 END AS Eye, 
			��������.���������� AS Price, 
			tblOffer.ID AS OfferID, 
			ISNULL(��������.[���� ��������], GETDATE()) AS DateReal, 
			��������.���������� AS Note
	FROM            (SELECT  tempDocNumb, tempYear, ManipulationID, MType
							  FROM            [#DocID]) AS DocID INNER JOIN
							 �������� ON DocID.tempDocNumb = ��������.[� ��������] AND DocID.tempYear = ��������.��� INNER JOIN
							 tblOffer ON ��������.���������� = tblOffer.DocName LEFT OUTER JOIN
							 tblPatientInfo ON ��������.[� ��������] = tblPatientInfo.Number
	WHERE        (DocID.MType = 1) AND ((��������.[� ����] IS NOT NULL) OR (��������.[� ���_��] IS NOT NULL))

	GO

	-- ��������� ������	��������� ����
	DECLARE @FIRST_RECEPT_ID AS uniqueidentifier
	SET @FIRST_RECEPT_ID = NEWID()
	INSERT INTO tblOffer (ID, DocName, ShortName, DefaultPrice, Classification) 
	VALUES     (@FIRST_RECEPT_ID, '��������� ����', '��������� ����', 500, 3/* OfferClassification.PrimaryReception = 3 */)

	-- ��������� ������	��������� ����
	INSERT INTO tblOffer (DocName, ShortName, DefaultPrice, Classification) 
	VALUES     ('����������', '����������', 0, 2/* OfferClassification.Monitor = 2 */)

	-- ��������� ������	������� ����
	INSERT INTO tblOffer (DocName, ShortName, DefaultPrice, Classification) 
	VALUES     ('������� ���', '������� ���', 500, 5/* OfferClassification.PaidReception = 5 */)

  -- 3 ����������� ���������� ������
	INSERT INTO tblManipulation(PatientID, OfferID, Eye, Price, DateRealization, ResponsibleEmpID)
	SELECT tblPatientInfo.ID AS PatientID, @FIRST_RECEPT_ID AS OfferID, 3 AS Eye, [������ ��������].���� AS Price,
		 ISNULL([������ ��������].[���� ��������], GETDATE()) AS DateReal, tblEmployees.ID AS RespEmp
	FROM tblPatientInfo INNER JOIN
		������������ ON tblPatientInfo.Number = ������������.[� ��������] INNER JOIN
		tblEmployees ON ������������.[������� �����] = tblEmployees.ShortName LEFT OUTER JOIN
		[������ ��������] ON ������������.[� ��������] = [������ ��������].[� ��������]
	GO


-- Ok


-- ������� ��������� Common
-- 4 /* OfferClassification.Product = 4 */
-- 1 /* OfferClassification.Operation = 1 */

-- 1 ������

	

	DELETE FROM tblDocumentCommon
	INSERT INTO tblDocumentCommon (PatientID, ManipulationID, Number, 
									FirstName, LastName, FatherName, BirthDay, Address, PaspSeriya, PaspNumber, PaspIssuing, 
									Phon, Eye, Price, Printed, DateRealization)
	SELECT        tblManipulation.PatientID, tblManipulation.ID AS ManipulationID, DocID.tempDocNumb, tblPatient.FirstName, tblPatient.LastName, tblPatient.FatherName, tblPatient.BirthDay, tblPatientInfo.Address, 
							 tblPatientInfo.PaspSeriya, tblPatientInfo.PaspNumber, tblPatientInfo.PaspIssuing, tblPatientInfo.Phon, tblManipulation.Eye, tblManipulation.Price, 1 AS printed, tblManipulation.DateRealization
	FROM            tblPatientInfo INNER JOIN
							 tblManipulation INNER JOIN
							 tblPatient ON tblManipulation.PatientID = tblPatient.ID INNER JOIN
								 (SELECT        tempDocNumb, tempYear, ManipulationID, MType
								   FROM            [#DocID]) AS DocID ON tblManipulation.ID = DocID.ManipulationID ON tblPatientInfo.ID = tblManipulation.PatientID INNER JOIN
							 tblOffer ON tblManipulation.OfferID = tblOffer.ID
	WHERE        (DocID.MType = 0)

-- ��������� ������
	INSERT INTO tblDocumentService
							 (ID, Name)
	SELECT        tblDocumentCommon.ID, tblOffer.DocName
	FROM            tblDocumentCommon INNER JOIN
							 tblManipulation ON tblDocumentCommon.ManipulationID = tblManipulation.ID INNER JOIN
							 tblOffer ON tblManipulation.OfferID = tblOffer.ID
	WHERE       (tblDocumentCommon.TriggerDocType = 0)


-- 2 �������� ������ 
  -- ����������������� �� ����� ��� ������� ���������� ������ �� ���� ���������
	INSERT INTO tblDocumentCommon(PatientID, ManipulationID, Number, 
				FirstName, LastName, FatherName, BirthDay, Address, 
				PaspSeriya, PaspNumber, PaspIssuing, Phon, Eye, Price, Printed, 
				DateRealization)
	SELECT       tblManipulation.PatientID, tblManipulation.ID AS ManipulationID, ��������.[� ���_��], 
				 tblPatient.FirstName, tblPatient.LastName, tblPatient.FatherName, tblPatient.BirthDay, tblPatientInfo.Address, 
				 tblPatientInfo.PaspSeriya, tblPatientInfo.PaspNumber, tblPatientInfo.PaspIssuing, tblPatientInfo.Phon, tblManipulation.Eye, tblManipulation.Price, 1 AS printed, 
				 tblManipulation.DateRealization
	FROM            (SELECT        tempDocNumb, tempYear, ManipulationID, MType
							  FROM            [#DocID]) AS DocID INNER JOIN
							 �������� ON DocID.tempDocNumb = ��������.[� ��������] AND DocID.tempYear = ��������.��� INNER JOIN
							 tblManipulation INNER JOIN
							 tblPatient ON tblManipulation.PatientID = tblPatient.ID INNER JOIN
							 tblPatientInfo ON tblManipulation.PatientID = tblPatientInfo.ID ON DocID.ManipulationID = tblManipulation.ID
	WHERE        (DocID.MType = 1)


-- ��������� �������� ����� �������
	INSERT INTO tblDocumentProduct
							 (ID, ProductName, ProductModel)
	SELECT        tblDocumentCommon.ID, tblOffer.DocName, tblOffer.ProductModel
	FROM            tblDocumentCommon INNER JOIN
							 tblManipulation ON tblDocumentCommon.ManipulationID = tblManipulation.ID INNER JOIN
							 tblOffer ON tblManipulation.OfferID = tblOffer.ID
	WHERE       (tblDocumentCommon.TriggerDocType = 0)

-- 3 ����
	INSERT INTO tblDocumentCommon(PatientID, ManipulationID, Number, 
				FirstName, LastName, FatherName, BirthDay, Address, 
				PaspSeriya, PaspNumber, PaspIssuing, Phon, Eye, Price, Printed, 
				DateRealization)
	SELECT       tblManipulation.PatientID, tblManipulation.ID AS ManipulationID, ��������.[� ����], 
				 tblPatient.FirstName, tblPatient.LastName, tblPatient.FatherName, tblPatient.BirthDay, tblPatientInfo.Address, 
				 tblPatientInfo.PaspSeriya, tblPatientInfo.PaspNumber, tblPatientInfo.PaspIssuing, tblPatientInfo.Phon, tblManipulation.Eye, tblManipulation.Price, 1 AS printed, 
				 tblManipulation.DateRealization
	FROM            (SELECT        tempDocNumb, tempYear, ManipulationID, MType
							  FROM            [#DocID]) AS DocID INNER JOIN
							 �������� ON DocID.tempDocNumb = ��������.[� ��������] AND DocID.tempYear = ��������.��� INNER JOIN
							 tblManipulation INNER JOIN
							 tblPatient ON tblManipulation.PatientID = tblPatient.ID INNER JOIN
							 tblPatientInfo ON tblManipulation.PatientID = tblPatientInfo.ID ON DocID.ManipulationID = tblManipulation.ID
	WHERE        (DocID.MType = 1)

-- ��������� ����
	INSERT INTO tblDocumentSalesReceipt
							 (ID, ProductName, ProductModel)
	SELECT        tblDocumentCommon.ID, tblOffer.DocName, tblOffer.ProductModel
	FROM            tblDocumentCommon INNER JOIN
							 tblManipulation ON tblDocumentCommon.ManipulationID = tblManipulation.ID INNER JOIN
							 tblOffer ON tblManipulation.OfferID = tblOffer.ID
	WHERE       (tblDocumentCommon.TriggerDocType = 0)
  GO

-- �������������� �����
	-- ��� ������ ����������
	UPDATE tblOffer
	SET DateDelete = NULL 
	where Classification = 4

	-- ��������� ������� �����������, ��� �� ������� ������ ���������� � �����
	UPDATE  tblOffer
	SET   [Rank] = derivedtbl_1.manCount
	FROM            (SELECT        tblManipulation.OfferID, COUNT(*) AS manCount
							  FROM            tblManipulation INNER JOIN
														tblOffer AS tblOffer_1 ON tblManipulation.OfferID = tblOffer_1.ID
							  GROUP BY tblOffer_1.DocName, tblManipulation.OfferID
							 ) AS derivedtbl_1 INNER JOIN
							 tblOffer ON derivedtbl_1.OfferID = tblOffer.ID
    -- 20 ����� ���������� ��������������
	UPDATE       TOP (20) tblOffer
	SET                DateDelete = NULL
	FROM            tblOffer AS tblOffer_1 INNER JOIN
							 tblOffer ON tblOffer_1.ID = tblOffer.ID

	GO



DROP TABLE #CardNumbers
GO
DROP TABLE #DocID -- ��� ����� ����������� � ������ ����� ���������
GO

-- ��������� � ������ �������
--select {Registratura_1_PrintClass.TypeOper}
-- case  '��� + ��.���', '���� + ��.���', '�����������' :  {Registratura_1_PrintClass.TypeOper}
--default: '������������������ ��������� � ������������ �������������� ����� ' +  {Registratura_1_PrintClass.TypeOper}



--UPDATE  tblOffer
--SET DefailtFormeterID = 1, DateDelete = NULL
--WHERE (DocName = '��� + ��.���') OR (DocName = '���� + ��.���') OR (DocName = '�����������')

--UPDATE       tblDocumentCommon
--SET                PrintTemplateType = 1
--FROM            tblDocumentCommon INNER JOIN
--                         tblDocumentService ON tblDocumentCommon.ID = tblDocumentService.ID
--WHERE     (Name = '��� + ��.���') OR (Name = '���� + ��.���') OR (Name = '�����������')

/*
drop table [dbo].[������������]
drop table [dbo].[��������]
drop table [dbo].[������]
drop table [dbo].[�������������]
drop table [dbo].[���� �����������]
drop table [dbo].[���� ����������� Report]
drop table [dbo].[������ ��������]

*/
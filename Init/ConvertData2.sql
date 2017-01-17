--select top 100 * from tblPatient
--select top 100 * from tblCard
--select top 100 * from tblPatientInfo
--select top 100 * from tblReferralVendor

-- Копируем данные пациена
	DELETE FROM tblPatient

	DROP TABLE #CardNumbers
	GO

	DECLARE @ErrPatientID as uniqueidentifier --, @ErrorCard as uniqueidentifier
	SET @ErrPatientID = NEWID()
	SELECT [№ карточки] as Number , NEWID() as ID
	INTO #CardNumbers
	FROM [Данные пациента]
	UNION
	SELECT -1 as Number, @ErrPatientID AS ID -- Ошибочный пациетн, для учета ошибок старой регистратуры

	-- SELECT Number, ID FROM #CardNumbers   where number IS NULL


	-- копируем пациентов
	DECLARE @MIN_DATECREATE DATETIME
	SELECT @MIN_DATECREATE = MIN([Дата создания]) FROM [Данные пациента] -- Раньше дата создания е была проставлена, поэтому считаю что если даты нет, то ставим минимальную

	INSERT INTO tblPatient(ID, FirstName, LastName, FatherName, BirthDay, DateCreate)
	SELECT      CardNumbers.ID, [Данные пациента].Фамилия, [Данные пациента].Имя, [Данные пациента].Отчество, [Данные пациента].[Дата рождения], ISNULL([Данные пациента].[Дата создания], @MIN_DATECREATE) AS DateCreate
	FROM            [Данные пациента]  RIGHT OUTER JOIN
                             (SELECT Number, ID FROM #CardNumbers) AS CardNumbers ON [Данные пациента].[№ карточки] = CardNumbers.Number

	UPDATE tblPatient -- обновляем ошибочного пациента
	SET FirstName = 'Удален',
	    LastName = 'Ошибка старой БД',
		FatherName = 'Ошибка старой БД', 
		DateDelete = GETDATE(), 
		BirthDay = GETDATE()
	WHERE ID = @ErrPatientID

	-- Компируем дополнительную инфорацию
    SET IDENTITY_INSERT tblPatientInfo ON	
	INSERT INTO tblPatientInfo(ID, [Address], PaspSeriya, PaspNumber, PaspIssuing, 
				Job, Post, Phon, Number,
				Kind, 
				Location, 
				Printed)
	SELECT        CardNumbers.ID, [Данные пациента].Адресс, [Данные пациента].[Серия паспорта], [Данные пациента].[№ пспорта], [Данные пациента].[Паспорт выдан], [Данные пациента].Работа, 
				[Данные пациента].Должность, [Данные пациента].Телефон, CardNumbers.Number, 
				CASE WHEN [Данные пациента].[Порядковый номер] = 1 THEN 1 ELSE 0 END AS Kind, 
				CASE WHEN oper.[№ карточки] IS NOT NULL THEN 2 ELSE 1 END AS location, 1 AS printed
	FROM  (SELECT [№ карточки]
			FROM            Оперированные
			GROUP BY [№ карточки]) AS oper RIGHT OUTER JOIN
		 (SELECT        Number, ID
		    FROM            [#CardNumbers]) AS CardNumbers ON oper.[№ карточки] = CardNumbers.Number LEFT OUTER JOIN
							 [Данные пациента] ON CardNumbers.Number = [Данные пациента].[№ карточки]
	GO
	SET IDENTITY_INSERT tblPatientInfo OFF
	GO	

-- Ok

-- Копирование сотрудников
	DELETE FROM tblEmployees
	GO
	INSERT INTO tblEmployees (ShortName)
	SELECT [Фамилия врвча]
	FROM Диагностврач
	GROUP BY [Фамилия врвча]
	GO
--Ok


-- Копирование партнеров
-- Копирование направлений
-- TODO : Выяснить логику начисения! после включать в ПО
	DELETE FROM tblReferralVendor
	GO
	INSERT INTO tblReferralVendor (ShortName)
	SELECT [Фамилия врвча]
	FROM [dbo].[Учет направлений]
	GROUP BY [Фамилия врвча]
	GO

-- 1. формировние прайса предложений
	-- 1.1 предолжения:
	-- услуги

	DELETE FROM tblOffer
	GO
	INSERT INTO tblOffer(ShortName, DocName, ProductModel, DateDelete, DefaultPrice, Classification,
						 MonitorText, 
						 PriceDependence,
						 AnaliseText,
						 DontDriveText, 
						 PatientRequired)
		-- услуги
		SELECT        [Тип операции], [Тип операции] AS Expr1, NULL AS Expr2, NULL/*GETDATE()*/ AS datedelete, MAX([Стоимость операции]) AS DefaultPrice, 1 AS Expr3,
				   --IIF(COUNT([№ куп_пр])>0, 0 /*DocTemplateType.Cataract*/ , 1 /*DocTemplateType.Default*/) AS DocTemplateType, -- считаю что если есть продажа то это договор для катаракты
				   --3 as DefaudMonitorInfo -- 1 (одного) месяца
				   CASE WHEN ([Тип операции] LIKE '%ЛАСИК%') OR ([Тип операции] LIKE '%ФРК%') OR ([Тип операции] LIKE '%ФТК%') THEN '6 (шести) месяцев' ELSE '1 (одного) месяца' END AS MonitorText,
				   CASE WHEN ([Тип операции] LIKE '%ФЭК%') THEN 'и стоимостью интраокулярной линзы' 
				        WHEN ([Тип операции] LIKE '%ЛАСИК%') OR ([Тип операции] LIKE '%ФРК%') THEN 'и выражается в диоптриях' ELSE '' END AS PriceDependence,
	               CASE WHEN ([Тип операции] LIKE '%ФЭК%') THEN 'RW, ЭКГ + консультацию терапевта, ФЛГ' ELSE 'при необходимости и по требованию Исполнителя' END AS AnaliseText,
				   CASE WHEN ([Тип операции] LIKE '%ФЭК%') THEN '14 суток' ELSE '7 суток' END AS DontDriveText,
				   CASE WHEN ([Тип операции] LIKE '%ЛАСИК%') OR ([Тип операции] LIKE '%ФРК%') OR ([Тип операции] LIKE '%ФТК%') THEN 'Не носить контактные линзы в течение 2-х недель до диагностики и назначенной операции' ELSE '' END AS PatientRequired
		FROM            Договоры
		GROUP BY [Тип операции]
		HAVING        (NOT ([Тип операции] IS NULL)) OR
								 ([Тип операции] = '')

	INSERT INTO tblOffer(ShortName, DocName, ProductModel, DateDelete, DefaultPrice, Classification)
		-- Товары
		SELECT Назвтовара, Назвтовара, Модель, NULL /*GETDATE()*/ AS datedelete,  MAX([Стоимость операции]) AS DefaultPrice ,
		 4 /* OfferClassification.Product = 4 */
		 --, null AS DocTemplateType -- DocTemplateType - для продуктов определяется типом документа
		 --, null
		FROM Договоры
		GROUP BY Назвтовара, Модель
		HAVING NOT( (Назвтовара IS NULL) OR ((Назвтовара)  = N''))
	GO
	-- скорректироваь
	--select {Registratura_1_PrintClass.TypeOper} 
		--case 'ЛАСИК', 'ФРК', 'ФТК' : '6 (шести) месяцев' default : '1 (одного) месяца' 
	UPDATE  tblOffer
	SET MonitorText = CASE WHEN (DocName = 'ЛАСИК') OR (DocName = 'ФРК') OR (DocName = 'ФТК') THEN '6 (шести) месяцев' ELSE '1 (одного) месяца' END
	WHERE (Classification = 1/*OfferClassification.Operation = 1*/)



	--if ((SELECT tblOffer.ID 
	--	 FROM tblOffer INNER JOIN tblOfferService ON tblOffer.ID = tblOfferService.ID 
	--				   INNER JOIN tblOfferProtuct ON tblOffer.ID = tblOfferProtuct.ID) IS NOT NULL)
	--	PRINT ('Offer не может с одним ID в таблицах tblOfferService и tblOfferProtuct !!!')
	--GO
--Ok

-- Копирование манипуляций

	DROP TABLE #DocID -- для связи манипуляций и старых строк договоров
	GO
	SELECT [№ договора] as tempDocNumb, год as tempYear, NEWID() as ManipulationID, 0 as MType -- 0 услуги
	INTO #DocID
	FROM Договоры
	UNION
	SELECT [№ договора] as tempDocNumb, год as tempYear, NEWID() as ManipulationID, 1 as MType -- 1 продажи
	FROM Договоры
	WHERE (Договоры.[№ чека] IS NOT NULL) OR (Договоры.[№ куп_пр] IS NOT NULL)
	-- select tempDocNumb, tempYear, ManipulationID, MType from #DocID

	-- 1 копирование Операций

	DELETE FROM tblDocumentCommon
	DELETE FROM tblManipulation
	GO
	DECLARE @ErrPatientID as uniqueidentifier
	SELECT @ErrPatientID = ID FROM tblPatientInfo WHERE Number = -1
	-- SELECT @ErrPatientID = ID FROM tblPatient where (tempNumb is null) and (DateDelete is not null) -- Находим ID Удаленного пациента
	INSERT INTO tblManipulation(ID, PatientID, Eye, Price, OfferID, DateRealization, Note)
	SELECT DocID.ManipulationID, 
		   ISNULL(tblPatientInfo.ID, @ErrPatientID) AS PatientID, 
		   CASE WHEN Договоры.Глаз = 'OS' THEN 1 WHEN Договоры.Глаз = 'OD' THEN 2 WHEN Договоры.Глаз = 'OU' THEN 3 ELSE 0 END AS Eye, 
		   Договоры.[Стоимость операции] AS Price, 
		   tblOffer.ID AS OfferID, 
		   ISNULL(Договоры.[Дата операции], GETDATE()) AS DateReal, 
		   Договоры.Примечание AS Note
			--Договоры.[№ договора], Договоры.год
	FROM (SELECT        tempDocNumb, tempYear, ManipulationID, MType FROM [#DocID]) AS DocID INNER JOIN
							 Договоры ON DocID.tempDocNumb = Договоры.[№ договора] AND DocID.tempYear = Договоры.год INNER JOIN
							 tblOffer ON Договоры.[Тип операции] = tblOffer.DocName LEFT OUTER JOIN
							 tblPatientInfo ON Договоры.[№ карточки] = tblPatientInfo.Number
	WHERE DocID.MType = 0

	-- 2  Копирование продаж (суть дублирование)
	INSERT INTO tblManipulation(ID, PatientID, Eye, Price, OfferID, DateRealization, Note)
	SELECT  DocID.ManipulationID, 
			ISNULL(tblPatientInfo.ID, @ErrPatientID) AS PatientID, 
			CASE WHEN Договоры.Глаз = 'OS' THEN 1 WHEN Договоры.Глаз = 'OD' THEN 2 WHEN Договоры.Глаз = 'OU' THEN 3 ELSE 0 END AS Eye, 
			Договоры.ценатовара AS Price, 
			tblOffer.ID AS OfferID, 
			ISNULL(Договоры.[Дата операции], GETDATE()) AS DateReal, 
			Договоры.Примечание AS Note
	FROM            (SELECT  tempDocNumb, tempYear, ManipulationID, MType
							  FROM            [#DocID]) AS DocID INNER JOIN
							 Договоры ON DocID.tempDocNumb = Договоры.[№ договора] AND DocID.tempYear = Договоры.год INNER JOIN
							 tblOffer ON Договоры.Назвтовара = tblOffer.DocName LEFT OUTER JOIN
							 tblPatientInfo ON Договоры.[№ карточки] = tblPatientInfo.Number
	WHERE        (DocID.MType = 1) AND ((Договоры.[№ чека] IS NOT NULL) OR (Договоры.[№ куп_пр] IS NOT NULL))

	GO

	-- Добавляем услугу	Первичный приём
	DECLARE @FIRST_RECEPT_ID AS uniqueidentifier
	SET @FIRST_RECEPT_ID = NEWID()
	INSERT INTO tblOffer (ID, DocName, ShortName, DefaultPrice, Classification) 
	VALUES     (@FIRST_RECEPT_ID, 'Первичный приём', 'Первичный приём', 500, 3/* OfferClassification.PrimaryReception = 3 */)

	-- Добавляем услугу	Вторичный приём
	INSERT INTO tblOffer (DocName, ShortName, DefaultPrice, Classification) 
	VALUES     ('Наблюдение', 'Наблюдение', 0, 2/* OfferClassification.Monitor = 2 */)

	-- Добавляем услугу	Платный приём
	INSERT INTO tblOffer (DocName, ShortName, DefaultPrice, Classification) 
	VALUES     ('Платный прём', 'Платный прём', 500, 5/* OfferClassification.PaidReception = 5 */)

  -- 3 Копирование первичного приема
	INSERT INTO tblManipulation(PatientID, OfferID, Eye, Price, DateRealization, ResponsibleEmpID)
	SELECT tblPatientInfo.ID AS PatientID, @FIRST_RECEPT_ID AS OfferID, 3 AS Eye, [Данные пациента].Цена AS Price,
		 ISNULL([Данные пациента].[Дата создания], GETDATE()) AS DateReal, tblEmployees.ID AS RespEmp
	FROM tblPatientInfo INNER JOIN
		Диагностврач ON tblPatientInfo.Number = Диагностврач.[№ карточки] INNER JOIN
		tblEmployees ON Диагностврач.[Фамилия врвча] = tblEmployees.ShortName LEFT OUTER JOIN
		[Данные пациента] ON Диагностврач.[№ карточки] = [Данные пациента].[№ карточки]
	GO


-- Ok


-- Создаем документы Common
-- 4 /* OfferClassification.Product = 4 */
-- 1 /* OfferClassification.Operation = 1 */

-- 1 услуги

	

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

-- Связываем услуги
	INSERT INTO tblDocumentService
							 (ID, Name)
	SELECT        tblDocumentCommon.ID, tblOffer.DocName
	FROM            tblDocumentCommon INNER JOIN
							 tblManipulation ON tblDocumentCommon.ManipulationID = tblManipulation.ID INNER JOIN
							 tblOffer ON tblManipulation.OfferID = tblOffer.ID
	WHERE       (tblDocumentCommon.TriggerDocType = 0)


-- 2 договоры продаж 
  -- ЗкштеЕуьздфеуЕнзу не нужет тип шаблона выбирается исходя из типа документа
	INSERT INTO tblDocumentCommon(PatientID, ManipulationID, Number, 
				FirstName, LastName, FatherName, BirthDay, Address, 
				PaspSeriya, PaspNumber, PaspIssuing, Phon, Eye, Price, Printed, 
				DateRealization)
	SELECT       tblManipulation.PatientID, tblManipulation.ID AS ManipulationID, Договоры.[№ куп_пр], 
				 tblPatient.FirstName, tblPatient.LastName, tblPatient.FatherName, tblPatient.BirthDay, tblPatientInfo.Address, 
				 tblPatientInfo.PaspSeriya, tblPatientInfo.PaspNumber, tblPatientInfo.PaspIssuing, tblPatientInfo.Phon, tblManipulation.Eye, tblManipulation.Price, 1 AS printed, 
				 tblManipulation.DateRealization
	FROM            (SELECT        tempDocNumb, tempYear, ManipulationID, MType
							  FROM            [#DocID]) AS DocID INNER JOIN
							 Договоры ON DocID.tempDocNumb = Договоры.[№ договора] AND DocID.tempYear = Договоры.год INNER JOIN
							 tblManipulation INNER JOIN
							 tblPatient ON tblManipulation.PatientID = tblPatient.ID INNER JOIN
							 tblPatientInfo ON tblManipulation.PatientID = tblPatientInfo.ID ON DocID.ManipulationID = tblManipulation.ID
	WHERE        (DocID.MType = 1)


-- Связываем договора купли продажи
	INSERT INTO tblDocumentProduct
							 (ID, ProductName, ProductModel)
	SELECT        tblDocumentCommon.ID, tblOffer.DocName, tblOffer.ProductModel
	FROM            tblDocumentCommon INNER JOIN
							 tblManipulation ON tblDocumentCommon.ManipulationID = tblManipulation.ID INNER JOIN
							 tblOffer ON tblManipulation.OfferID = tblOffer.ID
	WHERE       (tblDocumentCommon.TriggerDocType = 0)

-- 3 чеки
	INSERT INTO tblDocumentCommon(PatientID, ManipulationID, Number, 
				FirstName, LastName, FatherName, BirthDay, Address, 
				PaspSeriya, PaspNumber, PaspIssuing, Phon, Eye, Price, Printed, 
				DateRealization)
	SELECT       tblManipulation.PatientID, tblManipulation.ID AS ManipulationID, Договоры.[№ чека], 
				 tblPatient.FirstName, tblPatient.LastName, tblPatient.FatherName, tblPatient.BirthDay, tblPatientInfo.Address, 
				 tblPatientInfo.PaspSeriya, tblPatientInfo.PaspNumber, tblPatientInfo.PaspIssuing, tblPatientInfo.Phon, tblManipulation.Eye, tblManipulation.Price, 1 AS printed, 
				 tblManipulation.DateRealization
	FROM            (SELECT        tempDocNumb, tempYear, ManipulationID, MType
							  FROM            [#DocID]) AS DocID INNER JOIN
							 Договоры ON DocID.tempDocNumb = Договоры.[№ договора] AND DocID.tempYear = Договоры.год INNER JOIN
							 tblManipulation INNER JOIN
							 tblPatient ON tblManipulation.PatientID = tblPatient.ID INNER JOIN
							 tblPatientInfo ON tblManipulation.PatientID = tblPatientInfo.ID ON DocID.ManipulationID = tblManipulation.ID
	WHERE        (DocID.MType = 1)

-- Связываем чеки
	INSERT INTO tblDocumentSalesReceipt
							 (ID, ProductName, ProductModel)
	SELECT        tblDocumentCommon.ID, tblOffer.DocName, tblOffer.ProductModel
	FROM            tblDocumentCommon INNER JOIN
							 tblManipulation ON tblDocumentCommon.ManipulationID = tblManipulation.ID INNER JOIN
							 tblOffer ON tblManipulation.OfferID = tblOffer.ID
	WHERE       (tblDocumentCommon.TriggerDocType = 0)
  GO

-- Разблокировать прайс
	-- все товары правильные
	UPDATE tblOffer
	SET DateDelete = NULL 
	where Classification = 4

	-- Добавляем рейтинг предложению, что бы выводит самыае популярные с верху
	UPDATE  tblOffer
	SET   [Rank] = derivedtbl_1.manCount
	FROM            (SELECT        tblManipulation.OfferID, COUNT(*) AS manCount
							  FROM            tblManipulation INNER JOIN
														tblOffer AS tblOffer_1 ON tblManipulation.OfferID = tblOffer_1.ID
							  GROUP BY tblOffer_1.DocName, tblManipulation.OfferID
							 ) AS derivedtbl_1 INNER JOIN
							 tblOffer ON derivedtbl_1.OfferID = tblOffer.ID
    -- 20 самых популярных разблокировать
	UPDATE       TOP (20) tblOffer
	SET                DateDelete = NULL
	FROM            tblOffer AS tblOffer_1 INNER JOIN
							 tblOffer ON tblOffer_1.ID = tblOffer.ID

	GO



DROP TABLE #CardNumbers
GO
DROP TABLE #DocID -- для связи манипуляций и старых строк договоров
GO

-- Коррекция с учетом отчетов
--select {Registratura_1_PrintClass.TypeOper}
-- case  'ГСЭ + пр.ЗСЭ', 'НГСЭ + пр.ЗСЭ', 'Витрэктомия' :  {Registratura_1_PrintClass.TypeOper}
--default: 'факоэмульсификация катаракты с имплантацией интраокулярной линзы ' +  {Registratura_1_PrintClass.TypeOper}



--UPDATE  tblOffer
--SET DefailtFormeterID = 1, DateDelete = NULL
--WHERE (DocName = 'ГСЭ + пр.ЗСЭ') OR (DocName = 'НГСЭ + пр.ЗСЭ') OR (DocName = 'Витрэктомия')

--UPDATE       tblDocumentCommon
--SET                PrintTemplateType = 1
--FROM            tblDocumentCommon INNER JOIN
--                         tblDocumentService ON tblDocumentCommon.ID = tblDocumentService.ID
--WHERE     (Name = 'ГСЭ + пр.ЗСЭ') OR (Name = 'НГСЭ + пр.ЗСЭ') OR (Name = 'Витрэктомия')

/*
drop table [dbo].[Диагностврач]
drop table [dbo].[Договоры]
drop table [dbo].[Журнал]
drop table [dbo].[Оперированные]
drop table [dbo].[Учет направлений]
drop table [dbo].[Учет направлений Report]
drop table [dbo].[Данные пациента]

*/
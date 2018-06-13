SELECT        tblPatientInfo.Number AS [Номер карты], tblPatient.FirstName AS Фамилия, tblPatient.LastName AS Имя, tblPatient.FatherName AS Отчество, 
                         tblPatientInfo.Address AS Адрес, tblPatientInfo.Job AS [Место службы. работы], tblPatientInfo.Post AS [Профессия. должность], 
                         tblPatient.BirthDay AS [Дата рождени], tblPatientInfo.PaspSeriya AS [Серия документа], tblPatientInfo.PaspNumber AS [Номер документа], 
                         tblPatientInfo.PaspIssuing AS [Кем выдан], tblPatientInfo.Phon AS Телефон, tblPatient.DateDelete
FROM            tblPatient INNER JOIN
                         tblPatientInfo ON tblPatient.ID = tblPatientInfo.ID
WHERE        (tblPatient.DateDelete IS NULL)
SELECT        tblPatientInfo.Number AS [����� �����], tblPatient.FirstName AS �������, tblPatient.LastName AS ���, tblPatient.FatherName AS ��������, 
                         tblPatientInfo.Address AS �����, tblPatientInfo.Job AS [����� ������. ������], tblPatientInfo.Post AS [���������. ���������], 
                         tblPatient.BirthDay AS [���� �������], tblPatientInfo.PaspSeriya AS [����� ���������], tblPatientInfo.PaspNumber AS [����� ���������], 
                         tblPatientInfo.PaspIssuing AS [��� �����], tblPatientInfo.Phon AS �������, tblPatient.DateDelete
FROM            tblPatient INNER JOIN
                         tblPatientInfo ON tblPatient.ID = tblPatientInfo.ID
WHERE        (tblPatient.DateDelete IS NULL)
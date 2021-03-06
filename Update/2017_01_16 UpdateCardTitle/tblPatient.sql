/*
   13 января 2017 г.22:12:21
   Пользователь: sa
   Сервер: RECEPTION2\SQLEXPRESS
   База данных: RealVision
   Приложение: 
*/

/* Чтобы предотвратить возможность потери данных, необходимо внимательно просмотреть этот скрипт, прежде чем запускать его вне контекста конструктора баз данных.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.tblPatient ADD
	Sex smallint NOT NULL CONSTRAINT DF_tblPatient_Sex DEFAULT 0
GO
ALTER TABLE dbo.tblPatient SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

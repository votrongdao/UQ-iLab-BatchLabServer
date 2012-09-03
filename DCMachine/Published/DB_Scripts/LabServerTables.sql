IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[Queue]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
	DROP TABLE [dbo].[Queue]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[Results]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
	DROP TABLE [dbo].[Results]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[Statistics]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
	DROP TABLE [dbo].[Statistics]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[ServiceBrokers]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
	DROP TABLE [dbo].[ServiceBrokers]
GO

/*********************************************************************************************************************/

CREATE TABLE [dbo].[Queue] (
	Id int IDENTITY (1, 1) NOT NULL,
	ExperimentId int NOT NULL,
	SbName varchar(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	UserGroup varchar(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	PriorityHint int NOT NULL,
	XmlSpecification varchar(max) NULL,
	EstimatedExecTime int NOT NULL,
	Status varchar(16) NOT NULL,
	UnitId int NULL,
	Cancelled bit NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Queue] WITH NOCHECK
ADD CONSTRAINT [PK_Queue] PRIMARY KEY CLUSTERED (
	Id
) ON [PRIMARY] 
GO

/*********************************************************************************************************************/

CREATE TABLE [dbo].[Results] (
	Id int IDENTITY (1, 1) NOT NULL,
	ExperimentId int NOT NULL,
	SbName varchar(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	UserGroup varchar(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	PriorityHint int NOT NULL,
	Status varchar(16) NOT NULL,
	XmlExperimentResult varchar(max) NULL,
	XmlResultExtension varchar(2048) NULL,
	XmlBlobExtension varchar(2048) NULL,
	WarningMessages varchar(2048) NULL,
	ErrorMessage varchar(2048) NULL,
	Notified bit NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Results] WITH NOCHECK
ADD CONSTRAINT [PK_Results] PRIMARY KEY CLUSTERED (
	Id
) ON [PRIMARY] 
GO

/*********************************************************************************************************************/

CREATE TABLE [dbo].[Statistics] (
	Id int IDENTITY (1, 1) NOT NULL,
	ExperimentId int NOT NULL,
	SbName varchar(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	UserGroup varchar(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	PriorityHint int NOT NULL,
	EstimatedExecTime int NOT NULL,
	TimeSubmitted datetime NOT NULL,
	QueueLength int NOT NULL,
	EstimatedWaitTime int NOT NULL,
	TimeStarted datetime NULL,
	UnitId int NULL,
	TimeCompleted datetime NULL,
	Cancelled bit NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Statistics] WITH NOCHECK
ADD CONSTRAINT [PK_Statistics] PRIMARY KEY CLUSTERED (
	Id
) ON [PRIMARY] 
GO

/*********************************************************************************************************************/

CREATE TABLE [dbo].[ServiceBrokers] (
	Id int IDENTITY (1, 1) NOT NULL,
	Name varchar(32) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Guid varchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	OutgoingPasskey varchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	IncomingPasskey varchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	WebServiceUrl varchar(256) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	IsAllowed bit NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ServiceBrokers] WITH NOCHECK
ADD CONSTRAINT [PK_ServiceBrokers] PRIMARY KEY CLUSTERED (
	Id
) ON [PRIMARY] 
GO

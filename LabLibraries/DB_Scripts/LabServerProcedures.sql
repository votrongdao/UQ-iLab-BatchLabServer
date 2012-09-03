IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[StoreQueue]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[StoreQueue]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[UpdateQueueStatusToRunning]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[UpdateQueueStatusToRunning]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[UpdateQueueStatus]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[UpdateQueueStatus]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[UpdateQueueCancel]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[UpdateQueueCancel]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[RetrieveQueue]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[RetrieveQueue]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[RetrieveQueueAllWithStatus]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[RetrieveQueueAllWithStatus]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[RetrieveQueueCountWithStatus]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[RetrieveQueueCountWithStatus]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[StoreResults]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[StoreResults]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[UpdateResultsNotified]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[UpdateResultsNotified]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[RetrieveResults]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[RetrieveResults]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[RetrieveResultsAllNotNotified]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[RetrieveResultsAllNotNotified]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[RetrieveResultsAll]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[RetrieveResultsAll]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[StoreStatisticsSubmitted]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[StoreStatisticsSubmitted]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[UpdateStatisticsStarted]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[UpdateStatisticsStarted]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[UpdateStatisticsCompleted]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[UpdateStatisticsCompleted]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[UpdateStatisticsCancelled]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[UpdateStatisticsCancelled]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[RetrieveAllStatistics]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[RetrieveAllStatistics]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[RetrieveStatisticsBySbName]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[RetrieveStatisticsBySbName]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[RetrieveStatisticsByUserGroup]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[RetrieveStatisticsByUserGroup]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[DeleteServiceBroker]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[DeleteServiceBroker]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[RetrieveServiceBroker]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[RetrieveServiceBroker]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[RetrieveServiceBrokerAll]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[RetrieveServiceBrokerAll]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[StoreServiceBroker]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[StoreServiceBroker]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[UpdateServiceBroker]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[UpdateServiceBroker]
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE StoreQueue
	@ExperimentId int,
	@SbName varchar(256),
	@UserGroup varchar(256),
	@PriorityHint int,
	@XmlSpecification varchar(max),
	@EstimatedExecTime int,
	@Status varchar(16)
AS
BEGIN TRANSACTION
	INSERT INTO [dbo].[Queue] (ExperimentId, SbName, UserGroup, PriorityHint, XmlSpecification, EstimatedExecTime, Status, UnitId, Cancelled)
	VALUES (@ExperimentId, @SbName, @UserGroup, @PriorityHint, @XmlSpecification, @EstimatedExecTime, @Status, -1, 0)
	IF (@@error > 0)
		GOTO on_error
COMMIT TRANSACTION	
RETURN
	on_error: 
	ROLLBACK TRANSACTION
RETURN
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE UpdateQueueStatusToRunning
	@ExperimentId int,
	@SbName varchar(256),
	@Status varchar(16),
	@UnitId int
AS
BEGIN TRANSACTION
	UPDATE [dbo].[Queue]
	SET Status = @Status, UnitId = @UnitId
	WHERE ExperimentId = @ExperimentId and SbName = @SbName
	IF (@@error > 0)
		GOTO on_error
COMMIT TRANSACTION	
RETURN
	on_error: 
	ROLLBACK TRANSACTION
RETURN
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE UpdateQueueStatus
	@ExperimentId int,
	@SbName varchar(256),
	@Status varchar(16)
AS
BEGIN TRANSACTION
	UPDATE [dbo].[Queue]
	SET Status = @Status
	WHERE ExperimentId = @ExperimentId and SbName = @SbName
	IF (@@error > 0)
		GOTO on_error
COMMIT TRANSACTION	
RETURN
	on_error: 
	ROLLBACK TRANSACTION
RETURN
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE UpdateQueueCancel
	@ExperimentId int,
	@SbName varchar(256),
	@Status varchar(16)
AS
BEGIN TRANSACTION
	UPDATE [dbo].[Queue]
	SET Cancelled = 1
	WHERE ExperimentId = @ExperimentId and SbName = @SbName and Status = @Status
	IF (@@error > 0)
		GOTO on_error
COMMIT TRANSACTION	
RETURN
	on_error: 
	ROLLBACK TRANSACTION
RETURN
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE RetrieveQueue
	@ExperimentId int,
	@SbName varchar(256)
AS
	SELECT *
	FROM [dbo].[Queue]
	WHERE ExperimentId = @ExperimentId and SbName = @SbName
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE RetrieveQueueAllWithStatus
	@Status varchar(16)
AS
	SELECT *
	FROM [dbo].[Queue]
	WHERE Status = @Status
	ORDER BY PriorityHint DESC
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE RetrieveQueueCountWithStatus
	@Status varchar(16)
AS
	SELECT COUNT(*)
	FROM [dbo].[Queue]
	WHERE Status = @Status
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE StoreResults
	@ExperimentId int,
	@SbName varchar(256),
	@UserGroup varchar(256),
	@PriorityHint int,
	@Status varchar(16),
	@XmlExperimentResult varchar(max),
	@XmlResultExtension varchar(2048),
	@XmlBlobExtension varchar(2048),
	@WarningMessages varchar(2048),
	@ErrorMessage varchar(2048) 
AS
BEGIN TRANSACTION
	INSERT INTO [dbo].[Results] (ExperimentId, SbName, UserGroup, PriorityHint, Status, XmlExperimentResult, XmlResultExtension, XmlBlobExtension, WarningMessages, ErrorMessage, Notified)
	VALUES (@ExperimentId, @SbName, @UserGroup, @PriorityHint, @Status, @XmlExperimentResult, @XmlResultExtension, @XmlBlobExtension, @WarningMessages, @ErrorMessage, 0)
	IF (@@error > 0)
		GOTO on_error
COMMIT TRANSACTION	
RETURN
	on_error: 
	ROLLBACK TRANSACTION
RETURN
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE UpdateResultsNotified
	@ExperimentId int,
	@SbName varchar(256)
AS
BEGIN TRANSACTION
	UPDATE [dbo].[Results]
	SET Notified = 1
	WHERE ExperimentId = @ExperimentId and SbName = @SbName
	IF (@@error > 0)
		GOTO on_error
COMMIT TRANSACTION	
RETURN
	on_error: 
	ROLLBACK TRANSACTION
RETURN
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE RetrieveResults
	@ExperimentId int,
	@SbName varchar(256)
AS
	SELECT *
	FROM [dbo].[Results]
	WHERE ExperimentId = @ExperimentId and SbName = @SbName
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE RetrieveResultsAllNotNotified
AS
	SELECT *
	FROM [dbo].[Results]
	WHERE Notified = 0
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE RetrieveResultsAll
AS
	SELECT *
	FROM [dbo].[Results]
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE StoreStatisticsSubmitted
	@ExperimentId int,
	@SbName varchar(256),
	@UserGroup varchar(256),
	@PriorityHint int,
	@EstimatedExecTime int,
	@TimeSubmitted DateTime,
	@QueueLength int,
	@EstimatedWaitTime int
AS
BEGIN TRANSACTION
	INSERT INTO [dbo].[Statistics] (ExperimentId, SbName, UserGroup, PriorityHint, EstimatedExecTime, TimeSubmitted, QueueLength, EstimatedWaitTime)
	VALUES (@ExperimentId, @SbName, @UserGroup, @PriorityHint, @EstimatedExecTime, @TimeSubmitted, @QueueLength, @EstimatedWaitTime)
	IF (@@error > 0)
		GOTO on_error
COMMIT TRANSACTION	
RETURN
	on_error: 
	ROLLBACK TRANSACTION
RETURN
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE UpdateStatisticsStarted
	@ExperimentId int,
	@SbName varchar(256),
	@UnitId int,
	@TimeStarted DateTime
AS
BEGIN TRANSACTION
	UPDATE [dbo].[Statistics]
	SET UnitId = @UnitId, TimeStarted = @TimeStarted
	WHERE ExperimentId = @ExperimentId and SbName = @SbName
	IF (@@error > 0)
		GOTO on_error
COMMIT TRANSACTION	
RETURN
	on_error: 
	ROLLBACK TRANSACTION
RETURN
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE UpdateStatisticsCompleted
	@ExperimentId int,
	@SbName varchar(256),
	@TimeCompleted DateTime
AS
BEGIN TRANSACTION
	UPDATE [dbo].[Statistics]
	SET TimeCompleted = @TimeCompleted, Cancelled = 0
	WHERE ExperimentId = @ExperimentId and SbName = @SbName
	IF (@@error > 0)
		GOTO on_error
COMMIT TRANSACTION	
RETURN
	on_error: 
	ROLLBACK TRANSACTION
RETURN
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE UpdateStatisticsCancelled
	@ExperimentId int,
	@SbName varchar(256),
	@TimeCompleted DateTime
AS
BEGIN TRANSACTION
	UPDATE [dbo].[Statistics]
	SET TimeCompleted = @TimeCompleted, Cancelled = 1
	WHERE ExperimentId = @ExperimentId and SbName = @SbName
	IF (@@error > 0)
		GOTO on_error
COMMIT TRANSACTION	
RETURN
	on_error: 
	ROLLBACK TRANSACTION
RETURN
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE RetrieveAllStatistics
AS
	SELECT *
	FROM [dbo].[Statistics]
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE RetrieveStatisticsBySbName
	@SbName varchar(256)
AS
	SELECT *
	FROM [dbo].[Statistics]
	WHERE SbName = @SbName
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE RetrieveStatisticsByUserGroup
	@UserGroup varchar(256)
AS
	SELECT *
	FROM [dbo].[Statistics]
	WHERE UserGroup = @UserGroup
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE DeleteServiceBroker
	@Name varchar(32)
AS
	DELETE
	FROM [dbo].[ServiceBrokers]
	WHERE Name = @Name
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE RetrieveServiceBroker
	@Guid varchar(64)
AS
	SELECT *
	FROM [dbo].[ServiceBrokers]
	WHERE Guid = @Guid
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE RetrieveServiceBrokerAll
AS
	SELECT *
	FROM [dbo].[ServiceBrokers]
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE StoreServiceBroker
	@Name varchar(32),
	@Guid varchar(64),
	@OutgoingPasskey varchar(64),
	@IncomingPasskey varchar(64),
	@WebServiceUrl varchar(256),
	@IsAllowed bit
AS
BEGIN TRANSACTION
	INSERT INTO [dbo].[ServiceBrokers] (Name, Guid, OutgoingPasskey, IncomingPasskey, WebServiceUrl, IsAllowed)
	VALUES (@Name, @Guid, @OutgoingPasskey, @IncomingPasskey, @WebServiceUrl, @IsAllowed)
	IF (@@error > 0)
		GOTO on_error
COMMIT TRANSACTION	
RETURN
	on_error: 
	ROLLBACK TRANSACTION
RETURN
GO

/*********************************************************************************************************************/

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE UpdateServiceBroker
	@Name varchar(32),
	@Guid varchar(64),
	@OutgoingPasskey varchar(64),
	@IncomingPasskey varchar(64),
	@WebServiceUrl varchar(256),
	@IsAllowed bit
AS
BEGIN TRANSACTION
	UPDATE [dbo].[ServiceBrokers]
	SET Guid = @Guid, OutgoingPasskey = @OutgoingPasskey, IncomingPasskey = @IncomingPasskey, WebServiceUrl = @WebServiceUrl, IsAllowed = @IsAllowed
	WHERE Name = @Name
	IF (@@error > 0)
		GOTO on_error
COMMIT TRANSACTION	
RETURN
	on_error: 
	ROLLBACK TRANSACTION
RETURN
GO

-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE stpProcessDirtyExceptions
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here

	-- Aggregate queue exceptions by the given conditions and move that data to a temporary table, adding an exception repeat counter and choosing the exception ID in the process.
	SELECT * INTO #tmp
	FROM
	(
	SELECT MAX(queue.ExceptionId) as ExceptionId, Tenent, Environment, AppName, [Status], lastTime as LastTimeOfException, count as Counter, MAX(DirtyException_ExceptionId) as Exception_ExceptionId, DirtyException_Order
	FROM DirtyQueueExceptions as queue join 
	(SELECT COUNT(queue.ExceptionId) as count, MAX(queue.TimeOfException) as lastTime
	FROM DirtyQueueExceptions as queue join DirtyTransformExceptions as transform on queue.ExceptionId = transform.ExceptionId
	WHERE transform.[Order] = 0
	GROUP BY queue.Tenent, queue.Environment, queue.AppName, queue.[Status], transform.StackTrace)namedGroup on TimeOfException = namedGroup.lastTime
	GROUP BY Tenent, Environment, AppName, [Status], count, lastTime, DirtyException_Order
	)queueExceptions

	-- Copy exceptions which are already present in clean table to another temporary table that contains exception ID's that should be updated instead of new insertions.
	SELECT temp.ExceptionId as TempId, queue.ExceptionId as ExceptionId, temp.[Counter] as newCount, temp.LastTimeOfException as lastTime INTO #queueExceptionsToUpdate
	FROM #tmp as temp join DirtyTransformExceptions as dirty on temp.ExceptionId = dirty.ExceptionId
	join QueueExceptions as queue on (temp.Tenent = queue.Tenent and temp.Environment = queue.Environment and temp.AppName = queue.AppName and temp.[Status] = queue.[Status])
	join TransformExceptions as transform on queue.ExceptionId = transform.ExceptionId
	WHERE dirty.StackTrace = transform.StackTrace


	SELECT * FROM #tmp
	SELECT * FROM #queueExceptionsToUpdate
	-- Delete exceptions from first table that have same ID's in both temporary tables, so that the first one contains data to insert to clean table, and the second contains data to update in the clean table.
	DELETE FROM #tmp WHERE ExceptionId IN (SELECT TempId FROM #queueExceptionsToUpdate)

	INSERT INTO TransformExceptions
	SELECT * FROM DirtyTransformExceptions
	WHERE DirtyTransformExceptions.ExceptionId IN (SELECT ExceptionId FROM #tmp)
	
	UPDATE QueueExceptions SET [Counter] = [Counter] + newCount, LastTimeOfException = lastTime
	FROM #queueExceptionsToUpdate WHERE QueueExceptions.ExceptionId = #queueExceptionsToUpdate.ExceptionId  

	INSERT INTO QueueExceptions
	SELECT * FROM #tmp

	DROP TABLE #tmp
	DROP TABLE #queueExceptionsToUpdate

	INSERT INTO HistoricalQueueExceptions
	SELECT [Message], TimeOfException, queue.ExceptionId as ExceptionId FROM DirtyQueueExceptions as dirtyQueue join 
	DirtyTransformExceptions as dirtyTransform on dirtyQueue.ExceptionId = dirtyTransform.ExceptionId join
	(SELECT q.ExceptionId, Tenent, Environment, AppName, [Status], StackTrace from QueueExceptions as q 
	join TransformExceptions as t on q.ExceptionId = t.ExceptionId)queue
	ON queue.Tenent = dirtyQueue.Tenent and queue.Environment = dirtyQueue.Environment and queue.AppName = dirtyQueue.AppName and queue.[Status] = dirtyQueue.[Status] and queue.StackTrace = dirtyTransform.StackTrace

	DELETE FROM DirtyQueueExceptions
	DELETE FROM DirtyTransformExceptions

END
GO

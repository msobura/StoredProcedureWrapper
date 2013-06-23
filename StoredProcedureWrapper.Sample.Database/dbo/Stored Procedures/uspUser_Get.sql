CREATE PROCEDURE [dbo].[uspUser_Get]
    @userId int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [Id] = U.UserId,
        U.IsLocked,
        U.FirstName,
        U.MiddleName,
        U.LastName,
        U.Birthday
    FROM dbo.Users AS U
    WHERE U.UserId = @userId;
END
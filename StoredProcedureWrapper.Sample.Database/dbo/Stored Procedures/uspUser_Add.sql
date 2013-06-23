CREATE PROCEDURE [dbo].[uspUser_Add]
    @isLocked bit,    
    @firstName nvarchar(50),
    @middleName nvarchar(50),
    @lastName nvarchar(100),
    @birthday date,
    @userId int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Users (
        IsLocked,
        FirstName,
        MiddleName,
        LastName,
        Birthday
    )
    VALUES (
        @isLocked,
        @firstName,
        @middleName,
        @lastName,
        @birthday
    );
    
    SET @userId = CAST(SCOPE_IDENTITY() AS int);
END
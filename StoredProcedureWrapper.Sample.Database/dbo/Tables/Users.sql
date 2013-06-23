CREATE TABLE [dbo].[Users] (
    [UserId]     INT            IDENTITY (1, 1) NOT NULL,
    [IsLocked]   BIT            CONSTRAINT [DF_Users_IsLocked] DEFAULT ((0)) NOT NULL,
    [FirstName]  NVARCHAR (50)  NOT NULL,
    [MiddleName] NVARCHAR (50)  NOT NULL,
    [LastName]   NVARCHAR (100) NOT NULL,
    [Birthday]   DATE           NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId] ASC)
);


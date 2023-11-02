﻿CREATE TABLE [dbo].[Participants]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [ChatId] INT NOT NULL, 
    [UserId] [nvarchar](450) NOT NULL, 
    [RoleId] INT NOT NULL, 
    [JoinedAt] DATETIME NOT NULL, 
    CONSTRAINT [FK_Participants_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]), 
    CONSTRAINT [FK_Participants_Chats] FOREIGN KEY ([ChatId]) REFERENCES [Chats]([Id]), 
    CONSTRAINT [FK_Participants_Roles] FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id])
)
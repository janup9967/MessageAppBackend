CREATE PROCEDURE SendMessage
    @SenderId INT,
    @ReceiverId INT,
    @ConversationId INT,
    @Content NVARCHAR(MAX),
    @Time DATETIME,
    @IsRead BIT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Messages (SenderId, ReceiveId, ConversationId, Content, Time, IsRead)
    VALUES (@SenderId, @ReceiverId, @ConversationId, @Content, @Time, @IsRead);

    SELECT * FROM Messages WHERE Id = SCOPE_IDENTITY();
END



-- Conversation Stored Procedures

CREATE PROCEDURE CreateConversation
    @CreatedByUser INT,
    @ReceiveId INT,
    @CreatedAt DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Conversations (CreatedByUser, ReceiveId, CreatedAt)
    VALUES (@CreatedByUser, @ReceiveId, @CreatedAt);

    SELECT * FROM Conversations WHERE Id = SCOPE_IDENTITY();
END

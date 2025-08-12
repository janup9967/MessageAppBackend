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


CREATE PROCEDURE CheckConversationExists
    @User1Id INT,
    @User2Id INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM Conversations
        WHERE (CreatedByUser = @User1Id AND ReceiveId = @User2Id)
           OR (CreatedByUser = @User2Id AND ReceiveId = @User1Id)
    )
        SELECT CAST(1 AS BIT) AS [Exists];
    ELSE
        SELECT CAST(0 AS BIT) AS [Exists];
END


CREATE PROCEDURE GetConversationsForUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.Id,
        CASE 
            WHEN c.CreatedByUser = @UserId THEN u1.Username
            ELSE u2.Username
        END AS SenderUsername,
        CASE 
            WHEN c.ReceiveId = @UserId THEN u1.Username
            ELSE u2.Username
        END AS ReceiverUsername,
        c.CreatedAt,
        m.Content AS LastMessageContent,
        m.Time AS LastMessageTime,
        m.IsRead AS LastMessageIsRead
    FROM Conversations c
    INNER JOIN Users u1 ON u1.Id = c.CreatedByUser
    INNER JOIN Users u2 ON u2.Id = c.ReceiveId
    OUTER APPLY (
        SELECT TOP 1 Content, Time, IsRead
        FROM Messages
        WHERE ConversationId = c.Id
        ORDER BY Time DESC
    ) m
    WHERE c.CreatedByUser = @UserId OR c.ReceiveId = @UserId
    ORDER BY m.Time DESC;
END



--  Read Message
CREATE PROCEDURE MarkMessageAsRead
    @MessageId INT
AS
BEGIN
    UPDATE Messages
    SET IsRead = 1
    WHERE Id = @MessageId;
    
    SELECT * FROM Messages WHERE Id = @MessageId;
END

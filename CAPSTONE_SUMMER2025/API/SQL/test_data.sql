-- Insert test accounts
INSERT INTO Account (Email, Password, Role, Status) VALUES
('user1@test.com', 'password123', 'USER', 'ACTIVE'),
('user2@test.com', 'password123', 'USER', 'ACTIVE'),
('user3@test.com', 'password123', 'USER', 'ACTIVE'),
('user4@test.com', 'password123', 'USER', 'ACTIVE');

-- Insert account profiles
INSERT INTO AccountProfile (Account_ID, FirstName, LastName, AvatarUrl) VALUES
(1, 'John', 'Doe', 'https://example.com/avatar1.jpg'),
(2, 'Jane', 'Smith', 'https://example.com/avatar2.jpg'),
(3, 'Mike', 'Johnson', 'https://example.com/avatar3.jpg'),
(4, 'Sarah', 'Williams', 'https://example.com/avatar4.jpg');

-- Insert posts
INSERT INTO Post (Account_ID, Content, CreateAt) VALUES
(1, 'Hello everyone! This is my first post.', GETDATE()),
(2, 'Just finished a great project!', DATEADD(HOUR, -2, GETDATE())),
(3, 'Looking for collaborators for my startup.', DATEADD(HOUR, -4, GETDATE())),
(4, 'Excited to share my new idea!', DATEADD(HOUR, -6, GETDATE())),
(1, 'Another post from John', DATEADD(HOUR, -8, GETDATE())),
(2, 'Tech news: AI is advancing rapidly', DATEADD(HOUR, -10, GETDATE()));

-- Insert follows (user1 follows user2 and user3)
INSERT INTO Follow (Follower_Account_ID, Following_Account_ID, FollowDate) VALUES
(1, 2, GETDATE()),
(1, 3, GETDATE());

-- Insert likes
INSERT INTO PostLike (Post_ID, Account_ID, LikedAt) VALUES
(1, 2, GETDATE()),
(1, 3, GETDATE()),
(2, 1, GETDATE()),
(2, 4, GETDATE()),
(3, 1, GETDATE()),
(3, 2, GETDATE());

-- Insert comments
INSERT INTO PostComment (Post_ID, Account_ID, Content, CommentAt) VALUES
(1, 2, 'Great post!', GETDATE()),
(1, 3, 'Welcome!', GETDATE()),
(2, 1, 'Congratulations!', GETDATE()),
(3, 4, 'I might be interested!', GETDATE());

-- Insert a block (user1 blocks user4)
INSERT INTO AccountBlock (Blocker_Account_ID, Blocked_Account_ID, BlockedAt) VALUES
(1, 4, GETDATE()); 
INSERT INTO users (username, email, password_hash)
VALUES
    ('Chase', 'Chase@squid.inc', 'development-placeholder'),
    ('Kyle', 'Kyle@squid.inc', 'development-placeholder'),
    ('Tyler', 'Tyler@squid.inc', 'development-placeholder'),
    ('Cira', 'Cira@squid.inc', 'development-placeholder'),
    ('Jacob', 'Jacob@squid.inc', 'development-placeholder'),
    ('Jordan', 'Jordan@squid.inc', 'development-placeholder'),
    ('James', 'James@squid.inc', 'development-placeholder'),
    ('Trey', 'Trey@squid.inc', 'development-placeholder'),
    ('Kate', 'Kate@squid.inc', 'development-placeholder'),
    ('Alton', 'Alton@squid.inc', 'development-placeholder');

INSERT INTO teams (name)
VALUES ('Squid Squad');

INSERT INTO team_members (team_id, user_id, role)
VALUES
    (1, 1, 4), -- Chase = Owner
    (1, 2, 3), -- Kyle = TeamLead
    (1, 3, 2), -- Tyler = Editor
    (1, 4, 2), -- Cira = Editor
    (1, 5, 2), -- Jacob = Editor
    (1, 6, 2), -- Jordan = Editor
    (1, 7, 1), -- James = Member
    (1, 8, 1), -- Trey = Member
    (1, 9, 1), -- Kate = Member
    (1, 10, 1); -- Alton = Member



INSERT INTO notes (team_id, author_id, title, content)
VALUES
    (1, 1, 'Welcome', 'Welcome to Squid Inc.!');